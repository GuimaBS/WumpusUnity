using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MapaVisualPlayer : MonoBehaviour
{
    public static MapaVisualPlayer instancia;

    public GameObject celulaPrefab;
    public Transform gridContainer;

    private Dictionary<Vector2Int, Image> celulasVisual = new Dictionary<Vector2Int, Image>();

    private void Awake()
    {
        if (instancia == null)
            instancia = this;
        else
            Destroy(gameObject); // opcional para evitar múltiplas instâncias
    }


    public void InicializarMapa(int largura, int altura)
    {
        // Limpa o conteúdo anterior do grid
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }
        celulasVisual.Clear();

        // Define dinamicamente o tamanho correto no GridLayoutGroup
        GridLayoutGroup layout = gridContainer.GetComponent<GridLayoutGroup>();
        if (layout != null)
        {
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = largura;
        }

        for (int y = altura - 1; y >= 0; y--) // Inverter Y para espelhar visualmente
        {
            for (int x = 0; x < largura; x++)
            {
                GameObject celula = Instantiate(celulaPrefab, gridContainer);
                celula.name = $"Tile ({x},{y})";

                Image imagem = celula.GetComponent<Image>();
                imagem.color = Color.gray; // Desconhecido

                celulasVisual[new Vector2Int(x, y)] = imagem;
            }
        }
    }

    public void AtualizarTile(Vector2Int posicao, List<string> sensacoes)
    {
        if (!celulasVisual.ContainsKey(posicao)) return;

        Image imagem = celulasVisual[posicao];
        Color corFinal = Color.gray;

        // Identifica combinações
        bool temBrisa = sensacoes.Contains("brisa");
        bool temFedor = sensacoes.Contains("fedor");
        bool temBrilho = sensacoes.Contains("brilho");
        bool temWumpus = sensacoes.Contains("wumpus");
        bool temPoco = sensacoes.Contains("poco");

        // Combinação tripla: brisa + fedor + brilho
        if (temBrisa && temFedor && temBrilho)
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
