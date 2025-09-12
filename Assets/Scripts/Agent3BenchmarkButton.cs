using UnityEngine;

public class Agent3BenchmarkButton : MonoBehaviour
{
    public Agent3PerfectRunner runner; // arraste o runner da cena

    // Ligue este método no OnClick() do botão "Coringa"
    public void RodarCoringa()
    {
        if (runner == null)
        {
            runner = FindObjectOfType<Agent3PerfectRunner>();
        }
        if (runner == null)
        {
            Debug.LogError("[Coringa] Runner não encontrado na cena.");
            return;
        }
        runner.RunBenchmark();
    }
}
