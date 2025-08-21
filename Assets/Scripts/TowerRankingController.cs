using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TowerRankingController
{
    public static void SalvarDados()
    {
        TowerPlayerData novo = new TowerPlayerData
        {
            nick       = TowerStatsController.nick,
            passos     = TowerStatsController.passos,
            tempoTotal = TowerStatsController.tempoTotal,
            pontuacao  = TowerStatsController.pontuacao,
            ouros      = TowerStatsController.ouros,
            wumpus     = TowerStatsController.wumpus
        };

        TowerRankingManager.instancia?.AdicionarRegistro(novo);
    }

    public static List<TowerPlayerData> ObterRankingOrdenado()
    {
        if (TowerRankingManager.instancia == null) return new List<TowerPlayerData>();
        return TowerRankingManager.instancia.ranking
            .OrderByDescending(p => p.pontuacao)
            .ThenByDescending(p => p.wumpus)
            .ThenByDescending(p => p.ouros)
            .ThenBy(p => p.tempoTotal)
            .ToList();
    }
}
