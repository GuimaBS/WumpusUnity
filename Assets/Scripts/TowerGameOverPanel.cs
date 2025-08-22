using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TowerGameOverPanel : MonoBehaviour
{
    public static TowerGameOverPanel instancia;

    [Header("Referências")]
    public GameObject painelDerrota;
    public Button botaoCharSelect;
    public Button botaoLeaderboard;   // ainda não usado

    [Header("Config")]
    public string cenaCharSelect = "CharSelect";
    public string cenaTabeladePontosTower = "TabeladePontosTower";
    public bool pausarJogoAoMostrar = true;
    public bool desativarLeaderboard = true;

    [Header("Delay")]
    [Tooltip("Tempo (em segundos, tempo real) para esperar antes de exibir o painel.")]
    public float defaultDelay = 0.6f;

    void Awake()
    {
        instancia = this;
        if (painelDerrota != null) painelDerrota.SetActive(false);

        if (botaoLeaderboard != null)
        {
            botaoLeaderboard.onClick.AddListener(OnCliqueLeaderboard);
            if (desativarLeaderboard) botaoLeaderboard.interactable = false;
        }
    }

    // Mantém a assinatura antiga
    public void Show()
    {
        StartCoroutine(ShowRoutine(defaultDelay));
    }

    // Nova sobrecarga com delay customizável
    public void Show(float delaySeconds)
    {
        StartCoroutine(ShowRoutine(delaySeconds));
    }

    private IEnumerator ShowRoutine(float delay)
    {
        // Usa tempo real para não ser afetado por Time.timeScale
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        if (painelDerrota != null) painelDerrota.SetActive(true);
        if (pausarJogoAoMostrar) Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (pausarJogoAoMostrar) Time.timeScale = 1f;
        if (painelDerrota != null) painelDerrota.SetActive(false);
    }

    public void OnCliqueLeaderboard()
    {
        SceneManager.LoadScene(cenaTabeladePontosTower);
    }
}
