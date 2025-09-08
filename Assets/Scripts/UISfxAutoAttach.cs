using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-250)]
public class UISfxAutoAttach : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // Cena atual também:
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Percorre objetos de raiz e pega Buttons inclusive inativos
        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            var btns = roots[i].GetComponentsInChildren<Button>(true);
            for (int j = 0; j < btns.Length; j++)
            {
                var go = btns[j].gameObject;
                if (!go.GetComponent<UIButtonSfxHook>())
                    go.AddComponent<UIButtonSfxHook>();
            }
        }
    }
}
