using UnityEngine;
using TMPro;
using System.Linq;

public class TabelaDePontosTowerUI : MonoBehaviour
{
    public TMP_Text painelRanking;

    void Start()
    {
        // comita a run se ainda não foi salva
        TowerStatsController.TryCommitToRanking();

        AtualizarPainelRanking();
    }

    public void AtualizarPainelRanking()
    {
        painelRanking.text = "";

        var rankingOrdenado = TowerRankingManager.instancia.ranking
            .OrderByDescending(d => d.pontuacao)
            .ThenByDescending(d => d.wumpusMortos)
            .ThenByDescending(d => d.ouro)
            .ThenBy(d => d.passos)
            .ToList();

        int posicao = 1;

        painelRanking.text +=
            "Pos".PadRight(8) +
            "Nick".PadRight(14) +
            "Ouro".PadRight(10) +
            "Wumpus".PadRight(10) +
            "Passos".PadRight(12) +
            "Tempo(s)".PadRight(17) +
            "Pontos".PadRight(14) + "\n";

        painelRanking.text += new string('-', 92) + "\n";

        foreach (var d in rankingOrdenado)
        {
            string linha = "";
            linha += $"{posicao++}".PadRight(10);
            linha += $"{(string.IsNullOrEmpty(d.nick) ? "—" : d.nick)}".PadRight(15);
            linha += $"{d.ouro}".PadRight(12);
            linha += $"{d.wumpusMortos}".PadRight(12);
            linha += $"{d.passos}".PadRight(12);
            linha += $"{d.tempoTotal:F2}".PadRight(18);
            linha += $"{d.pontuacao}".PadRight(15);
            painelRanking.text += linha + "\n";
        }
    }
}
