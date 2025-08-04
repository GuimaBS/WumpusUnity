using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryPanel : MonoBehaviour
{
    public TMP_Text textoResumo;

    void OnEnable()
    {
        float total = TimerPontuacaoController.TempoTotal;
        textoResumo.text = $"Tempo total: {total:F2}s\nPontuação: {TimerPontuacaoController.pontuacaoFinal}";
    }

    public void ConfirmarEVirarParaRanking()
    {
        PlayerData dados = new PlayerData
        {
            nick = CharSelectManager.nickJogador,
            tempoTotal = TimerPontuacaoController.TempoTotal,
            pontuacao = TimerPontuacaoController.pontuacaoFinal,
            passos = TimerPontuacaoController.passosDados,
            mortes = TimerPontuacaoController.mortes
        };


        RankingManager.instancia.AdicionarRegistro(dados);
        SceneManager.LoadScene("TabelaDePontos");
    }

}
