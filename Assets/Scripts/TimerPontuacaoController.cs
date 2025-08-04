using UnityEngine;
using UnityEngine.SceneManagement;


public static class TimerPontuacaoController
{
    public static float TempoTotal = 0f;
    public static int passosDados = 0;
    public static int mortes = 0;
    public static int pontuacaoFinal = 0;
    public static float tempoInicio = 0f;

    public static void ResetarContadores()
    {
        TempoTotal = 0f;
        passosDados = 0;
        tempoInicio = Time.time;
        mortes = 0;
        pontuacaoFinal = 0;
    }

}
