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
        StartCoroutine(LoadLevel(sceneIndex));
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
}
