using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgenteInteligente : MonoBehaviour
{
    private TileManager tileManager;
    private float velocidade = 1f;
    private Slider velocidadeSlider;

    public System.Action onMorte;
    private MemoriaVisual memoriaVisual;

    private void Start()
    {
        tileManager = TileManager.instancia;
        memoriaVisual = MemoriaVisual.instancia;

        GameObject sliderObj = GameObject.FindWithTag("VelocidadeSlider");
        if (sliderObj != null)
            velocidadeSlider = sliderObj.GetComponent<Slider>();

        StartCoroutine(ComportamentoIA());
    }

    private IEnumerator ComportamentoIA()
    {
        while (true)
        {
            if (velocidadeSlider != null)
                velocidade = velocidadeSlider.value;

            yield return new WaitForSeconds(velocidade);

            Vector2Int posTile = new Vector2Int(
                Mathf.FloorToInt(transform.position.x / 1.7f),
                Mathf.FloorToInt(transform.position.z / 1.7f)
            );

            GameObject tile = tileManager.ObterTileEm(posTile);
            if (tile == null) yield break;

            memoriaVisual.AtualizarTile(posTile, "vazio"); // já marca como descoberta

            // Checa morte pelo Wumpus (posição exata)
            if (posTile == GridGenerator.posicaoWumpus)
            {
                LogManager.instancia.AdicionarLog("<color=red>O Agente2 foi morto pelo Wumpus!</color>");
                memoriaVisual.AtualizarTile(posTile, "wumpus");
                onMorte?.Invoke();
                Destroy(gameObject);
                yield break;
            }

            // Verifica propriedades da tile no TileManager
            TileManager.TileInfo info = tileManager.ObterInfoDaTile(posTile);

            if (info.temPoco)
            {
                LogManager.instancia.AdicionarLog("<color=red>O Agente2 caiu em um poço!</color>");
                memoriaVisual.AtualizarTile(posTile, "poco");
                onMorte?.Invoke();
                Destroy(gameObject);
                yield break;
            }

            if (info.temBrisa)
            {
                memoriaVisual.AtualizarTile(posTile, "brisa");
                LogManager.instancia.AdicionarLog("<color=blue>Agente2 sentiu uma brisa.</color>");
            }

            if (info.temFedor)
            {
                memoriaVisual.AtualizarTile(posTile, "fedor");
                LogManager.instancia.AdicionarLog("<color=green>Agente2 sentiu um fedor.</color>");
            }

            if (info.temOuro)
            {
                memoriaVisual.AtualizarTile(posTile, "brilho");
                LogManager.instancia.AdicionarLog("<color=orange>Agente2 percebeu brilho de ouro.</color>");
            }

            // Detectar ouro
            Collider[] colisores = Physics.OverlapSphere(transform.position + Vector3.up * 0.25f, 0.5f);
            foreach (var col in colisores)
            {
                if (col.CompareTag("brilho"))
                {
                    string[] acoes = { "pegar", "mover", "atirar", "voltar" };
                    string acao = acoes[Random.Range(0, acoes.Length)];

                    if (acao == "pegar")
                    {
                        LogManager.instancia.AdicionarLog("<color=yellow>O Agente2 pegou o ouro!</color>");
                        Destroy(col.gameObject);
                    }
                }
            }

            // Detectar Wumpus adjacente
            Vector3[] direcoesTiro = {
                Vector3.forward * 1.7f,
                Vector3.back * 1.7f,
                Vector3.left * 1.7f,
                Vector3.right * 1.7f
            };

            foreach (Vector3 dir in direcoesTiro)
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
                            LogManager.instancia.AdicionarLog("<color=yellow>O Agente2 matou o Wumpus com uma flecha!</color>");
                            memoriaVisual.AtualizarTile(posTile, "wumpus");
                            Destroy(col.gameObject);
                        }
                    }
                }
            }

            // Movimento aleatório para tiles válidas
            Vector3[] direcoes = {
                Vector3.forward * 1.7f,
                Vector3.back * 1.7f,
                Vector3.left * 1.7f,
                Vector3.right * 1.7f
            };

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

                Quaternion rotacaoAlvo = Quaternion.LookRotation(direcaoEscolhida, Vector3.up);
                Quaternion rotacaoInicial = transform.rotation;

                float tempoRotacao = 0.15f;
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime / tempoRotacao;
                    transform.rotation = Quaternion.Slerp(rotacaoInicial, rotacaoAlvo, t);
                    yield return null;
                }

                transform.position = destino;
            }
        }
    }
}
