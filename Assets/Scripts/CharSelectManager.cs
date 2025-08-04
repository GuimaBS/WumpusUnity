using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CharSelectManager : MonoBehaviour
{
    [Header("Referências de UI")]
    public InputField inputNick;
    public InputField inputX;
    public InputField inputY;
    public GameObject panelSelecaoModo;
    public GameObject painelTamanhoMapa;
    public GameObject painelAvisoTamanhoInvalido;
    public GameObject painelAvisoTorre; // Novo painel

    public static string nickJogador;

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
        nickJogador = nick;
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

        if (modo == "Classico")
        {
            painelTamanhoMapa.SetActive(true);
            painelAvisoTorre.SetActive(false);
        }
        else if (modo == "TorreInfinita")
        {
            painelTamanhoMapa.SetActive(false);
            painelAvisoTorre.SetActive(true); // Ativa painel de aviso da torre
        }
    }

    public bool SalvarTamanhoDoMapa()
    {
        int x, y;
        bool xValido = int.TryParse(inputX.text, out x);
        bool yValido = int.TryParse(inputY.text, out y);

        // Geração aleatória se estiver vazio
        if (!xValido || !yValido)
        {
            x = Random.Range(4, 21);
            y = Random.Range(4, 21);
        }

        if (x < 4 || x > 20 || y < 4 || y > 20)
        {
            Debug.LogWarning("[CharSelect] Valores inválidos para o mapa!");
            return false;
        }

        PlayerPrefs.SetInt("mapX", x);
        PlayerPrefs.SetInt("mapY", y);

        Debug.Log($"[CharSelect] Tamanho do mapa salvo: {x} x {y}");
        return true;
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
            bool tamanhoValido = SalvarTamanhoDoMapa();
            if (!tamanhoValido)
            {
                painelAvisoTamanhoInvalido.SetActive(true);
                return;
            }

            Debug.Log("Iniciando Modo Clássico...");
            SceneManager.LoadScene(Cena.ClassicScene);
        }
        else if (modo == "TorreInfinita")
        {
            Debug.Log("Iniciando Modo Torre Infinita...");
            SceneManager.LoadScene(Cena.TowerScene);
        }
        else
        {
            Debug.LogError("Modo de jogo inválido ou não reconhecido: " + modo);
        }
    }

    public void FecharAviso()
    {
        painelAvisoTamanhoInvalido.SetActive(false);
    }

    public void FecharTamanho()
    {
        painelTamanhoMapa.SetActive(false);
    }

    public void FecharAvisoTorre()
    {
        painelAvisoTorre.SetActive(false);
    }
}
