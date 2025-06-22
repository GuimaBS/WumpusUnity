using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgenteManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject agente1Prefab;
    public GameObject agente2Prefab;
    public GameObject wumpusPrefab;

    [Header("Partículas")]
    public GameObject fedorEffectPrefab;
    public GameObject spawnEffectPrefab;

    [Header("Configurações")]
    public Transform agentePai;
    public Toggle autoSpawnToggle;

    private bool autoSpawnAtivo = false;
    private List<GameObject> agentesVivos = new List<GameObject>();

    private void Start()
    {
        if (autoSpawnToggle != null)
        {
            autoSpawnToggle.onValueChanged.AddListener(delegate { autoSpawnAtivo = autoSpawnToggle.isOn; });
        }

        if (spawnEffectPrefab == null)
            Debug.LogError("Prefab de partícula de spawn NÃO atribuído no inspector!");
    }

    public void CriarAgente1()
    {
        Vector2Int posicaoTile = new Vector2Int(0, 0); // Tile (0,0) — canto inferior esquerdo
        Vector3 posicaoMundo = new Vector3(posicaoTile.x * 1.7f, 0, posicaoTile.y * 1.7f);

        CriarAgente(agente1Prefab, "Agente1", posicaoTile, posicaoMundo);
    }

    public void CriarAgente2()
    {
        int yMax = GridGenerator.tamanhoY - 1;
        Vector2Int posicaoTile = new Vector2Int(0, yMax); // Tile (0, yMax) — canto superior esquerdo
        Vector3 posicaoMundo = new Vector3(posicaoTile.x * 1.7f, 0, posicaoTile.y * 1.7f);

        CriarAgente(agente2Prefab, "Agente2", posicaoTile, posicaoMundo);
    }

    private void CriarAgente(GameObject prefab, string nomeAgente, Vector2Int posicaoTile, Vector3 posicaoMundo)
    {
        Quaternion rotacaoInicial = Quaternion.Euler(0, 0, 0);

        GameObject novoAgente = Instantiate(prefab, posicaoMundo, rotacaoInicial, agentePai);
        agentesVivos.Add(novoAgente);

        EmitirParticulaSpawn(posicaoTile);

        if (nomeAgente == "Agente1")
        {
            var script = novoAgente.GetComponent<AgenteReativo>();
            if (script != null)
            {
                script.onMorte += () =>
                {
                    agentesVivos.Remove(novoAgente);
                    if (autoSpawnAtivo) CriarAgente1();
                };
            }
        }
        else if (nomeAgente == "Agente2")
        {
            var script = novoAgente.GetComponent<AgenteInteligente>();
            if (script != null)
            {
                script.onMorte += () =>
                {
                    agentesVivos.Remove(novoAgente);
                    if (autoSpawnAtivo) CriarAgente2();
                };
            }
        }

        LogManager.instancia?.AdicionarLog($"{nomeAgente} criado com sucesso.");
    }

    private void EmitirParticulaSpawn(Vector2Int posicaoTile)
    {
        if (spawnEffectPrefab == null)
        {
            Debug.LogWarning("Prefab de partícula de spawn não atribuído. Efeito não emitido.");
            return;
        }

        Vector3 posicaoMundo = new Vector3(posicaoTile.x * 1.7f, 0.5f, posicaoTile.y * 1.7f);
        Instantiate(spawnEffectPrefab, posicaoMundo + Vector3.up * 0.5f, Quaternion.identity);
    }

    public void RemoverTodosAgentes()
    {
        foreach (var agente in agentesVivos)
        {
            if (agente != null)
                Destroy(agente);
        }
        agentesVivos.Clear();
    }

    public void SpawnarWumpusAleatorio()
    {
        List<Vector2Int> tilesValidas = new List<Vector2Int>();

        foreach (Vector2Int pos in TileManager.instancia.ObterTodasAsPosicoes())
        {
            var info = TileManager.instancia.ObterInfoDaTile(pos);

            // Verifica se não é poço e se não tem outro Wumpus já na posição
            if (info != null && !info.temPoco && !GridGenerator.posicoesWumpus.Contains(pos))
            {
                tilesValidas.Add(pos);
            }
        }

        if (tilesValidas.Count == 0)
        {
            Debug.LogWarning("Nenhuma tile válida disponível para spawn do Wumpus.");
            LogManager.instancia?.AdicionarLog("<color=red><b>Não há tiles disponíveis para spawn do Wumpus!</b></color>");
            return;
        }

        // Seleciona uma tile aleatória
        Vector2Int posicaoWumpus = tilesValidas[Random.Range(0, tilesValidas.Count)];
        Vector3 posicaoMundo = new Vector3(posicaoWumpus.x * 1.7f, 0.5f, posicaoWumpus.y * 1.7f);

        // Instancia o Wumpus
        Instantiate(wumpusPrefab, posicaoMundo, Quaternion.Euler(0, 180f, 0));

        // Adiciona no GridGenerator
        GridGenerator.posicoesWumpus.Add(posicaoWumpus);

        // Usa o método correto para adicionar fedor com partículas
        GridGenerator.instancia.AdicionarFedorNasAdjacentes(posicaoWumpus);

        // Efeito de spawn opcional
        EmitirParticulaSpawn(posicaoWumpus);

        LogManager.instancia?.AdicionarLog($"<color=green><b>Wumpus foi instanciado na posição {posicaoWumpus}!</b></color>");
    }

    private List<Vector2Int> Direcoes()
    {
        return new List<Vector2Int>
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };
    }
}
