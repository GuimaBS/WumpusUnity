using UnityEngine;
using TMPro;

public class SistemaDePontuacao : MonoBehaviour
{
    public static SistemaDePontuacao instancia;

    public TextMeshProUGUI textoVitorias;
    public TextMeshProUGUI textoDerrotas;

    private int vitorias = 0;
    private int derrotas = 0;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
    }

    public void AdicionarVitoria()
    {
        vitorias++;
        AtualizarPainel();
    }

    public void AdicionarDerrota()
    {
        derrotas++;
        AtualizarPainel();
    }

    public void ResetarPontuacao()
    {
        vitorias = 0;
        derrotas = 0;
        AtualizarPainel();
    }

    private void AtualizarPainel()
    {
        if (textoVitorias != null) textoVitorias.text = vitorias.ToString();
        if (textoDerrotas != null) textoDerrotas.text = derrotas.ToString();
    }
}
