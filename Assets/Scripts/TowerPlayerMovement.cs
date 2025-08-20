using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TowerPlayerMovement : MonoBehaviour
{
    [Header("Movimentação")]
    public float moveDistance = 10f;
    public float moveSpeed = 20f;
    public float rotationSpeed = 400f;

    [Header("Partículas")]
    public GameObject prefabParticulaMorte;
    public GameObject prefabParticulaRespawn;
    public GameObject prefabParticulaColetaOuro;
    public GameObject prefabParticulaAcerto;
    public GameObject prefabParticulaErro;
    public Vector3 offsetParticulaRespawn = new Vector3(0, 1f, 0);

    [Header("Partícula - Batida no limite (clássico)")]
    public GameObject particulaBatidaLimite;
    public Vector3 offsetparticulaBatidaLimite = Vector3.zero;

    [Header("Partícula - Vitória (usada no Tower como 'escada liberada')")]
    public GameObject prefabParticulaVitoria;
    public Vector3 offsetParticulaVitoria = new Vector3(0, 1f, 0);

    [Header("Respawn")]
    public float alturaExtraRespawn = 0.2f;
    public float offsetXRespawn = 0.2f;
    public float offsetZRespawn = -0.35f;

    [Header("Flecha")]
    public GameObject prefabFlecha;
    public Transform pontoDeDisparo;
    public float delayDisparo = 0.25f;

    [Header("Animação")]
    public string idleStateName = "Idle";

    private bool isMoving = false;
    private bool isDying = false;
    private bool gameOver = false;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private TowerGridGenerator gridGen;
    private Animator anim;

    private int flechas = 2;
    private int ouroColetado = 0;
    private int wumpusMortos = 0;
    private int vidas = 5;

    private Vector2Int ultimaSalaAtiva = new Vector2Int(-999, -999);

    private void OnEnable()
    {
        TowerGridGenerator.OnEscadaLiberada += OnEscadaLiberadaHandler;
    }

    private void OnDisable()
    {
        TowerGridGenerator.OnEscadaLiberada -= OnEscadaLiberadaHandler;
    }

    private void Start()
    {
        gridGen = TowerGridGenerator.instancia;
        anim = GetComponentInChildren<Animator>();

        targetPosition = transform.position;
        targetRotation = transform.rotation;

        AtualizarSalaAtual();
        AtualizarMapaVisualDaSalaAtual();
        AtualizarUI(); // inicializa contadores na UI
    }

    private void Update()
    {
        if (isDying || gameOver) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public void RotateLeft()
    {
        if (isMoving || isDying || gameOver) return;
        targetRotation *= Quaternion.Euler(0, -90, 0);
        anim?.SetTrigger("girarEs");
    }

    public void RotateRight()
    {
        if (isMoving || isDying || gameOver) return;
        targetRotation *= Quaternion.Euler(0, 90, 0);
        anim?.SetTrigger("girarDir");
    }

    public void MoveForward()
    {
        if (isMoving || isDying || gameOver) return;

        Vector3 dir = new Vector3(
            Mathf.Round(transform.forward.x),
            0,
            Mathf.Round(transform.forward.z)
        );

        Vector3 destination = targetPosition + dir * moveDistance;

        if (!SalaExisteNaDirecao(dir))
        {
            if (particulaBatidaLimite != null)
                Instantiate(particulaBatidaLimite, transform.position + offsetparticulaBatidaLimite, Quaternion.identity);

            Debug.Log("Tentativa de sair do mapa bloqueada!");
            return;
        }

        // custo por passo
        TowerUIManager.instancia?.AlterarPontuacao(-1);

        StartCoroutine(MoveToPosition(destination));
        anim?.SetTrigger("foward");
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

        return gridGen.gridInfo.ContainsKey(destino);
    }

    IEnumerator MoveToPosition(Vector3 destination)
    {
        isMoving = true;
        targetPosition = destination;

        while (Vector3.Distance(transform.position, destination) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = destination;
        isMoving = false;

        AtualizarSalaAtual();
        AtualizarMapaVisualDaSalaAtual();

        if (EstaEmSalaComPoco())
        {
            StartCoroutine(MorrerNoPoco());
            yield break;
        }
        if (EstaEmSalaComWumpus())
        {
            StartCoroutine(MorrerParaOWumpus());
            yield break;
        }

        if (EstaEmSalaComEscada())
        {
            TowerUIManager.instancia?.MostrarBotaoAvancar(true);
        }

    }

    bool EstaEmSalaComPoco()
    {
        Vector2Int pos = PegarPosicaoGrid();
        return gridGen.gridInfo.ContainsKey(pos) && gridGen.gridInfo[pos].temPoco;
    }

    bool EstaEmSalaComWumpus()
    {
        Vector2Int pos = PegarPosicaoGrid();
        return gridGen.gridInfo.ContainsKey(pos) && gridGen.gridInfo[pos].temWumpus;
    }

    bool EstaEmSalaComEscada()
    {
        Vector2Int pos = PegarPosicaoGrid();
        if (gridGen.gridInfo.ContainsKey(pos) && gridGen.gridInfo[pos].temEscada) return true;

        if (gridGen.mapaGerado.TryGetValue(pos, out GameObject sala))
        {
            foreach (Transform child in sala.transform)
            {
                if (child != null && child.CompareTag("escada")) return true;
            }
        }
        return false;
    }

    IEnumerator MorrerNoPoco()
    {
        Debug.Log("O jogador caiu no poço!");
        isDying = true;
        vidas--;

        if (prefabParticulaMorte != null)
            Instantiate(prefabParticulaMorte, transform.position + Vector3.up * 1f, Quaternion.identity);

        // penalidade por morte
        TowerUIManager.instancia?.AlterarPontuacao(-1000);

        AtualizarUI();

        var cam = FindFirstObjectByType<CameraFollow>();
        cam?.FocarNoPonto(CentroDaSalaAtual());

        anim?.SetTrigger("queda");
        var queda = GetComponent<FallOnDeath>();
        if (queda != null)
            yield return StartCoroutine(queda.ExecutarQueda());

        if (vidas <= 0) { AbrirTelaDerrota(); yield break; }

        yield return new WaitForSeconds(1f);
        RespawnarPlayer();
    }

    IEnumerator MorrerParaOWumpus()
    {
        Debug.Log("O jogador foi morto pelo Wumpus!");
        isDying = true;
        vidas--;

        DispararAnimacaoWumpusAtaqueNaSalaAtual();
        anim?.SetTrigger("queda");

        if (prefabParticulaMorte != null)
            Instantiate(prefabParticulaMorte, transform.position + Vector3.up * 1f, Quaternion.identity);

        // penalidade por morte
        TowerUIManager.instancia?.AlterarPontuacao(-1000);

        AtualizarUI();

        if (vidas <= 0) { AbrirTelaDerrota(); yield break; }

        yield return new WaitForSeconds(1f);
        RespawnarPlayer();
    }

    void DispararAnimacaoWumpusAtaqueNaSalaAtual()
    {
        Vector2Int pos = PegarPosicaoGrid();

        if (gridGen.mapaGerado.TryGetValue(pos, out GameObject sala))
        {
            Animator wAnim = null;
            foreach (Transform child in sala.transform)
            {
                if (child != null && child.CompareTag("wumpus"))
                {
                    wAnim = child.GetComponentInChildren<Animator>();
                    break;
                }
            }

            if (wAnim != null) wAnim.SetTrigger("wattack");
        }
    }

    void RespawnarPlayer()
    {
        Vector3 posRespawn = Vector3.zero + gridGen.offsetCentroSala + new Vector3(offsetXRespawn, alturaExtraRespawn, offsetZRespawn);
        transform.SetPositionAndRotation(posRespawn, Quaternion.identity);

        var cam = FindFirstObjectByType<CameraFollow>();
        Transform camTarget = transform.Find("CameraTarget") ?? transform;
        cam?.RetomarFollow(camTarget);
        GetComponent<FallOnDeath>()?.ResetarYParaZero();

        targetPosition = transform.position;
        targetRotation = transform.rotation;

        if (prefabParticulaRespawn != null)
            Instantiate(prefabParticulaRespawn, transform.position + offsetParticulaRespawn, Quaternion.identity);

        ForcarIdle();

        AtualizarSalaAtual();
        AtualizarMapaVisualDaSalaAtual();
        AtualizarUI();

        isDying = false;
    }

    void ForcarIdle()
    {
        if (anim == null) return;

        anim.ResetTrigger("queda");
        anim.ResetTrigger("foward");
        anim.ResetTrigger("girarEs");
        anim.ResetTrigger("girarDir");
        anim.ResetTrigger("Atirar");
        anim.ResetTrigger("Pick");

        int hash = Animator.StringToHash(idleStateName);
        if (anim.HasState(0, hash)) anim.CrossFade(hash, 0.05f, 0, 0f);
        else { anim.Rebind(); anim.Update(0f); }
    }

    Vector3 CentroDaSalaAtual()
    {
        Vector2Int pos = PegarPosicaoGrid();
        if (gridGen.mapaGerado.TryGetValue(pos, out GameObject sala))
            return sala.transform.position + gridGen.offsetCentroSala;

        // fallback caso não encontre no dicionário (não deve acontecer)
        return new Vector3(pos.x * gridGen.espacoEntreSalas, 0f, pos.y * gridGen.espacoEntreSalas) + gridGen.offsetCentroSala;
    }


    void AbrirTelaDerrota()
    {
        isDying = false;
        gameOver = true;
        ForcarIdle();

        TowerGameOverPanel.instancia?.Show();
    }

    void AtualizarSalaAtual()
    {
        Vector2Int pos = PegarPosicaoGrid();

        if (gridGen.mapaGerado.TryGetValue(ultimaSalaAtiva, out GameObject salaAnterior) && salaAnterior.activeSelf)
            salaAnterior.SetActive(false);

        AtivarSala(pos);
        ultimaSalaAtiva = pos;
    }

    void AtivarSala(Vector2Int pos)
    {
        if (gridGen.mapaGerado.TryGetValue(pos, out GameObject sala) && !sala.activeSelf)
            sala.SetActive(true);
    }

    Vector2Int PegarPosicaoGrid()
    {
        return new Vector2Int(
            Mathf.RoundToInt(transform.position.x / moveDistance),
            Mathf.RoundToInt(transform.position.z / moveDistance)
        );
    }

    void AtualizarUI()
    {
        TowerUIManager.instancia?.AtualizarFlechas(flechas);
        TowerUIManager.instancia?.AtualizarOuro(ouroColetado);
        TowerUIManager.instancia?.AtualizarDWumpus(wumpusMortos);
        TowerUIManager.instancia?.AtualizarVidas(vidas);
        // Pontuação é atualizada pelo TowerUIManager internamente via AlterarPontuacao
    }

    public void ColetarOuro()
    {
        if (gameOver) return;

        Vector2Int pos = PegarPosicaoGrid();
        if (!gridGen.gridInfo.ContainsKey(pos)) return;

        if (gridGen.gridInfo[pos].temOuro)
        {
            ouroColetado++;
            gridGen.RemoverOuroNaPosicao(pos);

            anim?.SetTrigger("Pick");

            if (prefabParticulaColetaOuro != null)
                Instantiate(prefabParticulaColetaOuro, transform.position + Vector3.up * 1f, Quaternion.identity);

            // recompensa por ouro
            TowerUIManager.instancia?.AlterarPontuacao(+1000);

            // bônus de flecha como você já fazia
            flechas++;
            TowerUIManager.instancia?.AtualizarFlechas(flechas);

            gridGen.TentarInstanciarEscadaSeElegivel();

            Debug.Log("Ouro coletado! Ouro visual removido da sala.");

            AtualizarUI();
        }
        else
        {
            Debug.Log("Não há ouro nesta sala.");
        }
    }

    public void AtirarFlecha()
    {
        if (flechas <= 0 || isDying || gameOver) return;

        flechas--;
        TowerUIManager.instancia?.AtualizarFlechas(flechas);

        anim?.SetTrigger("Atirar");

        StartCoroutine(DispararFlechaComDelay(delayDisparo));
    }

    IEnumerator DispararFlechaComDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (prefabFlecha != null && pontoDeDisparo != null)
            Instantiate(prefabFlecha, pontoDeDisparo.position, Quaternion.LookRotation(transform.forward));

        Vector2Int posAtual = PegarPosicaoGrid();
        Vector2Int direcao = new Vector2Int(
            Mathf.RoundToInt(transform.forward.x),
            Mathf.RoundToInt(transform.forward.z)
        );

        Vector2Int posAlvo = posAtual + direcao;

        if (posAlvo == gridGen.posicaoWumpus && gridGen.gridInfo.ContainsKey(posAlvo))
        {
            gridGen.EliminarWumpusNaPosicao(posAlvo);
            wumpusMortos++;

            if (prefabParticulaAcerto != null)
                Instantiate(prefabParticulaAcerto, transform.position + Vector3.up * 1f, Quaternion.identity);

            // recompensa por matar Wumpus
            TowerUIManager.instancia?.AlterarPontuacao(+1000);

            gridGen.TentarInstanciarEscadaSeElegivel();

            Debug.Log("Você acertou o Wumpus!");

            AtualizarUI();
        }
        else
        {
            if (prefabParticulaErro != null)
                Instantiate(prefabParticulaErro, transform.position + Vector3.up * 1f, Quaternion.identity);

            Debug.Log("Você errou o tiro.");
        }
    }

    private void AtualizarMapaVisualDaSalaAtual()
{
    if (gridGen == null) return;

    Vector2Int pos = PegarPosicaoGrid();
    var sensacoes = gridGen.ObterSensacoes(pos);
    MapaVisualTower.instancia?.AtualizarTile(pos, sensacoes);
}

    public void OnChegouNovoAndar()
    {
        isDying = false;
        gameOver = false;
        ForcarIdle();
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        AtualizarSalaAtual();
        AtualizarMapaVisualDaSalaAtual();
        AtualizarUI();
    }

    private void OnEscadaLiberadaHandler()
    {
        if (prefabParticulaVitoria != null)
            Instantiate(prefabParticulaVitoria, transform.position + offsetParticulaVitoria, Quaternion.identity);
    }
}