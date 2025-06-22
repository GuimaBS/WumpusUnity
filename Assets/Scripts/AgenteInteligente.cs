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
    public GameObject hitEffectPrefab;
    public GameObject pickEffectPrefab;

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

        int yMax = GridGenerator.tamanhoY - 1;
        posicaoAtual = new Vector2Int(0, yMax);
        transform.position = new Vector3(posicaoAtual.x * 1.7f, transform.position.y, posicaoAtual.y * 1.7f);

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

            // Logs de percepção ao entrar na tile
            var info = tileManager.ObterInfoDaTile(posicaoAtual);
            if (info != null)
            {
                if (info.temBrisa)
                    logManager.AdicionarLog("<color=lightblue>O Agente 2 sentiu brisa...</color>");
                if (info.temFedor)
                    logManager.AdicionarLog("<color=green>O Agente 2 sentiu um fedor...</color>");
                if (info.temOuro)
                    logManager.AdicionarLog("<color=yellow>O Agente 2 percebeu o brilho...</color>");
            }

            // Verificar morte por poço
            if (VerificarSeCaiuEmPoco())
                yield break;

            if (!visitadas.Contains(posicaoAtual))
            {
                visitadas.Add(posicaoAtual);
                memoriaVisual.AtualizarTile(posicaoAtual, "vazio");
                pontuacaoManager.AlterarPontuacao(-1);
            }

            if (info != null)
                ProcessarPercepcao(info);

            velocidade = (velocidadeSlider != null) ? Mathf.Max(0.1f, velocidadeSlider.value) : 1f;

            yield return new WaitForSeconds(velocidade);
        }
    }

    private bool VerificarSeCaiuEmPoco()
    {
        if (tileManager.PocoNaPosicao(posicaoAtual))
        {
            TileP tileP = tileManager.ObterTilePNaPosicao(posicaoAtual);
            if (tileP != null)
                tileP.AtivarFantasma();

            memoriaVisual.AtualizarTile(posicaoAtual, "poco");

            logManager.AdicionarLog("<color=red><b>O Agente 2 caiu em um poço e morreu!</b></color>");
            pontuacaoManager.AlterarPontuacao(-1000);
            SistemaDePontuacao.instancia?.AdicionarDerrota();
            onMorte?.Invoke();
            Destroy(gameObject);
            return true;
        }
        return false;
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
        if (VerificarContatoDiretoComWumpus() || GridGenerator.posicoesWumpus.Contains(posicaoAtual))
        {
            memoriaVisual.AtualizarTile(posicaoAtual, "wumpus");
            Morrer("<color=red><b>O Agente 2 foi morto pelo Wumpus!</b></color>");
            return;
        }

        if (info.temBrisa)
            memoriaVisual.AtualizarTile(posicaoAtual, "brisa");

        if (info.temFedor)
        {
            memoriaVisual.AtualizarTile(posicaoAtual, "fedor");

            if (TentarAtirar(posicaoAtual))
            {
                logManager.AdicionarLog("<color=purple><b>O Agente 2 atirou a flecha e matou o Wumpus!</b></color>");
                pontuacaoManager.AlterarPontuacao(+1000);
                SistemaDePontuacao.instancia?.AdicionarVitoria();
            }
            else
            {
                logManager.AdicionarLog("<color=purple>O Agente 2 errou a flecha!</color>");
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
        memoriaVisual.AtualizarTile(posicaoAtual, "wumpus");

        logManager.AdicionarLog(mensagem);
        pontuacaoManager.AlterarPontuacao(-1000);
        SistemaDePontuacao.instancia?.AdicionarDerrota();
        onMorte?.Invoke();
        Destroy(gameObject);
    }

    private bool VerificarContatoDiretoComWumpus()
    {
        Collider[] colisores = Physics.OverlapSphere(transform.position, 0.4f);
        foreach (var col in colisores)
        {
            if (col.CompareTag("wumpus"))
                return true;
        }
        return false;
    }

    private bool TentarAtirar(Vector2Int origem)
    {
        Vector2Int direcaoLogica = DirecaoPelaRotacao();
        Vector2Int posAlvo = origem + direcaoLogica;

        if (GridGenerator.posicoesWumpus.Contains(posAlvo))
        {
            Vector3 posicaoMundo = new Vector3(posAlvo.x * 1.7f, 0.5f, posAlvo.y * 1.7f);
            Instantiate(hitEffectPrefab, posicaoMundo + Vector3.up * 1f, Quaternion.identity);

            GridGenerator.EliminarWumpusNaPosicao(posAlvo);

            memoriaVisual.AtualizarTile(posAlvo, "vazio");

            return true;
        }

        return false;
    }

    private Vector2Int DirecaoPelaRotacao()
    {
        Vector3 forward = transform.forward;
        if (Mathf.Abs(forward.x) > Mathf.Abs(forward.z))
        {
            return (forward.x > 0) ? new Vector2Int(1, 0) : new Vector2Int(-1, 0);
        }
        else
        {
            return (forward.z > 0) ? new Vector2Int(0, 1) : new Vector2Int(0, -1);
        }
    }

    private bool PegarOuro(Vector2Int posicao)
    {
        var info = TileManager.instancia.ObterInfoDaTile(posicao);

        if (info != null && info.temOuro)
        {
            info.temOuro = false;
            GridGenerator.ColetarOuroNaPosicao(posicao);

            Vector3 posicaoMundo = new Vector3(posicao.x * 1.7f, 0.5f, posicao.y * 1.7f);
            Instantiate(pickEffectPrefab, posicaoMundo + Vector3.up * 0.5f, Quaternion.identity);

            GameObject[] brilhos = GameObject.FindGameObjectsWithTag("brilho");
            foreach (GameObject brilho in brilhos)
            {
                Vector3 brilhoPos = new Vector3(posicao.x * 1.7f, 0.5f, posicao.y * 1.7f);
                if (Vector3.Distance(brilho.transform.position, brilhoPos) < 0.8f)
                {
                    Destroy(brilho);
                    break;
                }
            }

            return true;
        }

        return false;
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
