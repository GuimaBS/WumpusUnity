using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgenteReativo : MonoBehaviour
{
    private TileManager tileManager;
    private LogManager logManager;
    private PontuacaoManager pontuacaoManager;
    public gerarCSV loggerCSV;

    private Slider velocidadeSlider;
    public System.Action onMorte;

    [Header("Configurações de Spawn")]
    public bool autoSpawn = false;
    private bool agenteVivo = false;

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

        if (!loggerCSV)
        {
            loggerCSV = FindFirstObjectByType<gerarCSV>();
            if (!loggerCSV) Debug.LogError("Não encontrou instância de gerarCSV!");
        }

        if (autoSpawn)
            SpawnAgente();
    }

    public void SpawnAgente()
    {
        if (agenteVivo) return;

        int yMax = GridGenerator.tamanhoY - 1;
        posicaoAtual = new Vector2Int(0, yMax);
        transform.position = new Vector3(posicaoAtual.x * 1.7f, transform.position.y, posicaoAtual.y * 1.7f);

        logManager.AdicionarLog("<color=white><b>Agente 1 foi gerado no mapa.</b></color>");
        loggerCSV?.RegistrarEvento("Spawn", transform.position, "Agente1");

        StartCoroutine(ComportamentoReativo());
        agenteVivo = true;
    }

    public void EncerrarAgente()
    {
        StopAllCoroutines();
        logManager.AdicionarLog("<color=red><b>Agente 1 foi encerrado manualmente.</b></color>");
        loggerCSV?.RegistrarEvento("EncerrarManual", transform.position, "Agente1");

        SistemaDePontuacao.instancia?.AdicionarDerrota();
        pontuacaoManager.AlterarPontuacao(-500);
        Destroy(gameObject);
        agenteVivo = false;
    }

    private IEnumerator ComportamentoReativo()
    {
        while (true)
        {
            Vector2Int proximaTile = EscolherProximaTile();

            yield return StartCoroutine(MoverPara(proximaTile));
            posicaoAtual = proximaTile;

            pontuacaoManager.AlterarPontuacao(-1);

            var info = tileManager.ObterInfoDaTile(posicaoAtual);
            if (info != null)
            {
                // Percepções (apenas log)
                if (info.temBrisa)
                    logManager.AdicionarLog("<color=lightblue>O Agente sentiu brisa....</color>");
                if (info.temFedor)
                    logManager.AdicionarLog("<color=green>O Agente sentiu um fedor...</color>");
                if (info.temOuro)
                    logManager.AdicionarLog("<color=yellow>O Agente percebeu brilho...</color>");
            }

            // Verificar morte por poço
            if (info != null && info.temPoco)
            {
                Morrer("<color=red><b>O Agente caiu em um poço!</b></color>", "MortePoco");
                yield break;
            }

            // Verificar morte por Wumpus
            if (VerificarContatoDiretoComWumpus() || GridGenerator.posicoesWumpus.Contains(posicaoAtual))
            {
                Morrer("<color=red><b>O Agente foi morto pelo Wumpus!</b></color>", "MorteWumpus");
                yield break;
            }

            if (velocidadeSlider != null)
                velocidade = Mathf.Lerp(0.1f, 2f, velocidadeSlider.value);

            yield return new WaitForSeconds(velocidade);

            // Escolher ação aleatória
            string[] acoes = { "atirar", "pegar", "voltar" };
            string acao = acoes[Random.Range(0, acoes.Length)];

            if (acao == "atirar")
            {
                bool acertou = TentarAtirar();
                if (acertou)
                {
                    loggerCSV?.RegistrarEvento("AtirarAcerto", transform.position, "Agente1");
                    logManager.AdicionarLog("<color=orange><b>Wumpus abatido!</b></color>");
                    pontuacaoManager.AlterarPontuacao(+1000);
                    SistemaDePontuacao.instancia?.AdicionarVitoria();
                }
                else
                {
                    loggerCSV?.RegistrarEvento("AtirarErro", transform.position, "Agente1");
                    logManager.AdicionarLog("<color=purple><b>O Agente atirou aleatoriamente e errou!</b></color>");
                    pontuacaoManager.AlterarPontuacao(-100);
                }
            }

            else if (acao == "pegar")
            {
                if (TentarPegarOuro())
                {
                    loggerCSV?.RegistrarEvento("PegarOuro", transform.position, "Agente1");
                    logManager.AdicionarLog("<color=yellow><b>Ouro coletado!</b></color>");
                    pontuacaoManager.AlterarPontuacao(+1000);
                    SistemaDePontuacao.instancia?.AdicionarVitoria();
                }
                else
                {
                    loggerCSV?.RegistrarEvento("PegarFalhou", transform.position, "Agente1");
                    logManager.AdicionarLog("<color=purple><b>O Agente tentou pegar ouro, mas não havia nada.</b></color>");
                    pontuacaoManager.AlterarPontuacao(-100);
                }
            }

            else if (acao == "voltar")
            {
                loggerCSV?.RegistrarEvento("Voltar", transform.position, "Agente1");
                logManager.AdicionarLog("<color=gray>O Agente parou e não fez nada.</color>");
                pontuacaoManager.AlterarPontuacao(-1);
            }
        }
    }

    private Vector2Int EscolherProximaTile()
    {
        List<Vector2Int> candidatos = new List<Vector2Int>();

        foreach (Vector2Int dir in Direcoes())
        {
            Vector2Int vizinho = posicaoAtual + dir;
            if (tileManager.ObterTileEm(vizinho) != null)
                candidatos.Add(vizinho);
        }

        if (candidatos.Count > 0)
            return candidatos[Random.Range(0, candidatos.Count)];

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

    private bool TentarAtirar()
    {
        Vector3[] direcoes = {
            Vector3.forward * 1.7f,
            Vector3.back * 1.7f,
            Vector3.left * 1.7f,
            Vector3.right * 1.7f
        };

        Vector3 dir = direcoes[Random.Range(0, direcoes.Length)];
        Vector3 alvo = transform.position + dir;

        Collider[] alvos = Physics.OverlapSphere(alvo, 0.3f);
        foreach (var col in alvos)
        {
            if (col.CompareTag("wumpus"))
            {
                Destroy(col.gameObject);

                Vector2Int posWumpus = posicaoAtual + new Vector2Int(
                    Mathf.RoundToInt(dir.x / 1.7f),
                    Mathf.RoundToInt(dir.z / 1.7f)
                );

                GridGenerator.EliminarWumpusNaPosicao(posWumpus);
                return true;
            }
        }

        return false;
    }

    private bool TentarPegarOuro()
    {
        var info = tileManager.ObterInfoDaTile(posicaoAtual);
        if (info != null && info.temOuro)
        {
            info.temOuro = false;
            GridGenerator.ColetarOuroNaPosicao(posicaoAtual);

            GameObject[] brilhos = GameObject.FindGameObjectsWithTag("brilho");
            foreach (GameObject brilho in brilhos)
            {
                Vector3 brilhoPos = new Vector3(posicaoAtual.x * 1.7f, 0.5f, posicaoAtual.y * 1.7f);
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

    private void Morrer(string mensagem, string eventoCSV)
    {
        logManager.AdicionarLog(mensagem);
        loggerCSV?.RegistrarEvento(eventoCSV, transform.position, "Agente1");

        pontuacaoManager.AlterarPontuacao(-1000);
        SistemaDePontuacao.instancia?.AdicionarDerrota();
        onMorte?.Invoke();
        Destroy(gameObject);
        agenteVivo = false;
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
