using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("Imagem do Personagem na HUD")]
    public Image imagemPersonagem;

    [Header("Sprites dos Personagens")]
    public Sprite spriteArqueiro;
    public Sprite spriteAmazona;

    void Start()
    {
        AtualizarImagemPersonagem();
    }

    public void AtualizarImagemPersonagem()
    {
        string personagem = GameSessionManager.instancia.personagemEscolhido;

        if (personagem == "arqueiro")
        {
            imagemPersonagem.sprite = spriteArqueiro;
            Debug.Log("Imagem do Arqueiro exibida na HUD.");
        }
        else if (personagem == "amazona")
        {
            imagemPersonagem.sprite = spriteAmazona;
            Debug.Log("Imagem da Amazona exibida na HUD.");
        }
        else
        {
            Debug.LogWarning("Personagem não reconhecido para exibir na HUD.");
        }
    }
}
