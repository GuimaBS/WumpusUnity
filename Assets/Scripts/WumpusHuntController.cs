using UnityEngine;
using System.Collections;

public class WumpusHuntController : MonoBehaviour
{
    [Header("Refs (opcionais se usar auto-bind)")]
    public WumpusAI wumpus;                 // pode deixar vazio; achamos sozinho
    public GameObject painelDespertar;      // painel UI (off por padrão)
    public AudioSource audioSource;         // se vazio, criaremos um no Start
    public AudioClip sfxDespertar;          // som do despertar (opcional)

    [Header("Config")]
    public int limiarPontuacao = -5000;
    public bool repetirAlertaSeDesligar = false;

    private bool ativado = false;

    private void Start()
    {
        if (painelDespertar != null) painelDespertar.SetActive(false);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D para UI
        }

        TryBindWumpusImmediate();
        if (wumpus == null) StartCoroutine(TryBindWumpusRoutine());
    }

    private void Update()
    {
        if (ativado) return;
        if (PlayerGridGenerator.instancia == null) return;
        if (PlayerGridGenerator.instancia.wumpusMorto) return;

        int score = TimerPontuacaoController.pontuacaoFinal;
        if (score <= limiarPontuacao)
        {
            AtivarCacada();
        }
    }

    private void AtivarCacada()
    {
        ativado = true;

        if (painelDespertar != null)
            painelDespertar.SetActive(true);

        if (audioSource != null && sfxDespertar != null)
            audioSource.PlayOneShot(sfxDespertar);

        if (wumpus != null)
            wumpus.SetCaçada(true);

        LogManager.instancia?.AdicionarLog("O Wumpus despertou! A caçada começou.");
    }

    // Fecha apenas o painel, mantendo a caçada do Wumpus ativa
    public void FecharPainelCacada()
    {
        if (painelDespertar != null)
            painelDespertar.SetActive(false);
        // não desativa 'ativado' e não chama SetCaçada(false)
    }

    // Encerra a caçada (mantido para quando você realmente quiser parar tudo)
    public void EncerrarCacada()
    {
        if (wumpus != null) wumpus.SetCaçada(false);
        if (painelDespertar != null) painelDespertar.SetActive(false);
        if (!repetirAlertaSeDesligar) ativado = true;
    }

    // ===== Auto-bind helpers =====
    private void TryBindWumpusImmediate()
    {
        if (wumpus != null) return;

        wumpus = FindFirstObjectByType<WumpusAI>();
        if (wumpus != null) return;

        var go = GameObject.FindWithTag("wumpus");
        if (go != null) wumpus = go.GetComponentInParent<WumpusAI>() ?? go.GetComponent<WumpusAI>();
    }

    private IEnumerator TryBindWumpusRoutine()
    {
        float timeout = 5f;
        float t = 0f;
        while (wumpus == null && t < timeout)
        {
            TryBindWumpusImmediate();
            if (wumpus != null) yield break;
            t += Time.deltaTime;
            yield return null;
        }
        if (wumpus == null)
        {
            Debug.LogWarning("[WumpusHuntController] Não foi possível localizar um WumpusAI na cena.");
        }
    }
}
