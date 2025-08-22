using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public void VoltarParaCharSelect()
    {
        // Transições/Animators precisam de timeScale normal
        if (Time.timeScale == 0f) Time.timeScale = 1f;

        // Tower: salva a run (se ainda não salvou) e zera para a próxima
        TowerStatsController.TryCommitToRanking();
        TowerStatsController.ResetarTodos();

        // Clássico (se esses singletons não existirem nessa cena, ignore silenciosamente)
        try { RankingController.SalvarDados(); } catch { }
        try { TimerPontuacaoController.ResetarContadores(); } catch { }

        // Carrega a CharSelect (modo padrão SINGLE)
        SceneManager.LoadScene("CharSelect", LoadSceneMode.Single);
    }
}
