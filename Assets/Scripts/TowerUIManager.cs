using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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
    public TMP_Text textoAndar;

    [Header("Botões Torre")]
    [SerializeField] private Button botaoAvancar;

    private int pontuacao = 0;

    private void Awake()
{
    if (instancia == null)
    {
        instancia = this;
    }
    else
    {
        Destroy(gameObject);
        return; // <- garante que não segue executando
    }

    // Garante que começa oculto
    if (botaoAvancar != null)
        botaoAvancar.gameObject.SetActive(false);
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

    public void MostrarBotaoAvancar(bool mostrar)
    {
        if (botaoAvancar != null)
            botaoAvancar.gameObject.SetActive(mostrar);
    }

      private Coroutine animAndarCo;

// Exibe sem animação (primeira carga)
public void AtualizarAndar(int andar)
{
    if (textoAndar != null)
        textoAndar.text = $"Andar: {andar}";
}

// Exibe com animação suave (ao subir)
public void AtualizarAndarAnimado(int andar)
{
    if (textoAndar == null)
    {
        AtualizarAndar(andar);
        return;
    }

    if (animAndarCo != null) StopCoroutine(animAndarCo);
    animAndarCo = StartCoroutine(AnimacaoAndarCoroutine(andar));
}

private IEnumerator AnimacaoAndarCoroutine(int andar)
{
    // Atualiza o texto imediatamente
    textoAndar.text = $"Andar: {andar}";

    // Parâmetros de animação
    Transform t = textoAndar.transform;
    Vector3 baseScale = Vector3.one;
    float upDur = 0.35f;
    float downDur = 0.25f;
    float punch = 1.2f;

    Color baseCol = textoAndar.color;
    Color flashCol = new Color(1f, 0.85f, 0f); // douradinho

    // Escala/Cor - up
    float e = 0f;
    while (e < upDur)
    {
        float p = e / upDur;
        t.localScale = Vector3.Lerp(baseScale, baseScale * punch, p);
        textoAndar.color = Color.Lerp(baseCol, flashCol, p);
        e += Time.unscaledDeltaTime; // independente de Time.timeScale
        yield return null;
    }

    // Escala/Cor - down
    e = 0f;
    while (e < downDur)
    {
        float p = e / downDur;
        t.localScale = Vector3.Lerp(baseScale * punch, baseScale, p);
        textoAndar.color = Color.Lerp(flashCol, baseCol, p);
        e += Time.unscaledDeltaTime;
        yield return null;
    }

    // Garantir estado final
    t.localScale = baseScale;
    textoAndar.color = baseCol;

    animAndarCo = null;
}
}
