using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject tilepPrefab;
    public GameObject wumpusPrefab;
    public GameObject ouroPrefab;
    public GameObject brilhoOuro;
    public GameObject brisaEffect;
    public GameObject fedorEffect;

    [Header("Estado Global")]
    public static bool wumpusVivo = true;
    public static bool ouroColetado = false;
    public static List<Vector2Int> posicoesWumpus = new List<Vector2Int>();
    public static List<GameObject> instanciasWumpus = new List<GameObject>();
    public static Vector2Int posicaoOuro;
    public static int tamanhoX;
    public static int tamanhoY;

    public static GameObject instanciaOuro;
    public static Dictionary<Vector2Int, GameObject> efeitosFedor = new Dictionary<Vector2Int, GameObject>();

    [Header("Configurações")]
    public float tileSize = 1.7f;
    [Range(0f, 1f)]
    public float densidadeDePocos = 0.12f;

    private int xSize;
    private int ySize;
    private TileManager.TileInfo[,] grid;

    void Start()
    {
        wumpusVivo = true;
        ouroColetado = false;
        posicoesWumpus.Clear();
        instanciasWumpus.Clear();
        efeitosFedor.Clear();

        xSize = PlayerPrefs.GetInt("mapX", 5);
        ySize = PlayerPrefs.GetInt("mapY", 5);

        tamanhoX = xSize;
        tamanhoY = ySize;

        MemoriaVisual.instancia?.InicializarMapa(xSize, ySize);

        grid = new TileManager.TileInfo[xSize, ySize];

        CriarTiles();
        AdicionarBrisas();
        ColocarWumpus();
        ColocarOuro();

        LogManager.instancia.AdicionarLog($"Mapa gerado ({xSize},{ySize})!");
    }

    void CriarTiles()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0, y * tileSize);

                bool ehCasaAgente1 = (x == 0 && y == 0);
                bool ehCasaAgente2 = (x == 0 && y == (ySize - 1));

                bool isPoco = !ehCasaAgente1 && !ehCasaAgente2 && Random.value < densidadeDePocos;

                GameObject prefab = isPoco ? tilepPrefab : tilePrefab;
                GameObject tile = Instantiate(prefab, pos, Quaternion.identity);

                TileManager.TileInfo info = new TileManager.TileInfo
                {
                    temPoco = isPoco,
                    temBrisa = false,
                    temFedor = false,
                    temOuro = false
                };

                grid[x, y] = info;

                TileManager.instancia.RegistrarTile(
                    new Vector2Int(x, y),
                    tile,
                    info.temPoco,
                    info.temBrisa,
                    info.temFedor,
                    info.temOuro
                );
            }
        }
    }

    void AdicionarBrisas()
    {
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                if (grid[x, y].temPoco)
                {
                    AdicionarBrisa(x + 1, y);
                    AdicionarBrisa(x - 1, y);
                    AdicionarBrisa(x, y + 1);
                    AdicionarBrisa(x, y - 1);
                }
            }
        }
    }

    void ColocarWumpus()
    {
        int quantidadeWumpus = (xSize >= 10 && ySize >= 10) ? 2 : 1;
        int colocados = 0;

        while (colocados < quantidadeWumpus)
        {
            int x = Random.Range(0, xSize);
            int y = Random.Range(0, ySize);

            bool ehCasaAgente1 = (x == 0 && y == 0);
            bool ehCasaAgente2 = (x == 0 && y == (ySize - 1));
            Vector2Int pos = new Vector2Int(x, y);

            bool jaPossuiOutroWumpus = posicoesWumpus.Contains(pos);

            if (!ehCasaAgente1 && !ehCasaAgente2 &&
                !grid[x, y].temPoco && !jaPossuiOutroWumpus)
            {
                Vector3 posMundo = new Vector3(x * tileSize, 0.5f, y * tileSize);
                GameObject wumpus = Instantiate(wumpusPrefab, posMundo, Quaternion.Euler(0, 180f, 0));

                posicoesWumpus.Add(pos);
                instanciasWumpus.Add(wumpus);

                AdicionarFedor(x + 1, y);
                AdicionarFedor(x - 1, y);
                AdicionarFedor(x, y + 1);
                AdicionarFedor(x, y - 1);

                colocados++;
            }
        }

        wumpusVivo = true;
    }

    void ColocarOuro()
    {
        bool colocado = false;

        while (!colocado)
        {
            int x = Random.Range(0, xSize);
            int y = Random.Range(0, ySize);

            bool ehCasaAgente1 = (x == 0 && y == 0);
            bool ehCasaAgente2 = (x == 0 && y == (ySize - 1));

            if (!ehCasaAgente1 && !ehCasaAgente2 &&
                !grid[x, y].temPoco && !grid[x, y].temOuro)
            {
                Vector3 pos = new Vector3(x * tileSize, 0.5f, y * tileSize);
                instanciaOuro = Instantiate(ouroPrefab, pos, Quaternion.Euler(0, 180f, 0));

                posicaoOuro = new Vector2Int(x, y);
                grid[x, y].temOuro = true;
                TileManager.instancia.ObterInfoDaTile(posicaoOuro).temOuro = true;

                if (brilhoOuro != null)
                {
                    Instantiate(brilhoOuro, pos + Vector3.up * 0.5f, Quaternion.identity, instanciaOuro.transform);
                }

                colocado = true;
            }
        }
    }

    void AdicionarBrisa(int x, int y)
    {
        if (x < 0 || x >= xSize || y < 0 || y >= ySize) return;
        if (grid[x, y].temPoco) return;

        if (!grid[x, y].temBrisa)
        {
            grid[x, y].temBrisa = true;
            TileManager.instancia.ObterInfoDaTile(new Vector2Int(x, y)).temBrisa = true;

            if (brisaEffect != null)
            {
                Vector3 pos = new Vector3(x * tileSize, 0.01f, y * tileSize);
                Instantiate(brisaEffect, pos, Quaternion.identity);
            }
        }
    }

    void AdicionarFedor(int x, int y)
    {
        if (x < 0 || x >= xSize || y < 0 || y >= ySize) return;
        if (grid[x, y].temPoco) return;

        if (!grid[x, y].temFedor)
        {
            grid[x, y].temFedor = true;
            TileManager.instancia.ObterInfoDaTile(new Vector2Int(x, y)).temFedor = true;

            if (fedorEffect != null)
            {
                Vector3 pos = new Vector3(x * tileSize, 0.02f, y * tileSize);
                GameObject efeito = Instantiate(fedorEffect, pos, Quaternion.identity);

                Vector2Int chave = new Vector2Int(x, y);
                if (!efeitosFedor.ContainsKey(chave))
                {
                    efeitosFedor.Add(chave, efeito);
                }
            }
        }
    }

    public static void RemoverFedor(Vector2Int origem)
    {
        Vector2Int[] adjacentes = new Vector2Int[]
        {
            new Vector2Int(origem.x + 1, origem.y),
            new Vector2Int(origem.x - 1, origem.y),
            new Vector2Int(origem.x, origem.y + 1),
            new Vector2Int(origem.x, origem.y - 1)
        };

        foreach (var pos in adjacentes)
        {
            var info = TileManager.instancia.ObterInfoDaTile(pos);
            if (info != null)
            {
                info.temFedor = false;
                MemoriaVisual.instancia?.AtualizarTile(pos, "vazio");
            }

            if (efeitosFedor.ContainsKey(pos))
            {
                GameObject efeito = efeitosFedor[pos];
                if (efeito != null)
                {
                    GameObject.Destroy(efeito);
                }
                efeitosFedor.Remove(pos);
            }
        }
    }

    public static void EliminarWumpusNaPosicao(Vector2Int posicao)
    {
        int index = posicoesWumpus.IndexOf(posicao);

        if (index != -1)
        {
            GameObject wumpus = instanciasWumpus[index];

            if (wumpus != null)
            {
                Destroy(wumpus);
                Debug.Log($"Wumpus na posição {posicao} destruído.");
            }

            posicoesWumpus.RemoveAt(index);
            instanciasWumpus.RemoveAt(index);

            RemoverFedor(posicao);

            MemoriaVisual.instancia?.AtualizarTile(posicao, "vazio");


            if (posicoesWumpus.Count == 0)
            {
                wumpusVivo = false;
            }
        }
    }

    public static void ColetarOuroNaPosicao(Vector2Int posicao)
    {
        ouroColetado = true;

        TileManager.TileInfo info = TileManager.instancia.ObterInfoDaTile(posicao);
        if (info != null)
        {
            info.temOuro = false;
        }

        if (instanciaOuro != null)
        {
            Destroy(instanciaOuro);
            Debug.Log("Ouro coletado e objeto destruído.");
        }

        MemoriaVisual.instancia?.AtualizarTile(posicao, "vazio");

    }
}
