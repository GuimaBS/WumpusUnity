using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgenteReativo : MonoBehaviour
{
    private TileManager tileManager;
    private float velocidade = 1f;
    private Slider velocidadeSlider;
    public System.Action onMorte;
    private bool pegouOuro = false;


    private void Start()
    {
        tileManager = TileManager.instancia;
        PontuacaoManager.instancia.AlterarPontuacao(0); // inicia com 0 visível

        GameObject sliderObj = GameObject.FindWithTag("VelocidadeSlider");
        if (sliderObj != null)
        {
            velocidadeSlider = sliderObj.GetComponent<Slider>();
        }

        StartCoroutine(ComportamentoReativo());
    }

    private IEnumerator ComportamentoReativo()
    {
           // 1. Converter posição do agente para a posição lógica da tile
            Vector2Int posicaoAgente = new Vector2Int(
            Mathf.FloorToInt(transform.position.x / 1.7f),
            Mathf.FloorToInt(transform.position.z / 1.7f)
        );

        // 2. Comparar com a posição salva do Wumpus
        if (posicaoAgente == GridGenerator.posicaoWumpus)
        {
            Debug.Log("O agente foi morto pelo Wumpus.");
            onMorte?.Invoke();
            Destroy(gameObject);
            SistemaDePontuacao.instancia?.AdicionarDerrota();
            PontuacaoManager.instancia.AlterarPontuacao(-1000);


            yield break;

        }

        while (true)
        {
            if (velocidadeSlider != null)
                velocidade = velocidadeSlider.value;

            yield return new WaitForSeconds(velocidade);

            // Verifica se morreu encontrar o Wumpus
            Vector2Int posicaoAtual = new Vector2Int(
                Mathf.FloorToInt(transform.position.x / 1.7f),
                Mathf.FloorToInt(transform.position.z / 1.7f)
            );

            if (posicaoAtual == GridGenerator.posicaoWumpus)
            {
                Debug.Log("O agente foi morto pelo Wumpus!");
                onMorte?.Invoke();
                Destroy(gameObject);
                SistemaDePontuacao.instancia?.AdicionarDerrota();
                LogManager.instancia.AdicionarLog("<color=red> O Agente 1 foi morto pelo Wumpus!</color>");
                PontuacaoManager.instancia.AlterarPontuacao(-1000);


                yield break;
            }

            // Verifica se o agente voltou para casa com o ouro
            if (pegouOuro && posicaoAtual == new Vector2Int(0, 0))
            {
                Debug.Log("O agente retornou com o ouro! Vitória!");
                SistemaDePontuacao.instancia?.AdicionarVitoria();
                onMorte?.Invoke();
                Destroy(gameObject);
                PontuacaoManager.instancia.AlterarPontuacao(+2000);

                yield break;
            }

            Vector3 pos = transform.position;
            int tileX = Mathf.RoundToInt(pos.x / 1.7f);
            int tileY = Mathf.RoundToInt(pos.z / 1.7f);
            Vector2Int posTile = new Vector2Int(tileX, tileY);

            GameObject tile = tileManager.ObterTileEm(posTile);
            if (tile == null)
                yield break;

            // Verifica colisão com poço
            Collider[] colisores = Physics.OverlapSphere(transform.position + Vector3.up * 0.25f, 0.5f);
            foreach (var col in colisores)
            {
                if (col.CompareTag("poco"))
                {
                    Debug.Log("O agente caiu em um poço e morreu.");
                    onMorte?.Invoke();
                    Destroy(gameObject);
                    SistemaDePontuacao.instancia?.AdicionarDerrota();
                    LogManager.instancia.AdicionarLog("<color=red> O Agente 1 morreu no poço!</color>");
                    PontuacaoManager.instancia.AlterarPontuacao(-1000);


                    yield break;
                }
            }

            // Detecta brilho do ouro
            foreach (var col in colisores)
            {
                if (col.CompareTag("brilho"))
                {
                    string[] acoes = { "pegar", "mover", "atirar", "voltar" };
                    string acao = acoes[Random.Range(0, acoes.Length)];

                    if (acao == "pegar")
                    {
                        Debug.Log("O agente pegou o ouro!");
                        Destroy(col.gameObject); // Remove o brilho
                        pegouOuro = true;
                        LogManager.instancia.AdicionarLog("<color=green>Ouro coletado!</color>");
                        PontuacaoManager.instancia.AlterarPontuacao(+1000);

                    }
                    continue;
                }
            }

            // Verifica se Wumpus está nas casas adjacentes
            Vector3[] direcoesParaAtirar = {
                Vector3.forward * 1.7f,
                Vector3.back * 1.7f,
                Vector3.left * 1.7f,
                Vector3.right * 1.7f
            };

            foreach (Vector3 dir in direcoesParaAtirar)
            {
                Vector3 alvo = transform.position + dir;
                Collider[] alvos = Physics.OverlapSphere(alvo, 0.3f);
                foreach (var col in alvos)
                {
                    if (col.CompareTag("wumpus"))
                    {
                        string[] acoes = { "mover", "atirar", "voltar" };
                        string acao = acoes[Random.Range(0, acoes.Length)];

                        if (acao == "atirar")
                        {
                            Debug.Log("O agente matou o Wumpus com uma flecha!");
                            Destroy(col.gameObject);
                            LogManager.instancia.AdicionarLog("Agente 1 matou o Wumpus com uma flecha!");
                            PontuacaoManager.instancia.AlterarPontuacao(+1000);

                        }
                        continue;
                    }
                }
            }

            // Movimento aleatório
            Vector3[] direcoes = {
                Vector3.forward * 1.7f,
                Vector3.back * 1.7f,
                Vector3.left * 1.7f,
                Vector3.right * 1.7f
        }
        ;

            List<Vector3> direcoesValidas = new List<Vector3>();
            foreach (Vector3 dir in direcoes)
            {
                Vector3 novaPos = transform.position + dir;
                int x = Mathf.RoundToInt(novaPos.x / 1.7f);
                int y = Mathf.RoundToInt(novaPos.z / 1.7f);
                Vector2Int novaTile = new Vector2Int(x, y);

                if (tileManager.ObterTileEm(novaTile) != null)
                    direcoesValidas.Add(dir);
            }

            if (direcoesValidas.Count > 0)
            {
                Vector3 direcaoEscolhida = direcoesValidas[Random.Range(0, direcoesValidas.Count)];
    Vector3 destino = transform.position + direcaoEscolhida;

    // Calcula rotação para a direção do destino
    Quaternion rotacaoAlvo = Quaternion.LookRotation(direcaoEscolhida, Vector3.up);

    // Suaviza a rotação (ajuste a velocidade conforme desejado)
    float tempoRotacao = 0.15f;
    float t = 0f;
    Quaternion rotacaoInicial = transform.rotation;

    while (t < 1f)
    {
        t += Time.deltaTime / tempoRotacao;
        transform.rotation = Quaternion.Slerp(rotacaoInicial, rotacaoAlvo, t);
        yield return null;
    }

    // Move o agente após a rotação
    transform.position = destino;
                PontuacaoManager.instancia.AlterarPontuacao(-1);
            }
        }
    }
}
