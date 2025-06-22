using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MemoriaVisual : MonoBehaviour
{
    public static MemoriaVisual instancia;

    public GameObject celulaPrefab;
    public Transform gridContainer;

    private Dictionary<Vector2Int, string> memoria = new Dictionary<Vector2Int, string>();
    private Dictionary<Vector2Int, Image> celulasVisual = new Dictionary<Vector2Int, Image>();

    private void Awake()
    {
        if (instancia == null)
            instancia = this;
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

        for (int y = altura - 1; y >= 0; y--) // Inverte o Y para parecer visualmente com o mapa real
        {
            for (int x = 0; x < largura; x++)
            {
                GameObject celula = Instantiate(celulaPrefab, gridContainer);
                celula.name = $"Memoria ({x},{y})";

                Image imagem = celula.GetComponent<Image>();
                imagem.color = Color.gray; // cor padrão para desconhecido

                celulasVisual[new Vector2Int(x, y)] = imagem;
            }
        }
    }

    public void AtualizarTile(Vector2Int posicao, string tipo)
    {
        if (celulasVisual.ContainsKey(posicao))
        {
            Image imagem = celulasVisual[posicao];

            switch (tipo)
            {
                case "brisa": imagem.color = Color.blue; break;
                case "fedor": imagem.color = Color.green; break;
                case "brilho": imagem.color = new Color(1f, 0.65f, 0f); break; // laranja
                case "poco": imagem.color = Color.black; break;
                case "wumpus": imagem.color = Color.red; break;
                case "vazio": imagem.color = Color.white; break;
            }
        }
    }

    public bool TilePareceSegura(Vector2Int posicao)
    {
        if (!memoria.ContainsKey(posicao))
            return true; // ainda não sabe nada sobre a tile

        string tipo = memoria[posicao];

        // Tile conhecida como perigosa
        if (tipo == "poco" || tipo == "wumpus")
            return false;

        // Tile com brisa ou fedor é potencialmente perigosa
        if (tipo == "brisa" || tipo == "fedor")
            return false;

        return true; // Caso contrário, é segura ou vazia
    }
    public bool TemRegistro(Vector2Int posicao)
    {
        return memoria.ContainsKey(posicao);
    }

}
