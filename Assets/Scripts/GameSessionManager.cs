using UnityEngine;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager instancia;

    public string nickDoJogador;
    public string personagemEscolhido;
    public string modoDeJogo; // "Classico" ou "TorreInfinita"

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
