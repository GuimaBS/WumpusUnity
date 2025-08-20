using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapaVisualTower : MonoBehaviour
{
    public static MapaVisualTower instancia;

    [Header("UI")]
    public GameObject celulaPrefab;
    public Transform gridContainer;

    private Dictionary<Vector2Int, Image> celulasVisual = new Dictionary<Vector2Int, Image>();

    private void Awake()
    {
        if (instancia == null) instancia = this;
        else Destroy(gameObject);
    }

    public void InicializarMapa(int largura, int altura)
    {
        // limpa grid antigo
        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);

        celulasVisual.Clear();

        // aplica largura no GridLayout
        var layout = gridContainer.GetComponent<GridLayoutGroup>();
        if (layout != null)
        {
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = largura;
        }

        // cria células espelhando o eixo Y (como no Player)
        for (int y = altura - 1; y >= 0; y--)
        {
            for (int x = 0; x < largura; x++)
            {
                var go = Instantiate(celulaPrefab, gridContainer);
                go.name = $"Tile ({x},{y})";

                var img = go.GetComponent<Image>();
                img.color = Color.gray; // desconhecido inicialmente

                celulasVisual[new Vector2Int(x, y)] = img;
            }
        }
    }

   public void AtualizarTile(Vector2Int posicao, List<string> sensacoes)
{
    if (!celulasVisual.ContainsKey(posicao)) return;

    Image imagem = celulasVisual[posicao];
    Color corFinal = Color.gray;

    bool temBrisa  = sensacoes.Contains("brisa");
    bool temFedor  = sensacoes.Contains("fedor");
    bool temBrilho = sensacoes.Contains("brilho");
    bool temWumpus = sensacoes.Contains("wumpus");
    bool temPoco   = sensacoes.Contains("poco");
    bool temEscada = sensacoes.Contains("escada"); // << NOVO

    // --- ESCADA (tem prioridade visual) ---
    if (temEscada)
    {
        corFinal = Color.magenta; // escada = magenta
    }
    // Combinação tripla: brisa + fedor + brilho
    else if (temBrisa && temFedor && temBrilho)
    {
        corFinal = new Color(0.6f, 0f, 0.8f); // roxo
    }
    // Combinações duplas específicas
    else if (temBrisa && temFedor)
    {
        corFinal = new Color(0.6f, 1f, 0.6f); // verde claro
    }
    else if (temBrilho && temFedor)
    {
        corFinal = new Color(0.5f, 0.25f, 0f); // marrom
    }
    else if (temBrilho && temBrisa)
    {
        corFinal = new Color(0.6f, 0.9f, 1f); // azul claro
    }
    // Sensações únicas
    else if (temWumpus)
    {
        corFinal = Color.red;
    }
    else if (temPoco)
    {
        corFinal = Color.black;
    }
    else if (temBrilho)
    {
        corFinal = new Color(1f, 0.65f, 0f); // laranja
    }
    else if (temFedor)
    {
        corFinal = Color.green;
    }
    else if (temBrisa)
    {
        corFinal = Color.blue;
    }
    else if (sensacoes.Contains("vazio"))
    {
        corFinal = Color.white;
    }

    imagem.color = corFinal;
}
}