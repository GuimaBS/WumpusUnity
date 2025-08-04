using System.Collections;
using System.Collections.Generic;
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
    public int dwumpus = 0;

    [Header("Colisão")]
    public LayerMask obstaculosLayer;

    [Header("Partículas de Acerto/Erro do Wumpus")]
    public GameObject particulaAcertoWumpus;
    public GameObject particulaErroWumpus;

    [Header("Flecha")]
    public GameObject prefabFlecha;
    public Transform pontoDeDisparo;
    public float delayDisparo = 0.25f;

    [Header("Partículas")]
    public GameObject prefabParticulaColetar;
    public GameObject prefabParticulaMorte;
    public GameObject prefabParticulaMorteWumpus;
    public GameObject prefabParticulaRespawn;
    public GameObject prefabParticulaVitoria;
    public GameObject particulaBatidaLimite;

    [Header("Offset da Partícula da barreira")]
    public Vector3 offsetparticulaBatidaLimite = Vector3.zero;


    [Header("Respawn")]
    public float alturaExtraRespawn = 0.2f;
    public float offsetXRespawn = 0f;
    public float offsetZRespawn = 0f;
    public Vector3 offsetParticulaRespawn = new Vector3(0, 1f, 0);

    private Animator animator;
    private Collider playerCollider;
    private Renderer[] renderers;
    private Transform salaFocus;

    private bool isMoving = false;
    private bool isDying = false;
    private bool vitoriaAlcancada = false;
    private Vector2Int posicaoAtual;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private PlayerGridGenerator gridGen;

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
        GameObject salaFocusObj = new GameObject("SalaFocus");
        salaFocus = salaFocusObj.transform;
        gridGen = PlayerGridGenerator.instancia;

        CameraFollow cam = FindFirstObjectByType<CameraFollow>();
        if (cam != null)
        {
            cam.DefinirAlvo(salaFocus);
            cam.offset = new Vector3(0, 4, -7);
        }

        AtualizarSalaAtual();

        animator = GetComponentInChildren<Animator>();
        playerCollider = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>();

        if (animator == null)
            Debug.LogWarning("Animator não encontrado!");

        targetPosition = transform.position;
        targetRotation = transform.rotation;

        AtualizarUI();
        RegistrarWumpusEPoco();
        AtualizarMapaVisual();
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
        animator?.SetTrigger("girarEs");
        targetRotation *= Quaternion.Euler(0, -90, 0);
    }

    public void RotateRight()
    {
        if (isMoving || isDying) return;
        animator?.SetTrigger("girarDir");
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
            if (particulaBatidaLimite != null)
                Instantiate(particulaBatidaLimite, transform.position + offsetparticulaBatidaLimite, Quaternion.identity);
            return;
        }

        StartCoroutine(MoveToPosition(destination, dir));

        animator?.SetTrigger("foward");

        TimerPontuacaoController.passosDados++; //
        TimerPontuacaoController.pontuacaoFinal -= 1; // Desconto de pontuação por passo
        UIManager.instancia?.AlterarPontuacao(-1);
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
        ChecarCondicaoDeVitoria();

        if (EstaEmSalaComPoco())
        {
            StartCoroutine(MorrerNoPoco());
        }
        else if (EstaEmSalaComWumpus())
        {
            StartCoroutine(MorrerParaOWumpus());
        }

        RegistrarWumpusEPoco();
        AtualizarMapaVisual();
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
        TimerPontuacaoController.mortes = mortes;
        AtualizarUI();

        animator?.SetTrigger("queda");

        if (prefabParticulaMorte != null)
            Instantiate(prefabParticulaMorte, transform.position + Vector3.up * 1f, Quaternion.identity);
        UIManager.instancia?.AlterarPontuacao(-1000);
        TimerPontuacaoController.pontuacaoFinal -= 1000;
        yield return new WaitForSeconds(1f);

        RespawnarPlayer();
        ChecarCondicaoDeVitoria();
    }

    IEnumerator MorrerParaOWumpus()
    {
        Debug.Log("O jogador foi morto pelo Wumpus!");

        // Identifica a posição atual do player
        Vector2Int pos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / moveDistance),
            Mathf.RoundToInt(transform.position.z / moveDistance)
        );

        // Ativa a animação
        if (PlayerGridGenerator.instancia.mapaGerado.TryGetValue(pos, out GameObject sala))
        {
            foreach (Transform child in sala.transform)
            {
                if (child.CompareTag("wumpus"))
                {
                    Animator animWumpus = child.GetComponent<Animator>();
                    if (animWumpus != null)
                    {
                        animWumpus.SetTrigger("wattack");
                        animator?.SetTrigger("queda");
                    }
                    break;
                }
            }
        }

        yield return new WaitForSeconds(0.1f);

        isDying = true;
        mortes++;
        TimerPontuacaoController.mortes = mortes;
        AtualizarUI();

        animator?.SetTrigger("dwumpus");

        if (prefabParticulaMorteWumpus != null)
            Instantiate(prefabParticulaMorteWumpus, transform.position + Vector3.up * 1f, Quaternion.identity);

        UIManager.instancia?.AlterarPontuacao(-1000);
        TimerPontuacaoController.pontuacaoFinal -= 1000;

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

        Vector3 posRespawn = RespawnPoint.instancia.transform.position + new Vector3(0.2f, alturaExtraRespawn, -0.35f);

        transform.SetPositionAndRotation(posRespawn, Quaternion.identity);
        targetPosition = transform.position;
        targetRotation = transform.rotation;

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.Rebind();
            animator.Update(0f);
        }

        Transform visual = transform.Find("Visual");
        if (visual != null)
        {
            visual.localPosition = Vector3.zero;

            Transform char1ajustado = visual.Find("Char1_ajustado");
            if (char1ajustado != null)
            {
                char1ajustado.localPosition = Vector3.zero;

                Transform char1 = char1ajustado.Find("Char1");
                if (char1 != null)
                    char1.localPosition = Vector3.zero;
            }
        }

        foreach (var rend in renderers)
            rend.enabled = true;

        if (playerCollider != null)
            playerCollider.enabled = true;

        if (prefabParticulaRespawn != null)
            Instantiate(prefabParticulaRespawn, transform.position + offsetParticulaRespawn, Quaternion.identity);

        AtualizarSalaAtual();
        Debug.Log("Jogador respawnado corretamente na sala (0,0), câmera reposicionada.");
        ChecarCondicaoDeVitoria();

        isDying = false;
    }

    void AtualizarSalaAtual()
    {
        Vector2Int pos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / PlayerGridGenerator.instancia.espacoEntreSalas),
            Mathf.RoundToInt(transform.position.z / PlayerGridGenerator.instancia.espacoEntreSalas)
        );

        if (salaFocus != null)
        {
            Vector3 centroSala = new Vector3(pos.x, 0, pos.y) * PlayerGridGenerator.instancia.espacoEntreSalas;
            centroSala += PlayerGridGenerator.instancia.offsetCentroSala;
            salaFocus.position = centroSala;
        }

        SalaManager.instancia?.AtualizarSalasAtivas(pos);
    }

    public void AtirarFlecha()
    {
        if (flechas <= 0 || isDying) return;

        flechas--;
        AtualizarUI();
        animator?.SetTrigger("Atirar");
        StartCoroutine(DispararFlechaComDelay(0.3f));

    }

    IEnumerator DispararFlechaComDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (prefabFlecha != null && pontoDeDisparo != null)
            Instantiate(prefabFlecha, pontoDeDisparo.position, Quaternion.LookRotation(transform.forward));

        Vector3 direcao = new Vector3(
            Mathf.RoundToInt(transform.forward.x),
            0,
            Mathf.RoundToInt(transform.forward.z)
        );

        Vector2Int posAtual = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / moveDistance),
            Mathf.RoundToInt(transform.position.z / moveDistance)
        );

        Vector2Int posAlvo = posAtual + new Vector2Int(
            Mathf.RoundToInt(direcao.x),
            Mathf.RoundToInt(direcao.z)
        );

        Debug.Log($"[Debug Tiro] Posição do alvo: {posAlvo}, Wumpus está em: {PlayerGridGenerator.instancia.posicaoWumpus}");

        if (posAlvo == PlayerGridGenerator.instancia.posicaoWumpus &&
    PlayerGridGenerator.instancia.gridInfo.ContainsKey(posAlvo))
        {
            PlayerGridGenerator.instancia.EliminarWumpusNaPosicao(posAlvo);

            if (particulaAcertoWumpus != null)
                Instantiate(particulaAcertoWumpus, transform.position + Vector3.up * 2f, Quaternion.identity);
            dwumpus++;
            UIManager.instancia?.AlterarPontuacao(1000);
            ChecarCondicaoDeVitoria();

            AtualizarUI();
            UIManager.instancia?.AtualizarDWumpus(dwumpus);
            PlayerGridGenerator.instancia?.RegistrarWumpusMorto();

            TimerPontuacaoController.pontuacaoFinal += 1000;


            Debug.Log("Você acertou o Wumpus!");

            //invalida a posição do Wumpus para impedir múltiplos acertos
            PlayerGridGenerator.instancia.posicaoWumpus = new Vector2Int(-1, -1);
        }
        else
        {
            if (particulaErroWumpus != null)
                Instantiate(particulaErroWumpus, transform.position + Vector3.up * 2f, Quaternion.identity);

            Debug.Log("Você errou o tiro.");
            UIManager.instancia?.AlterarPontuacao(-500);
            LogManager.instancia?.AdicionarLog("Você errou o tiro...");
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
                UIManager.instancia?.AlterarPontuacao(1000);

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
                PlayerGridGenerator.instancia?.RegistrarOuroColetado();

                TimerPontuacaoController.pontuacaoFinal += 1000;
                ChecarCondicaoDeVitoria();
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
            UIManager.instancia.AtualizarDWumpus(dwumpus);
        }
    }

    private void ChecarCondicaoDeVitoria()
    {
        if (vitoriaAlcancada) return;

        bool ouroColetado = ouro > 0;
        bool wumpusMorto = dwumpus > 0;

        Vector2Int pos = PlayerGridGenerator.instancia.ConverterPosicaoMundoParaGrid(transform.position);
        bool estaNaSalaInicial = pos == new Vector2Int(0, 0);

        Debug.Log($"[CHECAGEM VITÓRIA] ouro: {ouroColetado}, wumpus: {wumpusMorto}, pos: {pos}");

        if (ouroColetado && wumpusMorto && estaNaSalaInicial)
        {
            AplicarVitoria();
        }
        else if (ouroColetado && wumpusMorto && !estaNaSalaInicial)
        {
            Debug.Log("[VITÓRIA] Ouro e Wumpus obtidos. Retorne à sala inicial para vencer.");
        }
    }

    private void AplicarVitoria()
    {
        if (vitoriaAlcancada) return;

        vitoriaAlcancada = true;

        TimerPontuacaoController.TempoTotal = Time.time - TimerPontuacaoController.tempoInicio;
        TimerPontuacaoController.pontuacaoFinal += 2000;

        Debug.Log("[VITÓRIA] Condição alcançada! Ativando painel de vitória.");

        if (prefabParticulaVitoria != null)
            Instantiate(prefabParticulaVitoria, transform.position + Vector3.up * 1f, Quaternion.identity);

        animator?.SetTrigger("win");
        UIManager.instancia?.MostrarPainelVitoria();
    }

    void AtualizarMapaVisual()
    {
        if (gridGen == null || MapaVisualPlayer.instancia == null)
        {
            Debug.LogError("gridGen ou MapaVisualPlayer está nulo!");
            return;
        }

        Vector2Int pos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / moveDistance),
            Mathf.RoundToInt(transform.position.z / moveDistance)
        );

        // Marca como visitada
        if (gridGen.gridInfo.TryGetValue(pos, out PlayerGridGenerator.TileInfo info))
        {
            info.foiVisitada = true;

            if (info.temWumpus)
                MapaVisualPlayer.instancia.AtualizarTile(pos, new List<string> { "wumpus" });
            else if (info.temPoco)
                MapaVisualPlayer.instancia.AtualizarTile(pos, new List<string> { "poco" });
            else if (info.temOuro)
                MapaVisualPlayer.instancia.AtualizarTile(pos, new List<string> { "brilho" });
            else if (info.temFedor)
                MapaVisualPlayer.instancia.AtualizarTile(pos, new List<string> { "fedor" });
            else if (info.temBrisa)
                MapaVisualPlayer.instancia.AtualizarTile(pos, new List<string> { "brisa" });
            else
                MapaVisualPlayer.instancia.AtualizarTile(pos, new List<string> { "vazio" });
        }
        else
        {
            MapaVisualPlayer.instancia.AtualizarTile(pos, new List<string> { "desconhecido" });
        }
    }

    void RegistrarWumpusEPoco()
    {
        Vector2Int pos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x / moveDistance),
            Mathf.RoundToInt(transform.position.z / moveDistance)
        );

        if (!gridGen.gridInfo.TryGetValue(pos, out PlayerGridGenerator.TileInfo info))
            return;

        // Cria ou recupera a lista de sensações já registradas no dicionário
        if (!gridGen.sensacoesPorPosicao.ContainsKey(pos))
            gridGen.sensacoesPorPosicao[pos] = new List<string>();

        var sensacoes = gridGen.sensacoesPorPosicao[pos];

        if (info.temWumpus && !sensacoes.Contains("wumpus"))
            sensacoes.Add("wumpus");

        if (info.temPoco && !sensacoes.Contains("poco"))
            sensacoes.Add("poco");

        if (sensacoes.Count == 0 && !sensacoes.Contains("vazio"))
            sensacoes.Add("vazio");

        // Atualiza a visualização da tile com as sensações registradas
        MapaVisualPlayer.instancia.AtualizarTile(pos, new List<string>(sensacoes));
    }

}