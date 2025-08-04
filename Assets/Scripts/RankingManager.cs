using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RankingManager : MonoBehaviour
{
    public static RankingManager instancia;
    public List<PlayerData> ranking = new();

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void AdicionarRegistro(PlayerData data)
    {
        ranking.Add(data);

        // Ordenar pelo menor tempo total e, em caso de empate, pela maior pontuação
        ranking = ranking
            .OrderBy(p => p.tempoTotal)
            .ThenByDescending(p => p.pontuacao)
            .ToList();
    }
}
