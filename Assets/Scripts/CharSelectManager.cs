using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharSelectManager : MonoBehaviour
{
    [Header("Referências de UI")]
    public InputField inputNick;
    public GameObject panelSelecaoModo;
    public string nickDoJogador;

    public static class Cena
    {
        public const string ClassicScene = "ClassicScene";
        public const string TowerScene = "TowerScene";
    }


    public void AbrirPanelModo()
    {
        panelSelecaoModo.SetActive(true);
    }

    public void FecharPanelModo()
    {
        panelSelecaoModo.SetActive(false);
    }

    public void SalvarNick()
    {
        string nick = inputNick.text;

        if (string.IsNullOrEmpty(nick))
        {
            Debug.LogWarning("Nick vazio! Digite um nome.");
            return;
        }

        GameSessionManager.instancia.nickDoJogador = nick;
        Debug.Log("Nick salvo: " + nick);
    }

    public void SelecionarPersonagem(string nomePersonagem)
    {
        GameSessionManager.instancia.personagemEscolhido = nomePersonagem;
        Debug.Log("Personagem selecionado: " + nomePersonagem);
    }

    public void SelecionarModo(string modo)
    {
        GameSessionManager.instancia.modoDeJogo = modo;
        Debug.Log("Modo de jogo selecionado: " + modo);
    }

    public void IniciarJogo()
    {
        SalvarNick();

        string modo = GameSessionManager.instancia.modoDeJogo;

        if (string.IsNullOrEmpty(modo))
        {
            Debug.LogError("Nenhum modo de jogo selecionado!");
            return;
        }

        if (modo == "Classico")
        {
            Debug.Log("Iniciando Modo Clássico...");
            SceneManager.LoadScene("ClassicScene");
        }
        else if (modo == "TorreInfinita")
        {
            Debug.Log("Iniciando Modo Torre Infinita...");
            SceneManager.LoadScene("TowerScene");
        }
        else
        {
            Debug.LogError("Modo de jogo inválido ou não reconhecido: " + modo);
        }
    }
}
