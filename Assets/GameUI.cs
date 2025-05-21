using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public void VoltarAoMenu()
    {
        SceneManager.LoadScene("MenuScene"); 
    }
}
