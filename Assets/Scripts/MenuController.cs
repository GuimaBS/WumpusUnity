using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public InputField inputX;
    public InputField inputY;
    public GameObject warningPanel;

    public void GerarMapa()
    {
        int x, y;

        Debug.Log("Tentando gerar mapa...");

        // Tenta converter os textos digitados
        bool xOk = int.TryParse(inputX.text, out x);
        bool yOk = int.TryParse(inputY.text, out y);

        // Se não for número ou for abaixo do mínimo permitido
        if (x <= 3 || y <= 3 || x > 20 || y > 20)
        {
            Debug.Log("Tamanho de mapa inválido.");
            warningPanel.SetActive(true);
            return;
        }

        PlayerPrefs.SetInt("mapX", x);
        PlayerPrefs.SetInt("mapY", y);
        SceneManager.LoadScene("GameScene");
    }

    public void GerarMapaAleatorio()
    {
        int x = Random.Range(5, 21); // de 5 a 20
        int y = Random.Range(5, 21);

        PlayerPrefs.SetInt("mapX", x);
        PlayerPrefs.SetInt("mapY", y);
        PlayerPrefs.Save();

        Debug.Log($"Mapa aleatório gerado: {x}x{y}");

        SceneManager.LoadScene("GameScene");

    }
    public void FecharAviso()
    {
        warningPanel.SetActive(false);
    }
}
