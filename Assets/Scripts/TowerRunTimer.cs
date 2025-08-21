using UnityEngine;

public class TowerRunTimer : MonoBehaviour
{
    private void Start()
    {
      
        TowerStatsController.ResetarTodos();
    }

    private void Update()
    {
        TowerStatsController.TickTempo(Time.deltaTime);
    }
}
