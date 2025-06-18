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
    public gerarCSV loggerCSV;

    private void Start()
    {
        tileManager = TileManager.instancia;
        PontuacaoManager.instancia.AlterarPontuacao(0); // Inicia com 0 visível

        GameObject sliderObj = GameObject.FindWithTag("VelocidadeSlider");
        if (sliderObj != null)
        {
            velocidadeSlider = sliderObj.GetComponent<Slider>();
        }

        if (!loggerCSV)
        {
            loggerCSV = FindFirstObjectByType<gerarCSV>();
            if (!loggerCSV) Debug.LogError("Não encontrou instância de gerarCSV!");
        }

        StartCoroutine(ComportamentoReativo());
    }

    private IEnumerator ComportamentoReativo()
    {
        while (true)
        {
            if (velocidadeSlider != null)
                velocidade = Mathf.Max(0.1f, velocidadeSlider.value); // Garante que nunca será zero

            yield return new WaitForSeconds(velocidade);

            Vector3 pos = transform.position;
            Vector2Int posicaoAtual = new Vector2Int(
                Mathf.RoundToInt(pos.x / 1.7f),
                Mathf.RoundToInt(pos.z / 1.7f)
            );

            GameObject tile = tileManager.ObterTileEm(posicaoAtual);
            if (tile == null)
                yield break;

            // Verificar morte por poço
            Collider[] colisores = Physics.OverlapSphere(transform.position + Vector3.up * 0.25f, 0.5f);
            foreach (var col in colisores)
            {
                if (col.CompareTag("poco"))
                {
                    if (loggerCSV) loggerCSV.RegistrarEvento("MortePoco", transform.position, "Agente1");
                    LogManager.instancia.AdicionarLog("<color=red> O Agente caiu em um poço e morreu.</color>");
                    onMorte?.Invoke();
                    Destroy(gameObject);
                    SistemaDePontuacao.instancia?.AdicionarDerrota();
                    PontuacaoManager.instancia.AlterarPontuacao(-1000);
                    yield break;
                }
            }

            // Verificar morte por Wumpus
            if (GridGenerator.posicoesWumpus.Contains(posicaoAtual))
            {
                if (loggerCSV) loggerCSV.RegistrarEvento("MorteWumpus", transform.position, "Agente1");
                LogManager.instancia.AdicionarLog("<color=red> O Agente foi morto pelo Wumpus.</color>");
                onMorte?.Invoke();
                Destroy(gameObject);
                SistemaDePontuacao.instancia?.AdicionarDerrota();
                PontuacaoManager.instancia.AlterarPontuacao(-1000);
                yield break;
            }

            // Detecta ouro
            foreach (var col in colisores)
            {
                if (col.CompareTag("brilho"))
                {
                    string[] acoes = { "pegar", "mover", "atirar", "voltar" };
                    string acao = acoes[Random.Range(0, acoes.Length)];

                    if (acao == "pegar")
                    {
                        Destroy(col.gameObject);
                        pegouOuro = true;
                        if (loggerCSV) loggerCSV.RegistrarEvento("PegarOuro", transform.position, "Agente1");
                        LogManager.instancia.AdicionarLog("<color=yellow>O Agente pegou o ouro!</color>");
                        PontuacaoManager.instancia.AlterarPontuacao(+1000);
                    }
                }
            }

            // Detectar percepções (logs)
            var info = tileManager.ObterInfoDaTile(posicaoAtual);
            if (info != null)
            {
                if (info.temBrisa)
                    LogManager.instancia.AdicionarLog("<color=lightblue>O Agente sentiu brisa...</color>");
                if (info.temFedor)
                    LogManager.instancia.AdicionarLog("<color=green>O Agente sentiu um fedor...</color>");
                if (info.temOuro)
                    LogManager.instancia.AdicionarLog("<color=yellow>O Agente percebeu brilho...</color>");
            }

            // Verifica Wumpus nas casas adjacentes e tenta atirar (de forma aleatória, sem inferência)
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
                            Destroy(col.gameObject);

                            Vector2Int posWumpus = posicaoAtual + new Vector2Int(
                                Mathf.RoundToInt(dir.x / 1.7f),
                                Mathf.RoundToInt(dir.z / 1.7f)
                            );

                            GridGenerator.EliminarWumpusNaPosicao(posWumpus);

                            if (loggerCSV) loggerCSV.RegistrarEvento("AtirarAcerto", transform.position, "Agente1");
                            LogManager.instancia.AdicionarLog("<color=orange>O Agente matou o Wumpus com uma flecha!</color>");
                            PontuacaoManager.instancia.AlterarPontuacao(+1000);
                        }
                    }
                }
            }

            // Movimento aleatório
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
                Vector2Int novaTile = new Vector2Int(
                    Mathf.RoundToInt(novaPos.x / 1.7f),
                    Mathf.RoundToInt(novaPos.z / 1.7f)
                );

                if (tileManager.ObterTileEm(novaTile) != null)
                    direcoesValidas.Add(dir);
            }

            if (direcoesValidas.Count > 0)
            {
                Vector3 direcaoEscolhida = direcoesValidas[Random.Range(0, direcoesValidas.Count)];
                Vector3 destino = transform.position + direcaoEscolhida;

                // Rotaciona antes de se mover
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
                PontuacaoManager.instancia.AlterarPontuacao(-1);
            }
        }
    }
}
