using UnityEngine;

public class TowerStatsController : MonoBehaviour
{
    public static TowerStatsController instancia;

    public static string Nick => CharSelectManager.nickJogador;

    public static int Passos { get; private set; }
    public static int Ouro { get; private set; }
    public static int Wumpus { get; private set; }                 
    public static int WumpusMortos => Wumpus;                      
    public static int Pontuacao { get; private set; }
    public static float TempoTotal { get; private set; }          

    private static bool commitPendente = true;

    private void Awake()
    {
        if (instancia == null) { instancia = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public static void ResetarTodos()
    {
        Passos = 0;
        Ouro = 0;
        Wumpus = 0;
        Pontuacao = 0;
        TempoTotal = 0f;
        commitPendente = true;
    }

    public static void TickTempo(float dt)
    {
        TempoTotal += Mathf.Max(0f, dt);
    }

    public static void AddPasso() => Passos++;
    public static void AddOuro() => Ouro++;
    public static void AddWumpus() => Wumpus++;
    public static void SetPontuacao(int p) => Pontuacao = p;

    // -------- Build/Commit --------
    public static TowerRunData BuildData()
    {
        return new TowerRunData
        {
            nick = string.IsNullOrEmpty(Nick) ? "—" : Nick,
            passos = Passos,
            ouro = Ouro,
            wumpusMortos = WumpusMortos,
            pontuacao = Pontuacao,
            tempoTotal = TempoTotal,
        };
    }

    public static bool TryCommitToRanking()
    {
        if (!commitPendente) return false;
        var data = BuildData();
        TowerRankingManager.instancia?.AdicionarRegistro(data);
        commitPendente = false;
        return true;
    }
}
