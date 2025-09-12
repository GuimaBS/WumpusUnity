using UnityEngine;
using TMPro;

public class Agent3LabRow : MonoBehaviour
{
    [SerializeField] TMP_Text colRank;
    [SerializeField] TMP_Text colGen;
    [SerializeField] TMP_Text colAvg;
    [SerializeField] TMP_Text colBest;
    [SerializeField] TMP_Text colCx;
    [SerializeField] TMP_Text colMut;

    public float Best { get; private set; }
    public int Gen { get; private set; }

    public void Set(int rank, Agent3GA.GenStats s)
    {
        Best = s.bestGen; // propriedade interna usada para sort, se você ainda usar
        Gen = s.gen;

        if (colRank) colRank.text = rank.ToString();
        if (colGen) colGen.text = s.gen.ToString();
        if (colAvg) colAvg.text = s.avg.ToString("0.0");
        if (colBest) colBest.text = s.bestGen.ToString("0.0"); // mostra o melhor da geração
        if (colCx) colCx.text = (s.cx * 100f).ToString("0") + "%";
        if (colMut) colMut.text = (s.mut * 100f).ToString("0") + "%";
    }
}
