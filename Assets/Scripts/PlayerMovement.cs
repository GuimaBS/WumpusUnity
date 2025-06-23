using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movimentação")]
    public float moveDistance = 10f;
    public float moveSpeed = 20f;
    public float rotationSpeed = 400f;

    [Header("Inventário")]
    public int flechas = 3;
    public int ouro = 0;

    [Header("Colisão")]
    public LayerMask obstaculosLayer;
    public Vector3 boxSize = new Vector3(1f, 2f, 1f);

    [Header("Flecha")]
    public GameObject prefabFlecha;
    public Transform pontoDeDisparo;
    public float delayDisparo = 0.2f;

    private Animator animator;
    private bool isMoving = false;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private void OnEnable()
    {
        PlayerGridGenerator.OnMapaGerado += AtualizarSalaAtual;
    }

    private void OnDisable()
    {
        PlayerGridGenerator.OnMapaGerado -= AtualizarSalaAtual;
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        if (animator == null)
        {
            Debug.LogWarning("Animator não encontrado no filho deste Player!");
        }

        targetPosition = transform.position;
        targetRotation = transform.rotation;
        AtualizarUI();
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public void RotateLeft()
    {
        if (isMoving) return;


        targetRotation *= Quaternion.Euler(0, -290, 0);
    }

    public void RotateRight()
    {
        if (isMoving) return;

        targetRotation *= Quaternion.Euler(0, 90, 0);
    }

    public void MoveForward()
    {
        if (isMoving) return;

        Vector3 dir = new Vector3(
            Mathf.Round(transform.forward.x),
            0,
            Mathf.Round(transform.forward.z)
        );

        Vector3 destination = targetPosition + dir * moveDistance;

        StartCoroutine(MoveToPosition(destination));
    }

    IEnumerator MoveToPosition(Vector3 destination)
    {
        isMoving = true;
        targetPosition = destination;

        Collider col = GetComponent<Collider>();
        if (col) col.enabled = false;

        while (Vector3.Distance(transform.position, destination) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = destination;
        isMoving = false;

        if (col) col.enabled = true;

        AtualizarSalaAtual();
    }

    void AtualizarSalaAtual()
    {
        Vector2Int pos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / moveDistance),
            Mathf.RoundToInt(transform.position.z / moveDistance)
        );

        if (SalaManager.instancia != null)
        {
            SalaManager.instancia.AtualizarSalasAtivas(pos);
        }
    }

    public void AtirarFlecha()
    {
        if (flechas <= 0)
        {
            Debug.Log("Sem flechas!");
            return;
        }

        flechas--;
        AtualizarUI();

        Debug.Log("Flecha disparada!");

        animator?.SetTrigger("Atirar");

        StartCoroutine(DispararFlechaComDelay());
    }

    IEnumerator DispararFlechaComDelay()
    {
        yield return new WaitForSeconds(delayDisparo);

        if (prefabFlecha != null && pontoDeDisparo != null)
        {
            Instantiate(
                prefabFlecha,
                pontoDeDisparo.position,
                Quaternion.LookRotation(transform.forward)
            );

            Debug.Log("Flecha instanciada.");
        }
        else
        {
            Debug.LogWarning("Prefab de flecha ou ponto de disparo não configurado.");
        }
    }

    public void ColetarOuro()
    {
        animator?.SetTrigger("Pick");

        Collider[] cols = Physics.OverlapSphere(transform.position + Vector3.up * 0.5f, 1f);

        foreach (var col in cols)
        {
            if (col.CompareTag("ouro"))
            {
                ouro++;
                Destroy(col.gameObject);
                Debug.Log("Ouro coletado!");
                AtualizarUI();
                return;
            }
        }

        Debug.Log("Nenhum ouro nesta sala.");
    }

    void AtualizarUI()
    {
        if (UIManager.instancia != null)
        {
            UIManager.instancia.AtualizarFlechas(flechas);
            UIManager.instancia.AtualizarOuro(ouro);
        }
        else
        {
            Debug.LogWarning("UIManager não encontrado na cena!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(targetPosition, boxSize);
    }
}
