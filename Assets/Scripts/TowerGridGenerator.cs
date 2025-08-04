using System.Collections.Generic;
using UnityEngine;

public class TowerGridGenerator : MonoBehaviour
{
    public static TowerGridGenerator instancia;
    public static System.Action OnNovoAndar;

    [Header("Prefabs dos Personagens")]
    public GameObject prefabArqueiro;
    public GameObject prefabAmazona;

    [Header("Offset Específico por Personagem")]
    public Vector3 offsetArqueiro = Vector3.zero;
    public Vector3 offsetAmazona = Vector3.zero;

    [Header("Prefab da Escada")]
    public GameObject prefabEscada;
    public Vector3 offsetEscada = Vector3.zero;

    [Header("Prefabs das Salas")]
    public GameObject salaPrefab;
    public GameObject salaComPocoPrefab;

    [Header("Prefabs do bloqueio")]
    public GameObject prefabBloqueioSala;

    [Header("Offset dos Bloqueios")]
    public Vector3 offsetBloqueioX = new Vector3(5f, 0f, 0f);  // Direita/esquerda
    public Vector3 offsetBloqueioZ = new Vector3(0f, 0f, 5f);  // Cima/baixo

    [Header("Prefabs do Wumpus e Ouro")]
    public GameObject prefabWumpus;
    public GameObject prefabOuro;

    [Header("Prefabs de Sensações")]
    public GameObject prefabBrisa;
    public GameObject prefabFedor;
    public GameObject prefabBrilho;

    [Header("Espaçamento e Organização")]
    public float espacoEntreSalas = 10f;
    public Transform paiDasSalas;
    public Transform paiDoPlayer;

    [Header("Offsets e Rotação")]
    public Vector3 offsetCentroSala = new Vector3(5, 0, 5);
    public float rotacaoYWumpus = 0f;
    public float rotacaoYOuro = 0f;

    public Vector2Int posicaoWumpus;
    public Vector2Int posicaoOuro;
    public bool wumpusMorto = false;
    public bool ouroColetado = false;
    public int andarAtual = 1;

    [Header("Mapa Gerado")]
    public Dictionary<Vector2Int, GameObject> mapaGerado = new Dictionary<Vector2Int, GameObject>();

    [Header("Mapa Lógico")]
    public Dictionary<Vector2Int, TileInfo> gridInfo = new Dictionary<Vector2Int, TileInfo>();

    [Header("Sensações por Posição")]
    public Dictionary<Vector2Int, List<string>> sensacoesPorPosicao = new Dictionary<Vector2Int, List<string>>();

    [System.Serializable]
    public class TileInfo
    {
        public bool temPoco = false;
        public bool temBrisa = false;
        public bool temFedor = false;
        public bool temOuro = false;
        public bool temWumpus;
        public bool foiVisitada = false;
    }

    private int tamanhoX, tamanhoY;

    private void Awake()
    {
        if (instancia == null) instancia = this;
        else { Destroy(gameObject); return; }

        GerarNovoAndar();
    }

    public void GerarNovoAndar()
    {
        tamanhoX = Random.Range(4, 11);
        tamanhoY = Random.Range(4, 11);
        Debug.Log($"[TowerGrid] Gerando andar {andarAtual}: {tamanhoX}x{tamanhoY}");

        LimparMapa();
        GerarMapa();
        GarantirSalaSeguraEm00();
        AplicarBrisaNosPocos();
        InstanciarWumpus();
        InstanciarOuro();
        SpawnarPlayer();

        OnNovoAndar?.Invoke();
    }

