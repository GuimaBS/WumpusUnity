using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TowerRankingManager : MonoBehaviour
{
    public static TowerRankingManager instancia;
    public List<TowerPlayerData> ranking = new();

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void AdicionarRegistro(TowerPlayerData data)
    {
        ranking.Add(data);

        ranking = ranking
            .OrderByDescending(p => p.pontuacao)
            .ThenByDescending(p => p.wumpus)
            .ThenByDescending(p => p.ouros)
            .ThenBy(p => p.tempoTotal)
            .ToList();
    }

    public void LimparRanking() => ranking.Clear();
}
