using UnityEngine;
using TMPro;
using System.Linq;
using System.IO;

public class TabelaDePontosTowerUI : MonoBehaviour
{
    public TMP_Text painelRanking;

    void Start()
    {
        AtualizarPainelRanking();
    }

    public void AtualizarPainelRanking()
    {
        painelRanking.text = "";

        var ranking = TowerRankingController.ObterRankingOrdenado();

        int posicao = 1;

        // Cabeçalho
        painelRanking.text += 
            "Pos".PadRight(5) +
            "Nick".PadRight(14) +
            "Passos".PadRight(10) +
            "Wumpus".PadRight(10) +
            "Ouros".PadRight(9) +
            "Tempo(s)".PadRight(12) +
            "Pontos".PadRight(10) + "\n";

        painelRanking.text += new string('-', 90) + "\n";

        foreach (var d in ranking)
        {
            string linha = "";
            linha += posicao++.ToString().PadRight(5);
            linha += (d.nick ?? "").PadRight(14);
            linha += d.passos.ToString().PadRight(10);
            linha += d.wumpus.ToString().PadRight(10);
            linha += d.ouros.ToString().PadRight(9);
            linha += d.tempoTotal.ToString("F2").PadRight(12);
            linha += d.pontuacao.ToString().PadRight(10);
            linha += "\n";
            painelRanking.text += linha;
        }
    }

    public void ExportarRankingParaTXT()
    {
#if UNITY_EDITOR
        string caminho = @"E:\Unity\Projetos UNITY\WumpusUnity\RankingTower\ranking_tower.txt";
#else
        string caminho = Path.Combine(Application.persistentDataPath, "ranking_tower.txt");
#endif
        try
        {
            File.WriteAllText(caminho, painelRanking.text);
            Debug.Log($"[TowerRanking] Exportado para: {caminho}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TowerRanking] Erro ao exportar: {e.Message}");
        }
    }
}
