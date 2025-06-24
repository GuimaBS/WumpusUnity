using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instancia;

    [Header("Textos UI")]
    public TMP_Text textoFlechas;
    public TMP_Text textoOuro;
    public TMP_Text textoMortes;

    private void Awake()
    {
        if (instancia == null)
            instancia = this;
        else
            Destroy(gameObject);
    }

    public void AtualizarFlechas(int qtd)
    {
        textoFlechas.text = qtd.ToString();
    }

    public void AtualizarOuro(int qtd)
    {
        textoOuro.text = qtd.ToString();
    }

    public void AtualizarMortes(int qtd)
    {
        textoMortes.text = qtd.ToString();
    }
}
