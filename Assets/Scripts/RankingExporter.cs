using System.IO;
using UnityEngine;
using UnityEditor;

public class RankingExporter : MonoBehaviour
{
    // Arraste seu TMP_Text (ou Text) do painel de ranking aqui no Inspector
    [SerializeField] private TMPro.TMP_Text painelRanking;

    public void ExportarRankingClassico() => ExportarRankingParaTXT(ModoJogo.Classico);
    public void ExportarRankingTorre() => ExportarRankingParaTXT(ModoJogo.Torre);

    private enum ModoJogo { Classico, Torre }

    private void ExportarRankingParaTXT(ModoJogo modo)
    {
        if (painelRanking == null)
        {
            Debug.LogError("painelRanking não está atribuído no Inspector.");
            return;
        }

        string conteudo = painelRanking.text ?? string.Empty;

        string baseDir;

 
        baseDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "RankingPlayer"));

        baseDir = Path.Combine(Application.persistentDataPath, "RankingPlayer");

        // Subpasta conforme o modo
        string subdir = (modo == ModoJogo.Classico) ? "Classico" : "Torre";

        // Caminho final da pasta e do arquivo
        string targetDir = Path.Combine(baseDir, subdir);
        string targetFile = Path.Combine(targetDir, "ranking.txt");

        try
        {
            // Criação idempotente: se existir, não duplica
            Directory.CreateDirectory(targetDir);

            File.WriteAllText(targetFile, conteudo);
            Debug.Log($"Ranking ({modo}) exportado para: {targetFile}");

              AssetDatabase.Refresh();

        }
        catch (System.Exception e)
        {
            Debug.LogError($"Erro ao exportar ranking ({modo}): {e.Message}");
        }
    }
}
