using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject painelPause;
    public GameObject painelGuia;

    public InputAction pauseAction;

    private float tempoPausado = 0f;
    public static bool jogoPausado = false;

    void OnEnable()
    {
        pauseAction.Enable();
        pauseAction.performed += OnPausePressed;
    }

    void OnDisable()
    {
        pauseAction.Disable();
        pauseAction.performed -= OnPausePressed;
    }

    void OnPausePressed(InputAction.CallbackContext context)
    {
        if (!jogoPausado)
            PausarJogo();
        else
            ContinuarJogo();
    }

    public void PausarJogo()
    {
        painelPause.SetActive(true);
        Time.timeScale = 0f;
        jogoPausado = true;

        // Pausa o tempo lógico
        tempoPausado = Time.time;
    }

    public void ContinuarJogo()
    {
        painelPause.SetActive(false);
        Time.timeScale = 1f;
        jogoPausado = false;

        // Corrige o tempo total ignorando o tempo pausado
        float tempoDurantePausa = Time.time - tempoPausado;
        TimerPontuacaoController.tempoInicio += tempoDurantePausa;
    }

    public void AbrirGuia()
    {
        painelGuia.SetActive(true);
        painelPause.SetActive(false);
    }

    public void FecharGuia()
    {
        painelGuia.SetActive(false);
        painelPause.SetActive(true);
    }

    public void ResetarPartida()
    {
        ContinuarJogo(); // Garante o Time.timeScale = 1f
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VoltarParaCharSelect()
    {
        Time.timeScale = 1f;
        jogoPausado = false;
        RankingController.SalvarDados();
        TimerPontuacaoController.ResetarContadores();
        SceneManager.LoadScene("CharSelect");
    }
}
