using System.IO;
using UnityEngine;
using TMPro;
using System.Linq;

public class TabelaDePontosUI : MonoBehaviour
{
    public TMP_Text painelRanking;

    void Start()
    {
        AtualizarPainelRanking();
    }

    public void AtualizarPainelRanking()
    {
        painelRanking.text = "";

        var rankingOrdenado = RankingManager.instancia.ranking
            .OrderBy(d => d.tempoTotal)
            .ThenByDescending(d => d.pontuacao)
            .ToList();

        int posicao = 1;

        painelRanking.text += "Pos    ".PadRight(5) +
                              "    Nick   ".PadRight(10) +
                              "     Passos   ".PadRight(8) +
                              "    Mortes      ".PadRight(8) +
                              "      Tempo(s)     ".PadRight(13) +
                              "      Pontos   ".PadRight(14) + "\n";

        painelRanking.text += new string('-', 96) + "\n";

        foreach (var data in rankingOrdenado)
        {
            string linha = $"{posicao++.ToString().PadRight(10)}";
            linha += $"{data.nick.PadRight(19)}";
            linha += $"{data.passos.ToString().PadRight(19)}";
            linha += $"{data.mortes.ToString().PadRight(18)}";
            linha += $"{data.tempoTotal.ToString("F2").PadRight(18)}";
            linha += $"{data.pontuacao.ToString().PadRight(16)}\n";

            painelRanking.text += linha;
        }
    }

}