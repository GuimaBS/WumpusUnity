using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuControllerPlayerMode : MonoBehaviour
{
    public void IniciarJogo()
    {
        string modoSelecionado = GameSessionManager.instancia.modoDeJogo;

        if (string.IsNullOrEmpty(modoSelecionado))
        {
            Debug.LogWarning("Nenhum modo de jogo foi selecionado!");
            return;
        }

        Debug.Log($"Modo selecionado: {modoSelecionado}");

        if (modoSelecionado == "Classico")
        {
            // Geração do mapa aleatório para ClassicScene
            int x = Random.Range(5, 21);
            int y = Random.Range(5, 21);

            PlayerPrefs.SetInt("mapX", x);
            PlayerPrefs.SetInt("mapY", y);
            PlayerPrefs.Save();

            Debug.Log($"Mapa aleatório gerado: {x}x{y} para ClassicScene");

            SceneManager.LoadScene("ClassicScene");
        }
        else if (modoSelecionado == "TorreInfinita")
        {
            Debug.Log("Iniciando modo Torre Infinita");
            SceneManager.LoadScene("TowerScene");
        }
        else
        {
            Debug.LogError($"Modo de jogo desconhecido: {modoSelecionado}");
        }
    }
}
