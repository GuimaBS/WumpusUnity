using UnityEngine;
using TMPro;
using System.Collections;           
using System.Collections.Generic;   

public class UIManager : MonoBehaviour
{
    public static UIManager instancia;

    [Header("Textos UI")]
    public TMP_Text textoFlechas;
    public TMP_Text textoOuro;
    public TMP_Text textoMortes;         
    public TMP_Text textoWumpusMortos;
    public TMP_Text textoPontuacao;
    public TMP_Text textoTamanhoMapa;

    [Header("Painel de Vitória")]
    public GameObject painelVitoria;

    private int pontuacao = 0;

    // --- Animação: 1 coroutine ativa por label + baselines por label ---
    private readonly Dictionary<TMP_Text, Coroutine> _animCoPorLabel = new Dictionary<TMP_Text, Coroutine>();
    private readonly Dictionary<TMP_Text, Vector3> _baseScalePorLabel = new Dictionary<TMP_Text, Vector3>();
    private readonly Dictionary<TMP_Text, Color> _baseColorPorLabel = new Dictionary<TMP_Text, Color>();

    private void Awake()
    {
        if (instancia == null)
            instancia = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Inicializações sem animação
        if (textoPontuacao) textoPontuacao.text = "Pontuação: " + pontuacao;
        if (textoFlechas) textoFlechas.text = "0";
        if (textoOuro) textoOuro.text = "0";
        if (textoMortes) textoMortes.text = "0";
        if (textoWumpusMortos) textoWumpusMortos.text = "0";

        AtualizarTextoTamanhoMapa(); // sem animação
    }

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

    public void AtualizarMortes(int qtd)
    {
        if (textoMortes != null)
            SetTextAnimated(textoMortes, qtd.ToString());
    }

    public void AtualizarDWumpus(int qtd)
    {
        if (textoWumpusMortos != null)
            SetTextAnimated(textoWumpusMortos, qtd.ToString());
    }

    public void AlterarPontuacao(int valor)
    {
        pontuacao += valor;

        // sincroniza com o seu controlador clássico
        TimerPontuacaoController.pontuacaoFinal = pontuacao;

        // anima o texto
        if (textoPontuacao != null)
            SetTextAnimated(textoPontuacao, "Pontuação: " + pontuacao);
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

    private void EnsureBaseline(TMP_Text label)
    {
        if (label == null) return;
        if (!_baseScalePorLabel.ContainsKey(label))
            _baseScalePorLabel[label] = label.transform.localScale;
        if (!_baseColorPorLabel.ContainsKey(label))
            _baseColorPorLabel[label] = label.color;
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

        // Interrompe animação anterior deste label e restaura baseline
        if (_animCoPorLabel.TryGetValue(label, out var co) && co != null)
        {
            StopCoroutine(co);
            label.transform.localScale = _baseScalePorLabel[label];
            label.color = _baseColorPorLabel[label];
            _animCoPorLabel[label] = null;
        }

        var flash = flashColor ?? new Color(1f, 0.85f, 0f); // dourado suave
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

        // Usa as baselines salvas
        EnsureBaseline(label);
        Transform t = label.transform;
        Vector3 baseScale = _baseScalePorLabel[label];
        Color baseCol = _baseColorPorLabel[label];

        // Fase "up"
        float e = 0f;
        while (e < upDur)
        {
            float p = e / upDur;
            t.localScale = Vector3.Lerp(baseScale, baseScale * punch, p);
            label.color = Color.Lerp(baseCol, flashCol, p);
            e += Time.unscaledDeltaTime; // independente de timeScale
            yield return null;
        }

        // Fase "down"
        e = 0f;
        while (e < downDur)
        {
            float p = e / downDur;
            t.localScale = Vector3.Lerp(baseScale * punch, baseScale, p);
            label.color = Color.Lerp(flashCol, baseCol, p);
            e += Time.unscaledDeltaTime;
            yield return null;
        }

        // Estado final: baseline
        t.localScale = baseScale;
        label.color = baseCol;

        _animCoPorLabel[label] = null;
    }
}
