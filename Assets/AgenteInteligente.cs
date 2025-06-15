using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgenteInteligente : MonoBehaviour
{
    private TileManager tileManager;
    private MemoriaVisual memoriaVisual;
    private LogManager logManager;
    private PontuacaoManager pontuacaoManager;
    private Slider velocidadeSlider;

    public System.Action onMorte;

    private float velocidade = 1f;
    private Vector2Int posicaoAtual;
    private HashSet<Vector2Int> direcoesComFalha = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> visitadas = new HashSet<Vector2Int>();

    private void Start()
    {
        tileManager = TileManager.instancia;
        memoriaVisual = MemoriaVisual.instancia;
        logManager = LogManager.instancia;
        pontuacaoManager = PontuacaoManager.instancia;

        pontuacaoManager.AlterarPontuacao(0);

        GameObject sliderObj = GameObject.FindWithTag("VelocidadeSlider");
        if (sliderObj != null)
            velocidadeSlider = sliderObj.GetComponent<Slider>();

        posicaoAtual = PegarPosicaoAtual();
        visitadas.Add(posicaoAtual);
        memoriaVisual.AtualizarTile(posicaoAtual, "vazio");

        StartCoroutine(ComportamentoIA());
    }

    private IEnumerator ComportamentoIA()
    {
        while (true)
        {
            Vector2Int proximaTile = EscolherProximaTile();

            yield return StartCoroutine(MoverPara(proximaTile));
            posicaoAtual = proximaTile;

            if (!visitadas.Contains(posicaoAtual))
            {
                visitadas.Add(posicaoAtual);
                memoriaVisual.AtualizarTile(posicaoAtual, "vazio");
                pontuacaoManager.AlterarPontuacao(-1);
            }

            var info = tileManager.ObterInfoDaTile(posicaoAtual);
            if (info != null)
                ProcessarPercepcao(info);

            if (velocidadeSlider != null)
                velocidade = Mathf.Lerp(0.1f, 2f, velocidadeSlider.value);

            yield return new WaitForSeconds(velocidade);
        }
    }

    private Vector2Int EscolherProximaTile()
    {
        List<Vector2Int> candidatos = new List<Vector2Int>();

        foreach (Vector2Int dir in Direcoes())
        {
            Vector2Int vizinho = posicaoAtual + dir;

            if (!visitadas.Contains(vizinho) &&
                tileManager.ObterTileEm(vizinho) != null &&
                memoriaVisual.TilePareceSegura(vizinho))
            {
                candidatos.Add(vizinho);
            }
        }

        if (candidatos.Count > 0)
            return candidatos[Random.Range(0, candidatos.Count)];

        foreach (Vector2Int dir in Direcoes())
        {
            Vector2Int vizinho = posicaoAtual + dir;
            if (tileManager.ObterTileEm(vizinho) != null)
                return vizinho;
        }

        return posicaoAtual;
    }

    private IEnumerator MoverPara(Vector2Int destino)
    {
        Vector3 destinoMundo = new Vector3(destino.x * 1.7f, transform.position.y, destino.y * 1.7f);

        Vector3 direcao = destinoMundo - transform.position;
        if (direcao != Vector3.zero)
        {
            Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao.normalized);
            float tempoRotacao = 0.15f;
            float t = 0f;
            Quaternion rotacaoInicial = transform.rotation;

            while (t < 1f)
            {
                t += Time.deltaTime / tempoRotacao;
                transform.rotation = Quaternion.Slerp(rotacaoInicial, rotacaoAlvo, t);
                yield return null;
            }
        }

        transform.position = destinoMundo;
        yield break;
    }

    private void ProcessarPercepcao(TileManager.TileInfo info)
    {
        if (info.temPoco)
        {
            memoriaVisual.AtualizarTile(posicaoAtual, "poco");
            Morrer("<color=red><b>O Agente 2 caiu em um poço!</b></color>");
            return;
        }

        if (GridGenerator.wumpusVivo &&
            (VerificarContatoDiretoComWumpus() || posicaoAtual == GridGenerator.posicaoWumpus))
        {
            memoriaVisual.AtualizarTile(posicaoAtual, "wumpus");
            Morrer("<color=red><b>O Agente 2 foi morto pelo Wumpus!</b></color>");
            return;
        }

        if (info.temBrisa)
        {
            memoriaVisual.AtualizarTile(posicaoAtual, "brisa");
            logManager.AdicionarLog("<color=lightblue>O Agente 2 sentiu brisa....</color>");
        }

        if (info.temFedor)
        {
            memoriaVisual.AtualizarTile(posicaoAtual, "fedor");
            logManager.AdicionarLog("<color=green>O Agente 2 sentiu um fedor...</color>");
            if (TentarAtirar(posicaoAtual))
            {
                logManager.AdicionarLog("<color=purple><b>O Agente 2 atirou uma flecha e matou o Wumpus!</b></color>");
                pontuacaoManager.AlterarPontuacao(+1000);
                SistemaDePontuacao.instancia?.AdicionarVitoria();
            }
            else
            {
                logManager.AdicionarLog("<color=purple>O Agente 2 errou o alvo!</color>");
                pontuacaoManager.AlterarPontuacao(-100);
                direcoesComFalha.Add(posicaoAtual);
            }
        }

        if (info.temOuro && !GridGenerator.ouroColetado)
        {
            memoriaVisual.AtualizarTile(posicaoAtual, "brilho");
            logManager.AdicionarLog("<color=yellow>O Agente 2 detectou brilho!</color>");
            if (PegarOuro(posicaoAtual))
            {
                logManager.AdicionarLog("<color=yellow><b>Ouro coletado!</b></color>");
                pontuacaoManager.AlterarPontuacao(+1000);
                SistemaDePontuacao.instancia?.AdicionarVitoria();
            }
        }
    }

    private void Morrer(string mensagem)
    {
        logManager.AdicionarLog(mensagem);
        pontuacaoManager.AlterarPontuacao(-1000);
        SistemaDePontuacao.instancia?.AdicionarDerrota();
        onMorte?.Invoke();
        Destroy(gameObject);
    }

    private bool VerificarContatoDiretoComWumpus()
    {
        if (!GridGenerator.wumpusVivo) return false;

        Collider[] colisores = Physics.OverlapSphere(transform.position, 0.6f);
        foreach (var col in colisores)
        {
            if (col.CompareTag("wumpus"))
                return true;
        }
        return false;
    }

    private bool TentarAtirar(Vector2Int origem)
    {
        Vector3 direcao = transform.forward;
        Vector3 origemMundo = new Vector3(origem.x * 1.7f, 0.5f, origem.y * 1.7f);
        Vector3 destino = origemMundo + direcao * 1.7f;

        Vector2Int direcaoLogica = new Vector2Int(Mathf.RoundToInt(direcao.x), Mathf.RoundToInt(direcao.z));
        Vector2Int posAlvo = origem + direcaoLogica;

        if (posAlvo == GridGenerator.posicaoWumpus)
        {
            GridGenerator.EliminarWumpus();
            return true;
        }

        return false;
    }

    private bool PegarOuro(Vector2Int posicao)
    {
        if (!GridGenerator.ouroColetado)
        {
            GridGenerator.ColetarOuroNaPosicao(posicao);
            return true;
        }
        return false;
    }

    private Vector2Int PegarPosicaoAtual()
    {
        return new Vector2Int(
            Mathf.RoundToInt(transform.position.x / 1.7f),
            Mathf.RoundToInt(transform.position.z / 1.7f)
        );
    }

    private List<Vector2Int> Direcoes()
    {
        return new List<Vector2Int>
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };
    }
}
