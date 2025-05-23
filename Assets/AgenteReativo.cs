using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgenteReativo : MonoBehaviour
{
    public float velocidade = 0.3f;
    private TileManager tileManager;
    private Slider velocidadeSlider;

    void Start()
    {
        tileManager = TileManager.instancia;

        GameObject sliderObj = GameObject.FindWithTag("VelocidadeSlider");
        if (sliderObj != null)
        {
            velocidadeSlider = sliderObj.GetComponent<Slider>();
            velocidade = velocidadeSlider.value;
        }

        StartCoroutine(ComportamentoReativo());
    }

    IEnumerator ComportamentoReativo()
    {
        while (true)
        {
            velocidade = velocidadeSlider != null ? velocidadeSlider.value : velocidade;

            Vector3 posAtual = transform.position;

            // Verifica se há poço ou wumpus
            Collider[] colisores = Physics.OverlapSphere(posAtual, 0.1f);
            foreach (var col in colisores)
            {
                if (col.CompareTag("poço"))
                {
                    Debug.Log(" Agente morreu ao cair em um poço!");
                    Destroy(gameObject);
                    yield break;
                }
                else if (col.CompareTag("wumpus"))
                {
                    Debug.Log(" Agente foi devorado pelo Wumpus!");
                    Destroy(gameObject);
                    yield break;
                }
                else if (col.CompareTag("brilho"))
                {
                    int acao = Random.Range(0, 4); // 0 = voltar, 1 = mover, 2 = pegar, 3 = atirar
                    if (acao == 2)
                    {
                        Debug.Log(" Agente pegou o ouro!");
                        Destroy(col.gameObject);
                    }
                }
            }

            // Verifica se há Wumpus ao redor para tentar atirar
            Vector3[] direcoes = {
                Vector3.forward * 1.7f,
                Vector3.back * 1.7f,
                Vector3.left * 1.7f,
                Vector3.right * 1.7f
            };

            foreach (var dir in direcoes)
            {
                Vector3 checar = posAtual + dir;
                Collider[] vizinhos = Physics.OverlapSphere(checar, 0.1f);
                foreach (var col in vizinhos)
                {
                    if (col.CompareTag("wumpus"))
                    {
                        int acao = Random.Range(0, 4);
                        if (acao == 3) // atirar
                        {
                            Debug.Log(" Agente matou o Wumpus com uma flecha!");
                            Destroy(col.gameObject);
                        }
                    }
                }
            }

            // Movimento aleatório apenas para tiles válidas
            List<Vector3> vizinhosValidos = new List<Vector3>();
            foreach (var dir in direcoes)
            {
                Vector3 destino = posAtual + dir;
                if (tileManager.tilesValidas.Contains(destino))
                {
                    vizinhosValidos.Add(destino);
                }
            }

            if (vizinhosValidos.Count > 0)
            {
                Vector3 destinoFinal = vizinhosValidos[Random.Range(0, vizinhosValidos.Count)];
                transform.position = destinoFinal;
                Debug.Log(" Agente se moveu para " + destinoFinal);
            }

            yield return new WaitForSeconds(velocidade);
        }
    }
}
