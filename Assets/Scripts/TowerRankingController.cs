using UnityEngine;

public static class TowerRankingController
{
    public static void SalvarDadosDaRunAtual()
    {
      
        var data = new TowerRunData
        {
            nick = TowerStatsController.Nick,
            passos = TowerStatsController.Passos,
            ouro = TowerStatsController.Ouro,
            wumpusMortos = TowerStatsController.WumpusMortos,
            pontuacao = TowerStatsController.Pontuacao,
            tempoTotal = TowerStatsController.TempoTotal
        };

        TowerRankingManager.instancia?.AdicionarRegistro(data);
    }
}
