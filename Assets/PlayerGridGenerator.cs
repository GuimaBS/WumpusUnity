using System.Collections.Generic;
using UnityEngine;

public class PlayerGridGenerator : MonoBehaviour
{
    public static PlayerGridGenerator instancia;
    public static System.Action OnMapaGerado;

    [Header("Prefabs das Salas")]
    public GameObject salaPrefab;
    public GameObject salaComPocoPrefab;

    [Header("Prefabs dos Personagens")]
    public GameObject prefabArqueiro;
    public GameObject prefabAmazona;

    [Header("Offset Específico por Personagem")]
    public Vector3 offsetArqueiro = Vector3.zero;
    public Vector3 offsetAmazona = Vector3.zero;

    [Header("Prefab de Sensações")]
    public GameObject prefabBrisa;
    public GameObject prefabFedor;
    public GameObject prefabBrilho;

    [Header("Prefab do Wumpus e do Ouro")]
    public GameObject prefabWumpus;
    public GameObject prefabOuro;

    [Header("Prefab de Bloqueio")]
    public GameObject prefabBloqueio;
    public Vector3 offsetBloqueio = Vector3.zero;

    [Header("Configuração do Mapa")]
    public float espacoEntreSalas = 10f;
    public Transform paiDasSalas;
    public Transform paiDoPlayer;

    [Header("Offset para Centralizar na Sala")]
    public Vector3 offsetCentroSala = new Vector3(5, 0, 5);

    [Header("Mapa Gerado")]
    public Dictionary<Vector2Int, GameObject> mapaGerado = new Dictionary<Vector2Int, GameObject>();

    [Header("Mapa Lógico")]
    public Dictionary<Vector2Int, TileInfo> gridInfo = new Dictionary<Vector2Int, TileInfo>();

    public Vector2Int posicaoWumpus;
    public Vector2Int posicaoOuro;

    [System.Serializable]
    public class TileInfo
    {
        public bool temPoco = false;
        public bool temBrisa = false;
        public bool temFedor = false;
        public bool temOuro = false;
        public bool foiVisitada = false;
    }

    private void Awake()
    {
        if (instancia == null) instancia = this;
        else { Destroy(gameObject); return; }

        tamanhoX = PlayerPrefs.GetInt("mapX");
        tamanhoY = PlayerPrefs.GetInt("mapY");

        Debug.Log($"Gerando mapa {tamanhoX}x{tamanhoY}");

        GerarMapa();
        GarantirSalaSeguraEm00();
        AplicarBrisaNosPocos();
        InstanciarWumpus();
        InstanciarOuro();
        SpawnarPlayer();

        OnMapaGerado?.Invoke();
    }

    private int tamanhoX;
    private int tamanhoY;

    public void GerarMapa()
    {
        LimparMapa();

        for (int x = 0; x < tamanhoX; x++)
        {
            for (int y = 0; y < tamanhoY; y++)
            {
                Vector3 pos = new Vector3(x * espacoEntreSalas, 0, y * espacoEntreSalas);
                Vector2Int gridPos = new Vector2Int(x, y);

                GameObject sala;
                bool temPoco = Random.value < 0.2f;

                if (temPoco)
                {
                    sala = Instantiate(salaComPocoPrefab, pos, Quaternion.identity, paiDasSalas);
                    sala.tag = "SalaP";
                }
                else
                {
                    sala = Instantiate(salaPrefab, pos, Quaternion.identity, paiDasSalas);
                }

                sala.name = $"Sala ({x},{y})";
                mapaGerado.Add(gridPos, sala);

                TileInfo info = new TileInfo { temPoco = temPoco };
                gridInfo.Add(gridPos, info);
            }
        }

        GerarBloqueiosDeBorda();
    }

