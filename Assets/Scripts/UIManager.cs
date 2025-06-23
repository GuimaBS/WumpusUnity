using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instancia;

    [Header("Referências de UI")]
    public TMP_Text flechasText;
    public TMP_Text ouroText;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // Atualiza a quantidade de flechas na UI.

    public void AtualizarFlechas(int quantidade)
    {
        if (flechasText != null)
        {
            flechasText.text = quantidade.ToString();
        }
        else
        {
            Debug.LogWarning("UIManager: flechasText não atribuído no inspector.");
        }
    }


    //Atualiza a quantidade de ouro na UI.

    public void AtualizarOuro(int quantidade)
    {
        if (ouroText != null)
        {
            ouroText.text = quantidade.ToString();
        }
        else
        {
            Debug.LogWarning("UIManager: ouroText não atribuído no inspector.");
        }
    }
}
