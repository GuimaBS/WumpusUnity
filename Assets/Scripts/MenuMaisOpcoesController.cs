using UnityEngine;

public class MenuMaisOpcoesController : MonoBehaviour
{
    public GameObject menuMaisOpcoes;

    public void AlternarMenu()
    {
        if (menuMaisOpcoes != null)
        {
            bool estaAtivo = menuMaisOpcoes.activeSelf;
            menuMaisOpcoes.SetActive(!estaAtivo);
        }
    }
}