    private void GerarMapa()
    {
        for (int x = 0; x < tamanhoX; x++)
        {
            for (int y = 0; y < tamanhoY; y++)
            {
                Vector3 pos = new Vector3(x * espacoEntreSalas, 0, y * espacoEntreSalas);
                Vector2Int gridPos = new(x, y);

                bool temPoco = Random.value < 0.2f;
                GameObject sala = Instantiate(
                    temPoco ? salaComPocoPrefab : salaPrefab,
                    pos,
                    Quaternion.identity,
                    paiDasSalas
                );

                sala.name = $"Sala ({x},{y})";
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
            Debug.LogError("[TowerGrid] Sala (0,0) não existe no gridInfo.");
        }
    }

    void AdicionarBloqueios(Vector2Int pos, GameObject sala)
    {
        // Direções possíveis e seus respectivos offsets
        Vector2Int[] direcoes = new Vector2Int[]
        {
        Vector2Int.up,     // Z+
        Vector2Int.down,   // Z-
        Vector2Int.right,  // X+
        Vector2Int.left    // X-
        };

        foreach (var dir in direcoes)
        {
            Vector2Int vizinha = pos + dir;

            if (!gridInfo.ContainsKey(vizinha))
            {
                Vector3 offset = Vector3.zero;
                Quaternion rotacao = Quaternion.identity;

                // Define o offset e a rotação com base na direção
                if (dir == Vector2Int.up)
                {
                    offset = offsetBloqueioZ;
                }
                else if (dir == Vector2Int.down)
                {
                    offset = -offsetBloqueioZ;
                    rotacao = Quaternion.Euler(0, 180, 0);
                }
                else if (dir == Vector2Int.right)
                {
                    offset = offsetBloqueioX;
                    rotacao = Quaternion.Euler(0, 90, 0);
                }
                else if (dir == Vector2Int.left)
                {
                    offset = -offsetBloqueioX;
                    rotacao = Quaternion.Euler(0, -90, 0);
                }

                if (prefabBloqueioSala != null)
                {
                    GameObject bloqueio = Instantiate(
                        prefabBloqueioSala,
                        sala.transform.position + offset,
                        rotacao,
                        sala.transform
                    );
                }
            }
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

            // Remover fedor das salas adjacentes
            Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in dirs)
            {
                Vector2Int adj = pos + dir;
                if (gridInfo.ContainsKey(adj))
                {
                    gridInfo[adj].temFedor = false;

                    if (mapaGerado.TryGetValue(adj, out GameObject salaAdj))
                    {
                        foreach (Transform child in salaAdj.transform)
                        {
                            if (child.name.Contains("Fedor"))
                                Destroy(child.gameObject);
                        }
                    }
                }
            }
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
        Instantiate(prefabOuro, pos, rot, mapaGerado[posicaoOuro].transform).tag = "ouro";

        Vector3 posBrilho = pos + Vector3.up * 0.5f;
        Instantiate(prefabBrilho, posBrilho, Quaternion.identity, mapaGerado[posicaoOuro].transform);
        gridInfo[posicaoOuro].temOuro = true;
    }

    private void SpawnarPlayer()
    {
        string personagem = GameSessionManager.instancia.personagemEscolhido;
        GameObject prefab = personagem == "arqueiro" ? prefabArqueiro : prefabAmazona;
        Vector3 offset = personagem == "arqueiro" ? offsetArqueiro : offsetAmazona;

        Vector3 pos = Vector3.zero + offsetCentroSala + CalcularOffsetDoPlayer(prefab) + offset;
        GameObject player = Instantiate(prefab, pos, Quaternion.identity, paiDoPlayer);

        CameraFollow cam = FindFirstObjectByType<CameraFollow>();
        if (cam) cam.DefinirAlvo(player.transform.Find("CameraTarget") ?? player.transform);
        GameManager.instancia?.DefinirPlayer(player);
    }

    private Vector3 CalcularOffsetDoPlayer(GameObject prefab)
    {
        Collider col = prefab.GetComponentInChildren<Collider>();
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
        GerarNovoAndar();
    }

    public void LimparMapa()
    {
        foreach (Transform t in paiDasSalas) Destroy(t.gameObject);
        mapaGerado.Clear();
        gridInfo.Clear();
    }
}
