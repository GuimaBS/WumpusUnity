using UnityEngine;
using TMPro;

public class TowerUIManager : MonoBehaviour
{
    public static TowerUIManager instancia;

    [Header("Textos UI")]
    public TMP_Text textoFlechas;
    public TMP_Text textoOuro;
    public TMP_Text textoWumpusMortos;
    public TMP_Text textoVidas;
    public TMP_Text textoPontuacao;

    private int pontuacao = 0;

    private void Awake()
    {
        if (instancia == null)
            instancia = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        AtualizarTextoPontuacao();
    }

    public void AtualizarFlechas(int qtd)
    {
        if (textoFlechas != null)
            textoFlechas.text = qtd.ToString();
    }

    public void AtualizarOuro(int qtd)
    {
        if (textoOuro != null)
            textoOuro.text = qtd.ToString();
    }

    public void AtualizarDWumpus(int qtd)
    {
        if (textoWumpusMortos != null)
            textoWumpusMortos.text = qtd.ToString();
    }

    public void AtualizarVidas(int qtd)
    {
        if (textoVidas != null)
            textoVidas.text = qtd.ToString();
    }

    public void AlterarPontuacao(int valor)
    {
        pontuacao += valor;
        AtualizarTextoPontuacao();
    }

    private void AtualizarTextoPontuacao()
    {
        if (textoPontuacao != null)
            textoPontuacao.text = "Pontuação: " + pontuacao;
    }

    public int ObterPontuacao()
    {
        return pontuacao;
    }

}
