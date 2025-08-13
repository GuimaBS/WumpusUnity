using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class TowerGridGenerator : MonoBehaviour
{
    public static TowerGridGenerator instancia;

    // Eventos
    public static System.Action OnNovoAndar;
    public static System.Action OnEscadaLiberada; // <- NOVO: avisa quando a escada foi criada

    [Header("Prefabs dos Personagens")]
    public GameObject prefabArqueiro;
    public GameObject prefabAmazona;

    [Header("Offset Espec�fico por Personagem")]
    public Vector3 offsetArqueiro = Vector3.zero;
    public Vector3 offsetAmazona = Vector3.zero;

    [Header("Prefab da Escada")]
    public GameObject prefabEscada;
    public Vector3 offsetEscada = Vector3.zero;

    [Header("Part�cula - Escada liberada (opcional, n�o usada)")]
    public GameObject prefabParticulaEscada;
    public Vector3 offsetParticulaEscada = new Vector3(0f, 1f, 0f);

    [Header("Prefabs das Salas")]
    public GameObject salaPrefab;
    public GameObject salaComPocoPrefab;

    [Header("Prefabs do bloqueio")]
    public GameObject prefabBloqueioSala;

    [Header("Offset dos Bloqueios")]
    public Vector3 offsetBloqueioX = new Vector3(5f, 0f, 0f);
    public Vector3 offsetBloqueioZ = new Vector3(0f, 0f, 5f);

    [Header("Prefabs do Wumpus e Ouro")]
    public GameObject prefabWumpus;
    public GameObject prefabOuro;

    [Header("Prefabs de Sensa��es")]
    public GameObject prefabBrisa;
    public GameObject prefabFedor;
    public GameObject prefabBrilho;

    [Header("Espa�amento e Organiza��o")]
    public float espacoEntreSalas = 10f;
    public Transform paiDasSalas;
    public Transform paiDoPlayer;

    [Header("Offsets e Rota��o")]
    public Vector3 offsetCentroSala = new Vector3(5, 0, 5);
    public float rotacaoYWumpus = 0f;
    public float rotacaoYOuro = 0f;

    [Header("Normaliza��o de Escala (Opcional)")]
    public bool normalizarEscala = true;
    public Vector3 tamanhoMundialWumpus = Vector3.one;
    public Vector3 tamanhoMundialOuro = Vector3.one;
    public Vector3 offsetLocalWumpus = Vector3.zero;
    public float rotacaoExtraYWumpus = 0f;
    public Vector3 offsetLocalOuro = Vector3.zero;
    public float rotacaoExtraYOuro = 0f;
    public bool logarEscalasNoSpawn = false;

    public Vector2Int posicaoWumpus;
    public Vector2Int posicaoOuro;
    public Vector2Int posicaoEscada;

    public bool wumpusMorto = false;
    public bool ouroColetado = false;
    public bool escadaInstanciada = false;

    private Transform playerTr;                
    private bool ultimoEstadoBotaoAvancar = false;

    public int andarAtual = 1;

    [Header("Mapa Gerado")]
    public Dictionary<Vector2Int, GameObject> mapaGerado = new Dictionary<Vector2Int, GameObject>();

    [Header("Mapa L�gico")]
    public Dictionary<Vector2Int, TileInfo> gridInfo = new Dictionary<Vector2Int, TileInfo>();

    [Header("Sensa��es por Posi��o")]
    public Dictionary<Vector2Int, List<string>> sensacoesPorPosicao = new Dictionary<Vector2Int, List<string>>();

    [System.Serializable]
    public class TileInfo
    {
        public bool temPoco = false;
        public bool temBrisa = false;
        public bool temFedor = false;
        public bool temOuro = false;
        public bool temWumpus = false;
        public bool temEscada = false;
        public bool foiVisitada = false;
    }

    private int tamanhoX, tamanhoY;

    private void Awake()
    {
        if (instancia == null) instancia = this;
        else { Destroy(gameObject); return; }

        GerarNovoAndar();
    }

    private void Update()
    {
        // Se não há escada ou player, garante botão oculto e sai
        if (!escadaInstanciada || playerTr == null)
        {
            ToggleBotaoAvancar(false);
            return;
        }

        // Estamos na sala da escada?
        bool naEscada = WorldToGrid(playerTr.position) == posicaoEscada;

        // Mostra/oculta o botão só quando muda de estado (evita custos desnecessários)
        ToggleBotaoAvancar(naEscada);
    }

    private void ToggleBotaoAvancar(bool mostrar)
    {
        if (mostrar == ultimoEstadoBotaoAvancar) return;
        ultimoEstadoBotaoAvancar = mostrar;

        // OPCIONAL: só funciona se você tiver esse método no seu UIManager
        TowerUIManager.instancia?.MostrarBotaoAvancar(mostrar);
    }



    public void GerarNovoAndar()
    {
        tamanhoX = Random.Range(4, 11);
        tamanhoY = Random.Range(4, 11);
        Debug.Log("[TowerGrid] Gerando andar " + andarAtual + ": " + tamanhoX + "x" + tamanhoY);

        LimparMapa();
        GerarMapa();
        GarantirSalaSeguraEm00();
        AplicarBrisaNosPocos();
        InstanciarWumpus();
        InstanciarOuro();
        SpawnarOuReposicionarPlayer();

        ouroColetado = false;
        wumpusMorto = false;
        escadaInstanciada = false;

        OnNovoAndar?.Invoke();

        ultimoEstadoBotaoAvancar = false;
        TowerUIManager.instancia?.MostrarBotaoAvancar(false);
    }

    private void GerarMapa()
    {
        for (int x = 0; x < tamanhoX; x++)
        {
            for (int y = 0; y < tamanhoY; y++)
            {
                Vector3 pos = new Vector3(x * espacoEntreSalas, 0, y * espacoEntreSalas);
                Vector2Int gridPos = new Vector2Int(x, y);

                bool temPoco = Random.value < 0.2f;
                GameObject sala = Instantiate(
                    temPoco ? salaComPocoPrefab : salaPrefab,
                    pos,
                    Quaternion.identity,
                    paiDasSalas
                );

                sala.name = "Sala (" + x + "," + y + ")";
                if (x != 0 || y != 0)
                    sala.SetActive(false);

                mapaGerado[gridPos] = sala;
                AdicionarBloqueios(gridPos, sala);

                TileInfo info = new TileInfo { temPoco = temPoco };
                gridInfo[gridPos] = info;
            }
        }
    }

    private void GarantirSalaSeguraEm00()
    {
        Vector2Int posInicial = Vector2Int.zero;

        if (gridInfo.ContainsKey(posInicial))
        {
            if (gridInfo[posInicial].temPoco)
            {
                if (mapaGerado.TryGetValue(posInicial, out GameObject antigaSala))
                {
                    Destroy(antigaSala);
                    mapaGerado.Remove(posInicial);
                }

                gridInfo[posInicial].temPoco = false;

                GameObject novaSala = Instantiate(salaPrefab, Vector3.zero, Quaternion.identity, paiDasSalas);
                novaSala.name = "Sala (0,0)";
                novaSala.SetActive(true);
                mapaGerado[posInicial] = novaSala;
            }

            if (mapaGerado.TryGetValue(posInicial, out GameObject sala00) && !sala00.activeSelf)
            {
                sala00.SetActive(true);
            }

            GameObject marcador = new GameObject("RespawnMarker");
            marcador.transform.SetParent(mapaGerado[posInicial].transform);
            marcador.transform.localPosition = Vector3.zero;
            marcador.AddComponent<RespawnPoint>();
        }
        else
        {
            Debug.LogError("[TowerGrid] Sala (0,0) n�o existe no gridInfo.");
        }
    }

    void AdicionarBloqueios(Vector2Int pos, GameObject sala)
    {
        if (prefabBloqueioSala == null) return;

        if (pos.x == 0)
        {
            Vector3 posBloqueio = sala.transform.position - offsetBloqueioX;
            Quaternion rot = Quaternion.Euler(0, -90, 0);
            Instantiate(prefabBloqueioSala, posBloqueio, rot, sala.transform);
        }

        if (pos.x == tamanhoX - 1)
        {
            Vector3 posBloqueio = sala.transform.position + offsetBloqueioX;
            Quaternion rot = Quaternion.Euler(0, 90, 0);
            Instantiate(prefabBloqueioSala, posBloqueio, rot, sala.transform);
        }

        if (pos.y == 0)
        {
            Vector3 posBloqueio = sala.transform.position - offsetBloqueioZ;
            Quaternion rot = Quaternion.Euler(0, 180, 0);
            Instantiate(prefabBloqueioSala, posBloqueio, rot, sala.transform);
        }

        if (pos.y == tamanhoY - 1)
        {
            Vector3 posBloqueio = sala.transform.position + offsetBloqueioZ;
            Quaternion rot = Quaternion.Euler(0, 0, 0);
            Instantiate(prefabBloqueioSala, posBloqueio, rot, sala.transform);
        }
    }

    private void AplicarBrisaNosPocos()
    {
        foreach (var kvp in gridInfo)
        {
            if (!kvp.Value.temPoco) continue;

            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var dir in dirs)
            {
                Vector2Int vizinha = kvp.Key + dir;
                if (gridInfo.ContainsKey(vizinha))
                {
                    gridInfo[vizinha].temBrisa = true;
                    Vector3 pos = mapaGerado[vizinha].transform.position + Vector3.up * 1.5f;
                    Instantiate(prefabBrisa, pos, Quaternion.identity, mapaGerado[vizinha].transform);
                }
            }
        }
    }

    private void InstanciarWumpus()
    {
        do
        {
            posicaoWumpus = new Vector2Int(Random.Range(0, tamanhoX), Random.Range(0, tamanhoY));
        } while (posicaoWumpus == Vector2Int.zero || gridInfo[posicaoWumpus].temPoco);

        Vector3 pos = mapaGerado[posicaoWumpus].transform.position + Vector3.up * 0.5f;
        Quaternion rot = Quaternion.Euler(0f, rotacaoYWumpus, 0f);

        GameObject wumpusGO = Instantiate(prefabWumpus, pos, rot, mapaGerado[posicaoWumpus].transform);
        wumpusGO.name = "Wumpus";
        wumpusGO.tag = "wumpus";

        NormalizarWorldScale(wumpusGO.transform, tamanhoMundialWumpus);
        wumpusGO.transform.localPosition += offsetLocalWumpus;
        wumpusGO.transform.localRotation = Quaternion.Euler(0f, rotacaoExtraYWumpus, 0f) * wumpusGO.transform.localRotation;

        if (logarEscalasNoSpawn)
            Debug.Log("[TowerGrid] Wumpus lossy=" + wumpusGO.transform.lossyScale);

        gridInfo[posicaoWumpus].temWumpus = true;

        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var dir in dirs)
        {
            Vector2Int adj = posicaoWumpus + dir;
            if (gridInfo.ContainsKey(adj))
            {
                gridInfo[adj].temFedor = true;
                Vector3 posF = mapaGerado[adj].transform.position + Vector3.up * 1.5f;
                Instantiate(prefabFedor, posF, Quaternion.identity, mapaGerado[adj].transform);
            }
        }
    }

    public void EliminarWumpusNaPosicao(Vector2Int pos)
    {
        if (gridInfo.ContainsKey(pos) && gridInfo[pos].temWumpus)
        {
            gridInfo[pos].temWumpus = false;
            wumpusMorto = true;

            if (mapaGerado.TryGetValue(pos, out GameObject sala))
            {
                foreach (Transform child in sala.transform)
                {
                    if (child.CompareTag("wumpus"))
                    {
                        Destroy(child.gameObject);
                        break;
                    }
                }
            }

            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in dirs)
            {
                Vector2Int adj = pos + dir;
                if (gridInfo.ContainsKey(adj))
                {
                    gridInfo[adj].temFedor = false;

                    if (mapaGerado.TryGetValue(adj, out GameObject salaAdj))
                    {
                        List<Transform> remover = new List<Transform>();
                        foreach (Transform child in salaAdj.transform)
                        {
                            if (child.name.Contains("Fedor"))
                                remover.Add(child);
                        }
                        foreach (var t in remover) Destroy(t.gameObject);
                    }
                }
            }

            TentarInstanciarEscadaSeElegivel();
        }
    }

    private void InstanciarOuro()
    {
        do
        {
            posicaoOuro = new Vector2Int(Random.Range(0, tamanhoX), Random.Range(0, tamanhoY));
        }
        while (posicaoOuro == Vector2Int.zero || posicaoOuro == posicaoWumpus || gridInfo[posicaoOuro].temPoco);

        Vector3 pos = mapaGerado[posicaoOuro].transform.position + Vector3.up * 0.5f;
        Quaternion rot = Quaternion.Euler(0f, rotacaoYOuro, 0f);

        GameObject ouroGO = Instantiate(prefabOuro, pos, rot, mapaGerado[posicaoOuro].transform);
        ouroGO.name = "ouro";
        ouroGO.tag = "ouro";

        NormalizarWorldScale(ouroGO.transform, tamanhoMundialOuro);
        ouroGO.transform.localPosition += offsetLocalOuro;
        ouroGO.transform.localRotation = Quaternion.Euler(0f, rotacaoExtraYOuro, 0f) * ouroGO.transform.localRotation;

        if (logarEscalasNoSpawn)
            Debug.Log("[TowerGrid] Ouro lossy=" + ouroGO.transform.lossyScale);

        Vector3 posBrilho = pos + Vector3.up * 0.5f;
        GameObject brilhoGO = Instantiate(prefabBrilho, posBrilho, Quaternion.identity, mapaGerado[posicaoOuro].transform);
        brilhoGO.name = "Brilho";

        gridInfo[posicaoOuro].temOuro = true;
    }

    public void RemoverOuroNaPosicao(Vector2Int pos)
    {
        if (!gridInfo.ContainsKey(pos) || !gridInfo[pos].temOuro) return;

        gridInfo[pos].temOuro = false;
        ouroColetado = true;

        if (mapaGerado.TryGetValue(pos, out GameObject sala))
        {
            Transform ouro = null;
            Transform brilho = null;

            foreach (Transform child in sala.transform)
            {
                if (child.CompareTag("ouro")) ouro = child;
                else if (child.name == "Brilho" || child.name.Contains("Brilho")) brilho = child;
            }

            if (ouro) Destroy(ouro.gameObject);
            if (brilho) Destroy(brilho.gameObject);
        }

        TentarInstanciarEscadaSeElegivel();
    }

    public void TentarInstanciarEscadaSeElegivel()
    {
        if (escadaInstanciada) return;
        if (!ouroColetado || !wumpusMorto) return;

        Vector2Int posEscolhida;
        if (!EncontrarPosicaoEscada(out posEscolhida))
        {
            Debug.LogWarning("[TowerGrid] N�o foi poss�vel achar posi��o segura na �ltima fileira do eixo X para a escada.");
            return;
        }

        posicaoEscada = posEscolhida;

        if (mapaGerado.TryGetValue(posicaoEscada, out GameObject sala))
        {
            Vector3 pos = sala.transform.position + offsetEscada;
            GameObject escada = Instantiate(prefabEscada, pos, Quaternion.identity, sala.transform);
            escada.name = "Escada";
            escada.tag = "escada";

            gridInfo[posicaoEscada].temEscada = true;
            escadaInstanciada = true;
            OnEscadaLiberada?.Invoke();

            Debug.Log("[TowerGrid] Escada instanciada em " + posicaoEscada);
        }
    }

    bool EncontrarPosicaoEscada(out Vector2Int posOut)
    {
        int x = tamanhoX - 1; // �ltima fileira do eixo X
        for (int y = 0; y < tamanhoY; y++)
        {
            Vector2Int p = new Vector2Int(x, y);
            if (!gridInfo[p].temPoco)
            {
                posOut = p;
                return true;
            }
        }
        posOut = Vector2Int.zero;
        return false;
    }

    private void SpawnarOuReposicionarPlayer()
    {
        // 1) Garantir que a sala (0,0)
        if (!mapaGerado.TryGetValue(Vector2Int.zero, out GameObject sala00) || sala00 == null)
        {
            Debug.LogError("[TowerGrid] Sala (0,0) n�o encontrada ao spawnar player.");
            return;
        }
        if (!sala00.activeSelf) sala00.SetActive(true);

        // 2) Base de spawn = centro f�sico da sala (0,0)
        Vector3 basePos = sala00.transform.position + offsetCentroSala;

        // 3) Escolher prefab e offset
        string personagem = GameSessionManager.instancia.personagemEscolhido;
        GameObject prefab = personagem == "arqueiro" ? prefabArqueiro : prefabAmazona;
        Vector3 offsetPersonagem = personagem == "arqueiro" ? offsetArqueiro : offsetAmazona;

        // 4) Ver se existe player no pai dos players
        Transform existente = (paiDoPlayer != null && paiDoPlayer.childCount > 0) ? paiDoPlayer.GetChild(0) : null;

        if (existente != null)
        {
            // Reposiciona o mesmo player no centro da (0,0)
            Vector3 pos = basePos + offsetPersonagem;
            existente.SetPositionAndRotation(pos, Quaternion.identity);

            playerTr = existente;

            // Zera quaisquer velocidades residuais
            if (existente.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            GameManager.instancia?.DefinirPlayer(existente.gameObject);

            // Sinaliza para o movimento que chegamos num novo andar
            var mover = existente.GetComponent<TowerPlayerMovement>();
            if (mover != null) mover.OnChegouNovoAndar();
        }
        else
        {
            // Instancia um novo player ja no centro da (0,0)
            Vector3 pos = basePos + offsetPersonagem;
            GameObject player = Instantiate(prefab, pos, Quaternion.identity, paiDoPlayer);

            playerTr = player.transform;

            // Configura camera
            CameraFollow cam = FindFirstObjectByType<CameraFollow>();
            if (cam) cam.DefinirAlvo(player.transform.Find("CameraTarget") ?? player.transform);

            GameManager.instancia?.DefinirPlayer(player);
        }
    }


    private Vector3 CalcularOffsetDoPlayer(GameObject prefab)
    {
        Collider col = prefab != null ? prefab.GetComponentInChildren<Collider>() : null;
        if (col != null)
        {
            Bounds b = col.bounds;
            return -new Vector3(b.center.x, b.extents.y, b.center.z);
        }
        return Vector3.zero;
    }

    public void SubirParaProximoAndar()
    {
        andarAtual++;
        ouroColetado = false;
        wumpusMorto = false;
        escadaInstanciada = false;
        GerarNovoAndar();
    }

    private Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 p = worldPos - offsetCentroSala;
        int gx = Mathf.RoundToInt(p.x / espacoEntreSalas);
        int gy = Mathf.RoundToInt(p.z / espacoEntreSalas);
        return new Vector2Int(gx, gy);
    }

    public void OnClickAvancar()
    {
        if (!escadaInstanciada || playerTr == null) return;

        // Sobe apenas se o player estiver dentro da sala da escada
        if (WorldToGrid(playerTr.position) == posicaoEscada)
        {
            // Esconde o botão para evitar duplo clique
            ToggleBotaoAvancar(false);

            // Seu método já existente para trocar de andar
            SubirParaProximoAndar();
        }
    }



    public void LimparMapa()
    {
        if (paiDasSalas != null)
        {
            List<Transform> filhos = new List<Transform>();
            foreach (Transform t in paiDasSalas) filhos.Add(t);
            foreach (Transform t in filhos) Destroy(t.gameObject);
        }

        mapaGerado.Clear();
        gridInfo.Clear();
        posicaoEscada = Vector2Int.zero;
        escadaInstanciada = false;
    }

    void NormalizarWorldScale(Transform t, Vector3 desiredWorld)
    {
        if (!normalizarEscala || t == null) return;

        Vector3 parentLossy = Vector3.one;
        if (t.parent != null) parentLossy = t.parent.lossyScale;

        if (parentLossy.x == 0) parentLossy.x = 1;
        if (parentLossy.y == 0) parentLossy.y = 1;
        if (parentLossy.z == 0) parentLossy.z = 1;

        t.localScale = new Vector3(
            desiredWorld.x / parentLossy.x,
            desiredWorld.y / parentLossy.y,
            desiredWorld.z / parentLossy.z
        );
    }
}