using UnityEngine;
using UnityEngine.UI;
using System.Collections;           // IEnumerator / Coroutines
using System.Collections.Generic;   // Dictionary<T, T>
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
    private readonly Dictionary<TMP_Text, Vector3> _baseScalePorLabel = new Dictionary<TMP_Text, Vector3>();
    private readonly Dictionary<TMP_Text, Color> _baseColorPorLabel = new Dictionary<TMP_Text, Color>();

    // Mapa: 1 coroutine ativa por label (evita sobrepor animações)
    private readonly Dictionary<TMP_Text, Coroutine> _animCoPorLabel = new Dictionary<TMP_Text, Coroutine>();

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // começa oculto
        if (botaoAvancar != null)
            botaoAvancar.gameObject.SetActive(false);
    }

    private void Start()
    {
        // inicializa texto de pontuação sem animação
        AtualizarTextoPontuacao();

        // garante que o primeiro andar apareça (sem animação)
        if (TowerGridGenerator.instancia != null)
            AtualizarAndar(TowerGridGenerator.instancia.andarAtual);
    }

    // ----------- Atualizações com animação -----------

    public void AtualizarFlechas(int qtd)
    {
        if (textoFlechas != null)
            SetTextAnimated(textoFlechas, qtd.ToString());
    }

    public void AtualizarOuro(int qtd)
    {
        if (textoOuro != null)
            SetTextAnimated(textoOuro, qtd.ToString());
    }

    public void AtualizarDWumpus(int qtd)
    {
        if (textoWumpusMortos != null)
            SetTextAnimated(textoWumpusMortos, qtd.ToString());
    }

    public void AtualizarVidas(int qtd)
    {
        if (textoVidas != null)
            SetTextAnimated(textoVidas, qtd.ToString());
    }

    public void AlterarPontuacao(int valor)
    {
        pontuacao += valor;
        if (textoPontuacao != null)
            SetTextAnimated(textoPontuacao, "Pontuação: " + pontuacao);
    }

    // usada para primeira carga / reset (sem animação)
    private void AtualizarTextoPontuacao()
    {
        if (textoPontuacao != null)
            textoPontuacao.text = "Pontuação: " + pontuacao;
    }

    public int ObterPontuacao() => pontuacao;

    public void MostrarBotaoAvancar(bool mostrar)
    {
        if (botaoAvancar != null)
            botaoAvancar.gameObject.SetActive(mostrar);
    }

    // ----------- Andar: sem animação vs com animação -----------

    // Sem animação (primeira exibição)
    public void AtualizarAndar(int andar)
    {
        if (textoAndar != null)
            textoAndar.text = $"Andar: {andar}";
    }

    // Com animação (ao subir de andar)
    public void AtualizarAndarAnimado(int andar)
    {
        if (textoAndar == null)
        {
            AtualizarAndar(andar);
            return;
        }

        SetTextAnimated(textoAndar, $"Andar: {andar}");
    }

    private void SetTextAnimated(
    TMP_Text label,
    string newText,
    Color? flashColor = null,
    float upDur = 0.35f,
    float downDur = 0.25f,
    float punch = 1.2f)
    {
        if (label == null) return;

        EnsureBaseline(label);

        if (_animCoPorLabel.TryGetValue(label, out var co) && co != null)
        {
            StopCoroutine(co);
            label.transform.localScale = _baseScalePorLabel[label];
            label.color = _baseColorPorLabel[label];
            _animCoPorLabel[label] = null;
        }

        var flash = flashColor ?? new Color(1f, 0.85f, 0f); // douradinho padrão
        _animCoPorLabel[label] = StartCoroutine(
            AnimateTMPCoroutine(label, newText, flash, upDur, downDur, punch)
        );
    }


    private IEnumerator AnimateTMPCoroutine(
     TMP_Text label,
     string newText,
     Color flashCol,
     float upDur,
     float downDur,
     float punch)
    {
        // Atualiza o texto imediatamente
        label.text = newText;

        // Usa as baselines salvas (garante retorno ao estado original)
        EnsureBaseline(label);
        Transform t = label.transform;
        Vector3 baseScale = _baseScalePorLabel[label];
        Color baseCol = _baseColorPorLabel[label];

        // Fase "up": cresce e troca a cor
        float e = 0f;
        while (e < upDur)
        {
            float p = e / upDur;
            t.localScale = Vector3.Lerp(baseScale, baseScale * punch, p);
            label.color = Color.Lerp(baseCol, flashCol, p);
            e += Time.unscaledDeltaTime; // independente de timeScale
            yield return null;
        }

        // Fase "down": volta ao normal
        e = 0f;
        while (e < downDur)
        {
            float p = e / downDur;
            t.localScale = Vector3.Lerp(baseScale * punch, baseScale, p);
            label.color = Color.Lerp(flashCol, baseCol, p);
            e += Time.unscaledDeltaTime;
            yield return null;
        }

        // Garante estado final
        t.localScale = baseScale;
        label.color = baseCol;

        // Libera a referência desta label
        _animCoPorLabel[label] = null;
    }


    private void EnsureBaseline(TMP_Text label)
    {
        if (label == null) return;
        if (!_baseScalePorLabel.ContainsKey(label))
            _baseScalePorLabel[label] = label.transform.localScale;
        if (!_baseColorPorLabel.ContainsKey(label))
            _baseColorPorLabel[label] = label.color;
    }


}