    private void GerarBloqueiosDeBorda()
    {
        foreach (var kvp in mapaGerado)
        {
            Vector2Int pos = kvp.Key;
            GameObject sala = kvp.Value;

            Vector3 salaPos = sala.transform.position;

            Vector2Int[] direcoes = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            Vector3[] posicoesPortas = {
                salaPos + new Vector3(0, 0, espacoEntreSalas / 2),
                salaPos + new Vector3(0, 0, -espacoEntreSalas / 2),
                salaPos + new Vector3(-espacoEntreSalas / 2, 0, 0),
                salaPos + new Vector3(espacoEntreSalas / 2, 0, 0)
            };
            Vector3[] rotacoes = {
                new Vector3(0, 0, 0),
                new Vector3(0, 180, 0),
                new Vector3(0, -90, 0),
                new Vector3(0, 90, 0)
            };

            for (int i = 0; i < direcoes.Length; i++)
            {
                Vector2Int direcao = direcoes[i];
                Vector2Int destino = pos + direcao;

                if (!gridInfo.ContainsKey(destino))
                {
                    GameObject bloqueio = Instantiate(
                        prefabBloqueio,
                        posicoesPortas[i] + offsetBloqueio,
                        Quaternion.Euler(rotacoes[i]),
                        sala.transform
                    );
                    bloqueio.name = $"Bloqueio_{pos}_{direcao}";
                }
            }
        }
    }

    private void GarantirSalaSeguraEm00()
    {
        Vector2Int posInicial = new Vector2Int(0, 0);
        if (gridInfo[posInicial].temPoco)
        {
            Destroy(mapaGerado[posInicial]);
            mapaGerado.Remove(posInicial);
            gridInfo[posInicial].temPoco = false;

            GameObject novaSala = Instantiate(salaPrefab, Vector3.zero, Quaternion.identity, paiDasSalas);
            novaSala.name = "Sala (0,0)";
            mapaGerado.Add(posInicial, novaSala);
        }
    }

    private void AplicarBrisaNosPocos()
    {
        foreach (var kvp in gridInfo)
        {
            Vector2Int pos = kvp.Key;
            if (kvp.Value.temPoco)
            {
                Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

                foreach (Vector2Int dir in dirs)
                {
                    Vector2Int adj = pos + dir;
                    if (gridInfo.ContainsKey(adj) && !gridInfo[adj].temPoco)
                    {
                        gridInfo[adj].temBrisa = true;

                        GameObject salaAdj = mapaGerado[adj];
                        if (salaAdj.transform.Find("Brisa") == null)
                        {
                            Vector3 posBrisa = salaAdj.transform.position + new Vector3(0, 1.5f, 0);
                            Instantiate(prefabBrisa, posBrisa, Quaternion.identity, salaAdj.transform).name = "Brisa";
                        }
                    }
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

        Vector3 pos = mapaGerado[posicaoWumpus].transform.position + new Vector3(0, 0.5f, 0);
        Instantiate(prefabWumpus, pos, Quaternion.identity, mapaGerado[posicaoWumpus].transform).tag = "wumpus";

        AplicarFedorNoWumpus(posicaoWumpus);
    }

    private void AplicarFedorNoWumpus(Vector2Int origem)
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in dirs)
        {
            Vector2Int adj = origem + dir;
            if (gridInfo.ContainsKey(adj))
            {
                gridInfo[adj].temFedor = true;

                GameObject salaAdj = mapaGerado[adj];
                if (salaAdj.transform.Find("Fedor") == null)
                {
                    Vector3 posFedor = salaAdj.transform.position + new Vector3(0, 1.5f, 0);
                    Instantiate(prefabFedor, posFedor, Quaternion.identity, salaAdj.transform).name = "Fedor";
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
        while (posicaoOuro == Vector2Int.zero ||
               gridInfo[posicaoOuro].temPoco ||
               posicaoOuro == posicaoWumpus);

        Vector3 pos = mapaGerado[posicaoOuro].transform.position + new Vector3(0, 0.5f, 0);

        GameObject ouroObj = Instantiate(prefabOuro, pos, Quaternion.identity, mapaGerado[posicaoOuro].transform);
        ouroObj.name = "ouro";
        ouroObj.tag = "ouro";

        Vector3 posBrilho = pos + new Vector3(0, 0.5f, 0);
        Instantiate(prefabBrilho, posBrilho, Quaternion.identity, mapaGerado[posicaoOuro].transform).name = "Brilho";

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

        if (GameManager.instancia != null) GameManager.instancia.DefinirPlayer(player);
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

    public void LimparMapa()
    {
        foreach (Transform t in paiDasSalas) Destroy(t.gameObject);
        mapaGerado.Clear();
        gridInfo.Clear();
    }
}
