using UnityEngine;
using TMPro;

public class PontuacaoManager : MonoBehaviour
{
    public static PontuacaoManager instancia;

    private int pontuacaoAtual = 0;

    public TextMeshProUGUI textoPontuacao;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
    }

    public void AlterarPontuacao(int valor)
    {
        pontuacaoAtual += valor;
        AtualizarUI();
    }

    private void AtualizarUI()
    {
        if (textoPontuacao != null)
        {
            textoPontuacao.text = $"Pontuação: {pontuacaoAtual}";
        }
    }

    public void ResetarPontuacao()
    {
        pontuacaoAtual = 0;
        AtualizarUI();
    }

    public int ObterPontuacao() => pontuacaoAtual;
}
