using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class gerarCSV : MonoBehaviour
{
    private List<string> eventos = new List<string>();

    private string caminhoArquivo;

    private void Awake()
    {
        string pasta = Application.dataPath + "/Logs";
        if (!Directory.Exists(pasta))
            Directory.CreateDirectory(pasta);

        caminhoArquivo = Path.Combine(pasta, "log_mortes.csv");
    }

    public void RegistrarEvento(string evento, Vector2Int posicao, string agente)
    {
        string linha = $"{evento};{agente};{posicao};{Time.time}";
        eventos.Add(linha);
        Debug.Log($"Evento armazenado na memória: {linha}");
    }

    public void BaixarLog()
    {
        if (!File.Exists(caminhoArquivo))
        {
            // Cria o arquivo com cabeçalho
            File.WriteAllText(caminhoArquivo, "Evento;Agente;Posicao;Tempo(ms)\n");
        }
        
        // Adiciona eventos registrados
        File.AppendAllLines(caminhoArquivo, eventos);
        Debug.Log("Log salvo em: " + caminhoArquivo);

        // Limpa os eventos da memória após salvar
        eventos.Clear();
    }
}