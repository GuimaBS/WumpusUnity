using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PainelMemoriaAgente2 : MonoBehaviour
{
    public GameObject celulaPrefab; // Um quadrado simples com Image (UI)
    public GridLayoutGroup gridLayout;
    public int colunas;
    public int linhas;

    private GameObject[,] celulas;

    public void InicializarPainel(int xSize, int ySize)
    {
        colunas = xSize;
        linhas = ySize;

        celulas = new GameObject[colunas, linhas];

        foreach (Transform filho in gridLayout.transform)
        {
            Destroy(filho.gameObject);
        }

        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = colunas;

        for (int y = linhas - 1; y >= 0; y--)
        {
            for (int x = 0; x < colunas; x++)
            {
                GameObject novaCelula = Instantiate(celulaPrefab, gridLayout.transform);
                novaCelula.name = $"Celula_{x}_{y}";
                celulas[x, y] = novaCelula;
            }
        }
    }

    public void AtualizarCelula(int x, int y, string percepcao)
    {
        if (x < 0 || x >= colunas || y < 0 || y >= linhas) return;

        Image imagem = celulas[x, y].GetComponent<Image>();
        if (imagem == null) return;

        switch (percepcao)
        {
            case "B": // Brisa
                imagem.color = Color.cyan;
                break;
            case "F": // Fedor
                imagem.color = Color.green;
                break;
            case "R": // Brilho
                imagem.color = Color.yellow;
                break;
            case "W": // Wumpus
                imagem.color = Color.red;
                break;
            case "P": // Poço
                imagem.color = Color.black;
                break;
            case " ":
                imagem.color = Color.white;
                break;
        }
    }
}
