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
    public Vector3 boxSize = new Vector3(1f, 2f, 1f);

    [Header("Flecha")]
    public GameObject prefabFlecha;
    public Transform pontoDeDisparo;
    public float delayDisparo = 0.2f;

    [Header("Partículas")]
    public GameObject prefabParticulaColetar;
    public GameObject prefabParticulaMorte;
    public GameObject prefabParticulaMorteWumpus; //Partícula específica para morte pelo Wumpus
    public GameObject prefabParticulaRespawn;

    [Header("Respawn")]
    public Transform pontoDeSpawn;
    public float alturaYCorreta = 0f;

    private Animator animator;
    private Collider playerCollider;
    private Renderer[] renderers;

    private bool isMoving = false;
    private bool isDying = false;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private void Awake()
    {
        if (pontoDeSpawn == null)
        {
            pontoDeSpawn = new GameObject("PontoDeSpawnAuto").transform;
            pontoDeSpawn.position = Vector3.zero + PlayerGridGenerator.instancia.offsetCentroSala;
            Debug.LogWarning("Ponto de Spawn não estava configurado. Gerado automaticamente na sala (0,0).");
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

        bool salaValida = SalaExisteNaDirecao(direcao);

        if (playerCollider != null && salaValida)
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

        if (PlayerGridGenerator.instancia.gridInfo.ContainsKey(pos))
        {
            return PlayerGridGenerator.instancia.gridInfo[pos].temPoco;
        }
        return false;
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
        {
            Instantiate(prefabParticulaMorte, transform.position + Vector3.up * 1f, Quaternion.identity);
        }

        CameraFollow cam = FindFirstObjectByType<CameraFollow>();
        if (cam != null)
        {
            cam.DefinirAlvo(null);
            cam.MoverParaPonto(transform.position);
        }

        if (playerCollider != null)
            playerCollider.enabled = false;

        foreach (var rend in renderers)
            rend.enabled = true;

        float alturaFinal = transform.position.y - 4f;
        float velocidadeQueda = 5f;

        while (transform.position.y > alturaFinal)
        {
            transform.position -= new Vector3(0, velocidadeQueda * Time.deltaTime, 0);
            yield return null;
        }

        foreach (var rend in renderers)
            rend.enabled = false;

        yield return new WaitForSeconds(2f);

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
        {
            Instantiate(prefabParticulaMorteWumpus, transform.position + Vector3.up * 1f, Quaternion.identity);
        }

        CameraFollow cam = FindFirstObjectByType<CameraFollow>();
        if (cam != null)
        {
            cam.DefinirAlvo(null);
            cam.MoverParaPonto(transform.position);
        }

        if (playerCollider != null)
            playerCollider.enabled = false;

        foreach (var rend in renderers)
            rend.enabled = true;

        float alturaFinal = transform.position.y - 1.5f;
        float velocidadeDesaparecer = 2f;

        while (transform.position.y > alturaFinal)
        {
            transform.position -= new Vector3(0, velocidadeDesaparecer * Time.deltaTime, 0);
            yield return null;
        }

        foreach (var rend in renderers)
            rend.enabled = false;

        yield return new WaitForSeconds(2f);

        RespawnarPlayer();
    }

    void RespawnarPlayer()
    {
        transform.position = new Vector3(
            pontoDeSpawn.position.x,
            alturaYCorreta,
            pontoDeSpawn.position.z
        );

        targetPosition = transform.position;
        targetRotation = Quaternion.identity;
        transform.rotation = targetRotation;

        foreach (var rend in renderers)
            rend.enabled = true;

        if (playerCollider != null)
            playerCollider.enabled = true;

        if (prefabParticulaRespawn != null)
        {
            Instantiate(prefabParticulaRespawn, transform.position + Vector3.up * 1f, Quaternion.identity);
        }

        CameraFollow cam = FindFirstObjectByType<CameraFollow>();
        if (cam != null)
        {
            cam.DefinirAlvo(transform);
        }

        AtualizarSalaAtual();
        Debug.Log("Jogador respawnado na sala inicial.");

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
        {
            Instantiate(prefabFlecha, pontoDeDisparo.position, Quaternion.LookRotation(transform.forward));
        }
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(targetPosition, boxSize);
    }
}
