using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Refer�ncia ao Player")]
    public PlayerMovement playerAtual;

    [Header("Configura��es de Busca")]
    public bool buscarAutomaticamente = true;
    public float delayDeBusca = 0.2f; // Tempo de espera para garantir que o player foi instanciado

    public static PlayerController instancia;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
        else if (instancia != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (buscarAutomaticamente)
        {
            StartCoroutine(BuscarPlayerComDelay());
        }
    }

    IEnumerator BuscarPlayerComDelay()
    {
        yield return new WaitForSeconds(delayDeBusca);

        PlayerMovement playerEncontrado = FindFirstObjectByType<PlayerMovement>(); // M�todo atualizado

        if (playerEncontrado != null)
        {
            playerAtual = playerEncontrado;
            Debug.Log($"PlayerController vinculado automaticamente ao player: {playerAtual.name}");
        }
        else
        {
            Debug.LogWarning("Nenhum Player encontrado na cena ap�s o delay.");
        }
    }

    public void ComandoGirarEsquerda()
    {
        if (playerAtual != null) playerAtual.RotateLeft();
        else Debug.LogWarning("Nenhum Player vinculado para executar RotateLeft.");
    }

    public void ComandoGirarDireita()
    {
        if (playerAtual != null) playerAtual.RotateRight();
        else Debug.LogWarning("Nenhum Player vinculado para executar RotateRight.");
    }

    public void ComandoAndarFrente()
    {
        if (playerAtual != null) playerAtual.MoveForward();
        else Debug.LogWarning("Nenhum Player vinculado para executar MoveForward.");
    }

    public void ComandoAtirar()
    {
        if (playerAtual != null) playerAtual.AtirarFlecha();
        else Debug.LogWarning("Nenhum Player vinculado para executar AtirarFlecha.");
    }

    public void ComandoColetarOuro()
    {
        if (playerAtual != null) playerAtual.ColetarOuro();
        else Debug.LogWarning("Nenhum Player vinculado para executar ColetarOuro.");
    }
}
