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
    private TileInfo[,] grid;

    public class TileInfo
    {
        public GameObject instancia;
        public bool temPoco = false;
        public bool temBrisa = false;
        public bool temFedor = false;
        public bool temOuro = false;
    }

    void Start()
    {
        Debug.Log("Gerador de mapa iniciado!");

        xSize = PlayerPrefs.GetInt("mapX", 5);
        ySize = PlayerPrefs.GetInt("mapY", 5);

        grid = new TileInfo[xSize, ySize];

        // Etapa 1 – Gerar Tiles (algumas com poço)
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0, y * tileSize);
                bool isPoco = (x != 0 || y != 0) && Random.value < densidadeDePocos;

                GameObject prefab = isPoco ? tilepPrefab : tilePrefab;
                GameObject tile = Instantiate(prefab, pos, Quaternion.identity, transform);
                Vector2Int posicaoTile = new Vector2Int(x, y);
                TileManager.instancia.RegistrarTile(new Vector2Int(x, y), tile);

                grid[x, y] = new TileInfo
                {
                    instancia = tile,
                    temPoco = isPoco
                };
            }
        }

        // Etapa 2 – Adicionar brisas ao redor dos poços
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

        // Etapa 3 – Instanciar Wumpus
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

                posicaoWumpus = new Vector2Int(x, y); // salva a posição lógica do Wumpus

                wumpusColocado = true;

                AdicionarFedor(x + 1, y);
                AdicionarFedor(x - 1, y);
                AdicionarFedor(x, y + 1);
                AdicionarFedor(x, y - 1);
            }
        }

        // Etapa 4 – Instanciar ouro(s)
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

                if (brilhoOuro != null)
                {
                    Vector3 brilhoPos = pos + Vector3.up * 0.5f;
                    Instantiate(brilhoOuro, brilhoPos, Quaternion.identity, ouro.transform);
                }

                ourosColocados++;
            }
        }
    }

    void AdicionarBrisa(int x, int y)
    {
        if (x < 0 || x >= xSize || y < 0 || y >= ySize) return;
        if (grid[x, y].temPoco || grid[x, y].temBrisa) return;

        grid[x, y].temBrisa = true;

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

        if (fedorEffect != null)
        {
            Vector3 pos = new Vector3(x * tileSize, 0.02f, y * tileSize);
            Instantiate(fedorEffect, pos, Quaternion.identity, transform);
        }
    }
}
