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
    public int mortes = 0;

    [Header("Colisão")]
    public LayerMask obstaculosLayer;

    [Header("Flecha")]
    public GameObject prefabFlecha;
    public Transform pontoDeDisparo;
    public float delayDisparo = 0.2f;

    [Header("Partículas")]
    public GameObject prefabParticulaColetar;
    public GameObject prefabParticulaMorte;
    public GameObject prefabParticulaMorteWumpus;
    public GameObject prefabParticulaRespawn;

    [Header("Respawn")]
    public float alturaExtraRespawn = 0.2f;
    public float offsetXRespawn = 0f;
    public float offsetZRespawn = 0f;
    public Vector3 offsetParticulaRespawn = new Vector3(0, 1f, 0);

    private Animator animator;
    private Collider playerCollider;
    private Renderer[] renderers;

    private bool isMoving = false;
    private bool isDying = false;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private void Awake()
    {
        VerificarRespawnPoint();
    }

    private void VerificarRespawnPoint()
    {
        if (RespawnPoint.instancia == null)
        {
            GameObject obj = new GameObject("RespawnPointAuto");
            obj.transform.position = Vector3.zero + PlayerGridGenerator.instancia.offsetCentroSala;
            obj.AddComponent<RespawnPoint>();
            Debug.LogWarning("Nenhum RespawnPoint encontrado. Criado automaticamente em (0,0).");
        }
    }

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
        playerCollider = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>();

        if (animator == null)
            Debug.LogWarning("Animator não encontrado!");

        targetPosition = transform.position;
        targetRotation = transform.rotation;

        AtualizarUI();
    }

    void Update()
    {
        if (isDying) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public void RotateLeft()
    {
        if (isMoving || isDying) return;
        targetRotation *= Quaternion.Euler(0, -90, 0);
    }

    public void RotateRight()
    {
        if (isMoving || isDying) return;
        targetRotation *= Quaternion.Euler(0, 90, 0);
    }

    public void MoveForward()
    {
        if (isMoving || isDying) return;

        Vector3 dir = new Vector3(
            Mathf.Round(transform.forward.x),
            0,
            Mathf.Round(transform.forward.z)
        );

        Vector3 destination = targetPosition + dir * moveDistance;

        if (!SalaExisteNaDirecao(dir))
        {
            Debug.Log("Tentativa de sair do mapa bloqueada!");
            return;
        }

        StartCoroutine(MoveToPosition(destination, dir));
    }

    bool SalaExisteNaDirecao(Vector3 direcao)
    {
        Vector2Int posAtual = new Vector2Int(
            Mathf.RoundToInt(targetPosition.x / moveDistance),
            Mathf.RoundToInt(targetPosition.z / moveDistance)
        );

        Vector2Int destino = posAtual + new Vector2Int(
            Mathf.RoundToInt(direcao.x),
            Mathf.RoundToInt(direcao.z)
        );

        return PlayerGridGenerator.instancia.gridInfo.ContainsKey(destino);
    }

    IEnumerator MoveToPosition(Vector3 destination, Vector3 direcao)
    {
        isMoving = true;
        targetPosition = destination;

        if (playerCollider != null)
            playerCollider.enabled = false;

        while (Vector3.Distance(transform.position, destination) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = destination;
        isMoving = false;

        if (playerCollider != null)
            playerCollider.enabled = true;

        AtualizarSalaAtual();

        if (EstaEmSalaComPoco())
        {
            StartCoroutine(MorrerNoPoco());
        }
        else if (EstaEmSalaComWumpus())
        {
            StartCoroutine(MorrerParaOWumpus());
        }
    }

    bool EstaEmSalaComPoco()
    {
        Vector2Int pos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / moveDistance),
            Mathf.RoundToInt(transform.position.z / moveDistance)
        );

        return PlayerGridGenerator.instancia.gridInfo.ContainsKey(pos) &&
               PlayerGridGenerator.instancia.gridInfo[pos].temPoco;
    }

    bool EstaEmSalaComWumpus()
    {
        Vector2Int pos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / moveDistance),
            Mathf.RoundToInt(transform.position.z / moveDistance)
        );

        return pos == PlayerGridGenerator.instancia.posicaoWumpus;
    }

    IEnumerator MorrerNoPoco()
    {
        Debug.Log("O jogador caiu no poço!");

        isDying = true;
        mortes++;
        AtualizarUI();

        animator?.SetTrigger("queda");

        if (prefabParticulaMorte != null)
            Instantiate(prefabParticulaMorte, transform.position + Vector3.up * 1f, Quaternion.identity);

        yield return new WaitForSeconds(1f);

        RespawnarPlayer();
    }

    IEnumerator MorrerParaOWumpus()
    {
        Debug.Log("O jogador foi devorado pelo Wumpus!");

        isDying = true;
        mortes++;
        AtualizarUI();

        animator?.SetTrigger("dwumpus");

        if (prefabParticulaMorteWumpus != null)
            Instantiate(prefabParticulaMorteWumpus, transform.position + Vector3.up * 1f, Quaternion.identity);

        yield return new WaitForSeconds(1f);

        RespawnarPlayer();
    }

    void RespawnarPlayer()
    {
        if (RespawnPoint.instancia == null)
        {
            Debug.LogError("RespawnPoint não está presente na cena!");
            return;
        }

        Transform pontoRespawn = RespawnPoint.instancia.transform;

        Vector3 posRespawn = new Vector3(
            pontoRespawn.position.x + offsetXRespawn,
            0f + alturaExtraRespawn,
            pontoRespawn.position.z + offsetZRespawn
        );

        transform.position = posRespawn;
        targetPosition = posRespawn;

        targetRotation = Quaternion.identity;
        transform.rotation = targetRotation;

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        foreach (var rend in renderers)
            rend.enabled = true;

        if (playerCollider != null)
            playerCollider.enabled = true;

        if (prefabParticulaRespawn != null)
            Instantiate(prefabParticulaRespawn, transform.position + offsetParticulaRespawn, Quaternion.identity);

        CameraFollow cam = FindFirstObjectByType<CameraFollow>();
        if (cam != null)
            cam.DefinirAlvo(transform);

        AtualizarSalaAtual();

        Debug.Log("Jogador respawnado na posição limpa sem Rigidbody.");

        isDying = false;
    }

    void AtualizarSalaAtual()
    {
        Vector2Int pos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / moveDistance),
            Mathf.RoundToInt(transform.position.z / moveDistance)
        );

        SalaManager.instancia?.AtualizarSalasAtivas(pos);
    }

    public void AtirarFlecha()
    {
        if (flechas <= 0 || isDying) return;

        flechas--;
        AtualizarUI();

        animator?.SetTrigger("Atirar");

        StartCoroutine(DispararFlechaComDelay());
    }

    IEnumerator DispararFlechaComDelay()
    {
        yield return new WaitForSeconds(delayDisparo);

        if (prefabFlecha != null && pontoDeDisparo != null)
            Instantiate(prefabFlecha, pontoDeDisparo.position, Quaternion.LookRotation(transform.forward));
    }

    public void ColetarOuro()
    {
        if (isDying) return;

        animator?.SetTrigger("Pick");

        Vector2Int posAtual = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / moveDistance),
            Mathf.RoundToInt(transform.position.z / moveDistance)
        );

        if (PlayerGridGenerator.instancia.gridInfo.ContainsKey(posAtual))
        {
            var info = PlayerGridGenerator.instancia.gridInfo[posAtual];

            if (info.temOuro)
            {
                ouro++;
                info.temOuro = false;

                GameObject sala = PlayerGridGenerator.instancia.mapaGerado[posAtual];

                Transform ouroObj = sala.transform.Find("ouro");
                if (ouroObj != null) Destroy(ouroObj.gameObject);

                Transform brilhoObj = sala.transform.Find("Brilho");
                if (brilhoObj != null) Destroy(brilhoObj.gameObject);

                if (prefabParticulaColetar != null)
                {
                    Vector3 posParticula = transform.position + Vector3.up * 1f;
                    Instantiate(prefabParticulaColetar, posParticula, Quaternion.identity);
                }

                AtualizarUI();
                Debug.Log("Ouro coletado!");
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
            UIManager.instancia.AtualizarMortes(mortes);
        }
    }
}
