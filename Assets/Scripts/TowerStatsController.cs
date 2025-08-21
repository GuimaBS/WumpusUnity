using UnityEngine;

public static class TowerStatsController
{
    public static string nick = "";
    public static int passos = 0;
    public static int ouros = 0;
    public static int wumpus = 0;
    public static int pontuacao = 0;
    public static float tempoTotal = 0f;

    public static void ResetarTodos()
    {
        nick = CharSelectManager.nickJogador; 
        passos = 0;
        ouros = 0;
        wumpus = 0;
        pontuacao = 0;
        tempoTotal = 0f;
    }

    public static void TickTempo(float dt) { tempoTotal += dt; }

    public static void AddPasso()  { passos++; }
    public static void AddOuro()   { ouros++; }
    public static void AddWumpus() { wumpus++; }

    public static void SetPontuacao(int valor) { pontuacao = valor; }
    public static void AddPontuacao(int delta) { pontuacao += delta; }
}
