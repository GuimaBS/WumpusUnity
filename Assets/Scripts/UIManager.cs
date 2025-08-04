using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instancia;

    [Header("Textos UI")]
    public TMP_Text textoFlechas;
    public TMP_Text textoOuro;
    public TMP_Text textoMortes;
    public TMP_Text textoWumpusMortos;
    public TMP_Text textoPontuacao;
    public TMP_Text textoTamanhoMapa; // <-- Novo campo para exibir o tamanho do mapa

    [Header("Painel de Vitória")]
    public GameObject painelVitoria;

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
        AtualizarTextoTamanhoMapa(); // <-- Atualiza ao iniciar a cena
    }

    public void AtualizarFlechas(int qtd)
    {
        textoFlechas.text = qtd.ToString();
    }

    public void AtualizarOuro(int qtd)
    {
        textoOuro.text = qtd.ToString();
    }

    public void AtualizarMortes(int qtd)
    {
        textoMortes.text = qtd.ToString();
    }

    public void AtualizarDWumpus(int qtd)
    {
        textoWumpusMortos.text = qtd.ToString();
    }

    public void AlterarPontuacao(int valor)
    {
        pontuacao += valor;
        TimerPontuacaoController.pontuacaoFinal = pontuacao; // ok, sincroniza
        AtualizarTextoPontuacao(); // mostra na UI
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

    public void AtualizarTextoTamanhoMapa()
    {
        int x = PlayerPrefs.GetInt("mapX");
        int y = PlayerPrefs.GetInt("mapY");

        if (textoTamanhoMapa != null)
            textoTamanhoMapa.text = $"{x}x{y}";
    }

    public void MostrarPainelVitoria()
    {
        if (painelVitoria == null)
        {
            Debug.LogError("[UIManager] painelVitoria está NULL!");
            return;
        }

        Debug.Log("[UIManager] Ativando painel de vitória.");
        painelVitoria.SetActive(true);
    }


}
