using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public void VoltarParaCharSelect()
    {
        RankingController.SalvarDados();
        TimerPontuacaoController.ResetarContadores();
        SceneManager.LoadScene("CharSelect");
    }
}
