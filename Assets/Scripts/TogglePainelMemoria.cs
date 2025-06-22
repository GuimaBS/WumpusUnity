using UnityEngine;

public class TogglePainelMemoria : MonoBehaviour
{
    public GameObject JanelaMemoria;

    private bool visivel = false;

    public void AlternarPainel()
    {
        visivel = !visivel;
        JanelaMemoria.SetActive(visivel);
    }
}
