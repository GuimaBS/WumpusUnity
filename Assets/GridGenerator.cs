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

    public static Vector2Int posicaoWumpus;

    [Header("Configurações")]
    public float tileSize = 1f;
    [Range(0f, 1f)]
    public float densidadeDePocos = 0.12f;

    private int xSize;
    private int ySize;
    private TileManager.TileInfo[,] grid;

    void Start()
    {
        Debug.Log("Gerador de mapa iniciado!");

        xSize = PlayerPrefs.GetInt("mapX", 5);
        ySize = PlayerPrefs.GetInt("mapY", 5);

        MemoriaVisual.instancia?.InicializarMapa(xSize, ySize);

        grid = new TileManager.TileInfo[xSize, ySize];

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0, y * tileSize);
                bool isPoco = (x != 0 || y != 0) && Random.value < densidadeDePocos;

                GameObject prefab = isPoco ? tilepPrefab : tilePrefab;
                GameObject tile = Instantiate(prefab, pos, Quaternion.identity, transform);

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
                           grid[x, y].temPoco,
                           grid[x, y].temBrisa,
                           grid[x, y].temFedor,
                           grid[x, y].temOuro
                           );
            }
        }

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

        bool wumpusColocado = false;
        while (!wumpusColocado)
        {
            int x = Random.Range(0, xSize);
            int y = Random.Range(0, ySize);

            if ((x != 0 || y != 0) && !grid[x, y].temPoco)
            {
                Vector3 pos = new Vector3(x * tileSize, 0.5f, y * tileSize);
                Quaternion rot = Quaternion.Euler(0, 180f, 0);
                Instantiate(wumpusPrefab, pos, rot, transform);

                posicaoWumpus = new Vector2Int(x, y);
                wumpusColocado = true;

                AdicionarFedor(x + 1, y);
                AdicionarFedor(x - 1, y);
                AdicionarFedor(x, y + 1);
                AdicionarFedor(x, y - 1);
            }
        }

        int ourosParaGerar = (xSize >= 10 && ySize >= 10) ? 2 : 1;
        int ourosColocados = 0;

        while (ourosColocados < ourosParaGerar)
        {
            int x = Random.Range(0, xSize);
            int y = Random.Range(0, ySize);

            if ((x != 0 || y != 0) && !grid[x, y].temPoco && !grid[x, y].temOuro)
            {
                Vector3 pos = new Vector3(x * tileSize, 0.5f, y * tileSize);
                Quaternion rot = Quaternion.Euler(0, 180f, 0);
                GameObject ouro = Instantiate(ouroPrefab, pos, rot, transform);

                grid[x, y].temOuro = true;
                TileManager.instancia.ObterInfoDaTile(new Vector2Int(x, y)).temOuro = true;

                if (brilhoOuro != null)
                {
                    Vector3 brilhoPos = pos + Vector3.up * 0.5f;
                    Instantiate(brilhoOuro, brilhoPos, Quaternion.identity, ouro.transform);
                }

                ourosColocados++;
            }
        }

        LogManager.instancia.AdicionarLog($"Bem-vindo ao Labirinto de Wumpus! Mapa gerado: ({xSize},{ySize})");
    }

    void AdicionarBrisa(int x, int y)
    {
        if (x < 0 || x >= xSize || y < 0 || y >= ySize) return;
        if (grid[x, y].temPoco || grid[x, y].temBrisa) return;

        grid[x, y].temBrisa = true;
        TileManager.instancia.ObterInfoDaTile(new Vector2Int(x, y)).temBrisa = true;

        if (brisaEffect != null)
        {
            Vector3 pos = new Vector3(x * tileSize, 0.01f, y * tileSize);
            Instantiate(brisaEffect, pos, Quaternion.identity, transform);
        }
    }

    void AdicionarFedor(int x, int y)
    {
        if (x < 0 || x >= xSize || y < 0 || y >= ySize) return;
        if (grid[x, y].temPoco || grid[x, y].temFedor) return;

        grid[x, y].temFedor = true;
        TileManager.instancia.ObterInfoDaTile(new Vector2Int(x, y)).temFedor = true;

        if (fedorEffect != null)
        {
            Vector3 pos = new Vector3(x * tileSize, 0.02f, y * tileSize);
            Instantiate(fedorEffect, pos, Quaternion.identity, transform);
        }
    }
}
