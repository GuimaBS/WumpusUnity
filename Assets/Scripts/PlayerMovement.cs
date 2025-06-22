using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimentação")]
    public float moveDistance = 10f; // Deve ser igual ao espaçamento entre salas
    public float moveSpeed = 5f;
    public float rotationSpeed = 300f;

    [Header("Inventário")]
    public int flechas = 3;
    public int ouro = 0;

    [Header("UI")]
    public Text flechasText;
    public Text ouroText;

    [Header("Configuração de Colisão")]
    public LayerMask obstaculosLayer; // Camada dos obstáculos
    public Vector3 boxSize = new Vector3(1f, 2f, 1f); // Tamanho da caixa para detecção

    private bool isMoving = false;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    void Start()
    {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        AtualizarUI();
    }

    void Update()
    {
        // Suaviza movimentação e rotação
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public void RotateLeft()
    {
        if (isMoving) return;
        targetRotation *= Quaternion.Euler(0, -90, 0);
    }

    public void RotateRight()
    {
        if (isMoving) return;
        targetRotation *= Quaternion.Euler(0, 90, 0);
    }

    public void MoveForward()
    {
        if (isMoving) return;

        Vector3 direction = new Vector3(Mathf.Round(transform.forward.x), 0, Mathf.Round(transform.forward.z));
        Vector3 destination = targetPosition + direction * moveDistance;

        if (PodeMoverPara(destination))
        {
            StartCoroutine(MoveToPosition(destination));
        }
        else
        {
            Debug.Log("Movimento bloqueado por obstáculo!");
        }
    }

    IEnumerator MoveToPosition(Vector3 destination)
    {
        isMoving = true;
        targetPosition = destination;

        while (Vector3.Distance(transform.position, destination) > 0.01f)
        {
            yield return null;
        }

        transform.position = destination;
        isMoving = false;
    }

    // Função para checar se há obstáculo na tile de destino
    bool PodeMoverPara(Vector3 destino)
    {
        Collider[] colisores = Physics.OverlapBox(
            destino,
            boxSize / 2,
            Quaternion.identity,
            obstaculosLayer
        );

        return colisores.Length == 0;
    }

    public void AtirarFlecha()
    {
        if (flechas <= 0)
        {
            Debug.Log("Sem flechas!");
            return;
        }

        flechas--;
        Debug.Log("Flecha disparada!");

        AtualizarUI();
    }

    public void ColetarOuro()
    {
        ouro++;
        Debug.Log("Ouro coletado!");

        AtualizarUI();
    }

    void AtualizarUI()
    {
        if (flechasText != null) flechasText.text = "Flechas: " + flechas;
        if (ouroText != null) ouroText.text = "Ouro: " + ouro;
    }

    //  Visualização da caixa de colisão no editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(targetPosition, boxSize);
    }
}
