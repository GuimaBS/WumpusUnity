using UnityEngine;
using UnityEngine.UI;

public class Agent3BenchmarkButton : MonoBehaviour
{
    public Agent3PerfectRunner runner;
    public Button uiButton;

    void Awake()
    {
#if UNITY_2023_1_OR_NEWER
        if (runner == null) runner = Object.FindFirstObjectByType<Agent3PerfectRunner>(FindObjectsInactive.Exclude);
#else
        if (runner == null) runner = FindObjectOfType<Agent3PerfectRunner>();
#endif
        if (runner != null) runner.OnFinished.AddListener(ReabilitarBotao);
    }

    public void RodarCoringa()
    {
#if UNITY_2023_1_OR_NEWER
        if (runner == null) runner = Object.FindFirstObjectByType<Agent3PerfectRunner>(FindObjectsInactive.Exclude);
#else
        if (runner == null) runner = FindObjectOfType<Agent3PerfectRunner>();
#endif
        if (runner == null) { Debug.LogError("[Coringa] Runner não encontrado."); return; }

        if (uiButton != null) uiButton.interactable = false;
        runner.RunBenchmark();
    }

    private void ReabilitarBotao()
    {
        if (uiButton != null) uiButton.interactable = true;
    }
}
