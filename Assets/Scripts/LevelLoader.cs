using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [Header("Transição")]
    public Animator transition;
    public float transitionTime = 1f;

    [Header("Configuração de Saltos Específicos")]
    public List<int> indicesOrigem;
    public List<int> indicesDestino;

    public int indiceGameScene = 2;

    private void Start()
    {
        if (indicesOrigem.Count != indicesDestino.Count)
        {
            Debug.LogError("As listas de origem e destino devem ter o mesmo tamanho!");
        }
    }

    public void LoadNextLevel()
    {
        int cenaAtual = SceneManager.GetActiveScene().buildIndex;
        int proximaCena = VerificarSalto(cenaAtual, "proxima");
        StartCoroutine(LoadLevel(proximaCena));
    }

    public void LevelBack()
    {
        int cenaAtual = SceneManager.GetActiveScene().buildIndex;
        int cenaAnterior = VerificarSalto(cenaAtual, "anterior");
        StartCoroutine(LoadLevel(cenaAnterior));
    }

    public void LoadSpecificLevel(int sceneIndex)
    {
        string nomeCena = SceneManager.GetSceneByBuildIndex(sceneIndex).name;

        if (nomeCena == "GameScene")
        {
            int x = PlayerPrefs.GetInt("mapX", -1);
            int y = PlayerPrefs.GetInt("mapY", -1);

            // Verifica se os valores são válidos (mínimo 4 e máximo 20)
            if (x < 4 || y < 4 || x > 20 || y > 20)
            {
                Debug.LogWarning("[LevelLoader] Parâmetros do mapa inválidos. Cancelando transição para GameScene.");
                return;
            }
        }

        StartCoroutine(LoadLevel(sceneIndex));
    }


    IEnumerator LoadLevel(string nomeCena)
    {
        if (transition != null)
        {
            transition.SetTrigger("Start");
            yield return new WaitForSeconds(transitionTime);
        }

        SceneManager.LoadScene(nomeCena);
    }


    IEnumerator LoadLevel(int levelIndex)
    {
        if (transition != null)
        {
            transition.SetTrigger("Start");
            yield return new WaitForSeconds(transitionTime);
        }

        SceneManager.LoadScene(levelIndex);
    }

    private int VerificarSalto(int indexAtual, string sentido)
    {
        for (int i = 0; i < indicesOrigem.Count; i++)
        {
            if (indicesOrigem[i] == indexAtual)
            {
                return indicesDestino[i]; // Aplica o salto configurado
            }
        }

        if (sentido == "proxima")
        {
            return indexAtual + 1;
        }
        else
        {
            return indexAtual - 1;
        }
    }

    private bool ParametrosMapaValidos()
    {
        int x = PlayerPrefs.GetInt("mapX", -1);
        int y = PlayerPrefs.GetInt("mapY", -1);
        return x >= 4 && y >= 4 && x <= 20 && y <= 20;
    }

}
