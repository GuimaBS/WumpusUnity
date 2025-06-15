using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgenteReativo : MonoBehaviour
{
    private TileManager tileManager;
    private LogManager logManager;
    private PontuacaoManager pontuacaoManager;
    private Slider velocidadeSlider;

    public System.Action onMorte;

    private float velocidade = 1f;
    private Vector2Int posicaoAtual;

    private void Start()
    {
        tileManager = TileManager.instancia;
        logManager = LogManager.instancia;
        pontuacaoManager = PontuacaoManager.instancia;

        pontuacaoManager.AlterarPontuacao(0);

        GameObject sliderObj = GameObject.FindWithTag("VelocidadeSlider");
        if (sliderObj != null)
            velocidadeSlider = sliderObj.GetComponent<Slider>();

        posicaoAtual = new Vector2Int(0, 0);
        transform.position = new Vector3(posicaoAtual.x * 1.7f, transform.position.y, posicaoAtual.y * 1.7f);

        StartCoroutine(ComportamentoIA());
    }

    private IEnumerator ComportamentoIA()
    {
        while (true)
        {
            Vector2Int proximaTile = EscolherProximaTile();

            yield return StartCoroutine(MoverPara(proximaTile));
            posicaoAtual = proximaTile;

            pontuacaoManager.AlterarPontuacao(-1);

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
        List<Vector2Int> direcoes = Direcoes();
        Vector2Int direcao = direcoes[Random.Range(0, direcoes.Count)];
        Vector2Int destino = posicaoAtual + direcao;

        if (tileManager.ObterTileEm(destino) != null)
            return destino;

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
            Morrer("<color=red><b>O Agente 1 caiu em um poço!</b></color>");
            return;
        }

        if (VerificarContatoDiretoComWumpus() || GridGenerator.posicoesWumpus.Contains(posicaoAtual))
        {
            Morrer("<color=red><b>O Agente 1 foi morto pelo Wumpus!</b></color>");
            return;
        }

        if (info.temBrisa)
        {
            logManager.AdicionarLog("<color=lightblue>O Agente 1 sentiu brisa....</color>");
        }

        if (info.temFedor)
        {
            logManager.AdicionarLog("<color=green>O Agente 1 sentiu um fedor...</color>");

            if (TentarAtirar(posicaoAtual))
            {
                logManager.AdicionarLog("<color=orange><b>Wumpus abatido!</b></color>");
                pontuacaoManager.AlterarPontuacao(+1000);
                SistemaDePontuacao.instancia?.AdicionarVitoria();
            }
            else
            {
                logManager.AdicionarLog("<color=orange>O Agente 1 errou o alvo!</color>");
                pontuacaoManager.AlterarPontuacao(-100);
            }
        }

        if (info.temOuro && !GridGenerator.ouroColetado)
        {
            logManager.AdicionarLog("<color=yellow>O Agente 1 detectou brilho!</color>");
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
        Vector3 direcao = transform.forward;
        Vector3 origemMundo = new Vector3(origem.x * 1.7f, 0.5f, origem.y * 1.7f);
        Vector3 destino = origemMundo + direcao * 1.7f;

        Vector2Int direcaoLogica = new Vector2Int(Mathf.RoundToInt(direcao.x), Mathf.RoundToInt(direcao.z));
        Vector2Int posAlvo = origem + direcaoLogica;

        if (GridGenerator.posicoesWumpus.Contains(posAlvo))
        {
            GridGenerator.EliminarWumpusNaPosicao(posAlvo);
            return true;
        }

        return false;
    }

    private bool PegarOuro(Vector2Int posicao)
    {
        Collider[] objetos = Physics.OverlapSphere(new Vector3(posicao.x * 1.7f, 0.5f, posicao.y * 1.7f), 0.5f);
        foreach (var obj in objetos)
        {
            if (obj.CompareTag("brilho"))
            {
                Destroy(obj.gameObject);
                GridGenerator.ColetarOuroNaPosicao(posicao);
                return true;
            }
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
