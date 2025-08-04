using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RankingController
{
    public class DadosDoJogador
    {
        public string nick;
        public int passos;
        public int mortes;
        public float tempoTotal;
        public int pontuacao;
    }

    public static List<DadosDoJogador> ranking = new List<DadosDoJogador>();

    public static void SalvarDados()
    {
        // Cria nova entrada com os dados atuais
        DadosDoJogador novo = new DadosDoJogador
        {
            nick = CharSelectManager.nickJogador,
            passos = TimerPontuacaoController.passosDados,
            mortes = TimerPontuacaoController.mortes,
            tempoTotal = TimerPontuacaoController.TempoTotal,
            pontuacao = TimerPontuacaoController.pontuacaoFinal,
        };

        ranking.Add(novo);
    }

    public static List<DadosDoJogador> ObterRankingOrdenado()
    {
        return new List<DadosDoJogador>(ranking).OrderBy(j => j.tempoTotal).ThenByDescending(j => j.pontuacao).ToList();
    }
}
