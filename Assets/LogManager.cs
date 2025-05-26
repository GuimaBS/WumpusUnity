using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LogManager : MonoBehaviour
{
    public static LogManager instancia;
    public TextMeshProUGUI logText;
    private string logCompleto = "";
    public ScrollRect scrollRect; 

    private void Awake()
    {
        if (instancia == null)
            instancia = this;
    }

    public void AdicionarLog(string mensagem)
    {
        logCompleto += $"<color=#FFD700>[{System.DateTime.Now:HH:mm:ss}]</color> {mensagem}\n";
        logText.text = logCompleto;
       
        // Força rolagem automática para o fim do log
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void LimparLog()
    {
        logCompleto = "";
        logText.text = "";
    }
}
