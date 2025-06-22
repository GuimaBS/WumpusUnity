using UnityEngine;
using TMPro;

public class ExibirNickUI : MonoBehaviour
{
    public TMP_Text textoNick;

    void Start()
    {
        string nick = GameSessionManager.instancia.nickDoJogador;

        if (!string.IsNullOrEmpty(nick))
        {
            textoNick.text = nick;
        }
        else
        {
            textoNick.text = "Jogador";
        }
    }
}
