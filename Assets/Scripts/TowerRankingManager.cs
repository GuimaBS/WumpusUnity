using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TowerRankingManager : MonoBehaviour
{
    public static TowerRankingManager instancia;
    public List<TowerRunData> ranking = new();

    private void Awake()
    {
        if (instancia == null) { instancia = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void AdicionarRegistro(TowerRunData data)
    {
        ranking.Add(data);

  
        ranking = ranking
            .OrderByDescending(r => r.pontuacao)
            .ThenByDescending(r => r.wumpusMortos)
            .ThenByDescending(r => r.ouro)
            .ThenBy(r => r.passos)
            .ThenBy(r => r.tempoTotal) 
            .ToList();
    }
}
