using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class AgenteReativo : MonoBehaviour
{
    public Slider velocidadeSlider;

    private string[] acoes = { "voltar", "mover", "pegar", "atirar" };
    private float tempoProximaAcao = 0f;

    void Start()
    {
        if (velocidadeSlider == null)
        {
            GameObject sliderObj = GameObject.Find("VelocidadeSlider");
            if (sliderObj != null)
            {
                velocidadeSlider = sliderObj.GetComponent<Slider>();
            }
        }
    }

    void Update()
    {
        if (Time.time < tempoProximaAcao) return;

        tempoProximaAcao = Time.time + velocidadeSlider.value;

        if (Detectou("brisa") || Detectou("fedor") || Detectou("brilho"))
        {
            string acao = EscolherAcaoAleatoria();
            ExecutarAcao(acao);
        }
        else
        {
            Explorar();
        }
    }

    bool Detectou(string tipo)
    {
        Collider[] colisores = Physics.OverlapSphere(transform.position, 0.1f);
        foreach (var col in colisores)
        {
            if (col.CompareTag(tipo))
                return true;
        }
        return false;
    }

    string EscolherAcaoAleatoria()
    {
        int index = Random.Range(0, acoes.Length);
        return acoes[index];
    }

    void ExecutarAcao(string acao)
    {
        switch (acao)
        {
            case "voltar":
                transform.position += Vector3.back;
                break;
            case "mover":
                transform.position += DirecaoAleatoria();
                VerificarMortePorPoco();
                VerificarMortePorWumpus();
                break;
            case "pegar":
                // Verifica se há brilhoouro na posição atual
                Collider[] itens = Physics.OverlapSphere(transform.position, 0.1f);
                foreach (var col in itens)
                {
                    if (col.CompareTag("brilhoouro") || col.CompareTag("ouro"))
                    {
                        Debug.Log("Agente pegou o ouro!");
                        Destroy(col.gameObject);
                    }
                }
                break;

                break;
            case "atirar":
                TentarAtirarNoWumpus();
                break;
        }
    }

    void Explorar()
    {
        transform.position += DirecaoAleatoria();
        VerificarMortePorPoco();
        VerificarMortePorWumpus();
    }

    void VerificarMortePorPoco()
    {
        Collider[] colisores = Physics.OverlapSphere(transform.position, 0.1f);
        foreach (var col in colisores)
        {
            if (col.CompareTag("poco"))
            {
                Debug.Log("Agente caiu no poço e morreu!");
                Destroy(gameObject);
                return;
            }
        }
    }

    void VerificarMortePorWumpus()
    {
        Collider[] colisores = Physics.OverlapSphere(transform.position, 0.1f);
        foreach (var col in colisores)
        {
            if (col.CompareTag("wumpus"))
            {
                Debug.Log("Agente foi devorado pelo Wumpus!");
                Destroy(gameObject);
                return;
            }
        }
    }

    void TentarAtirarNoWumpus()
    {
        Vector3[] direcoes = {
            Vector3.forward,
            Vector3.back,
            Vector3.left,
            Vector3.right
        };

        foreach (var dir in direcoes)
        {
            Vector3 alvo = transform.position + dir;
            Collider[] colisores = Physics.OverlapSphere(alvo, 0.1f);
            foreach (var col in colisores)
            {
                if (col.CompareTag("wumpus"))
                {
                    Debug.Log("Wumpus foi morto por uma flecha!");
                    Destroy(col.gameObject);
                    return;
                }
            }
        }
    }

    Vector3 DirecaoAleatoria()
    {
        Vector2Int posAtual = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));
        List<Vector2Int> vizinhos = new List<Vector2Int>()
    {
        posAtual + Vector2Int.up,
        posAtual + Vector2Int.down,
        posAtual + Vector2Int.left,
        posAtual + Vector2Int.right
    };

        List<Vector2Int> validos = new List<Vector2Int>();

        foreach (var pos in vizinhos)
        {
            if (TileManager.instancia.TileExiste(pos))
                validos.Add(pos);
        }

        if (validos.Count > 0)
        {
            Vector2Int escolhido = validos[Random.Range(0, validos.Count)];
            return new Vector3(escolhido.x, transform.position.y, escolhido.y) - transform.position;
        }

        return Vector3.zero; // sem vizinhos válidos
    }

}
