using System.Collections.Generic;
using UnityEngine;

public class PlayerGridGenerator : MonoBehaviour
{
    [Header("Prefabs das Salas")]
    public GameObject salaPrefab;
    public GameObject salaComPocoPrefab;

    [Header("Prefabs dos Personagens")]
    public GameObject prefabArqueiro;
    public GameObject prefabAmazona;

    [Header("Configuração do Mapa")]
    public float espacoEntreSalas = 10f;
    public Transform paiDasSalas;
    public Transform paiDoPlayer;

    [Header("Offset para Centralizar na Sala")]
    public Vector3 offsetCentroSala = new Vector3(5, 0, 5);

    [Header("Mapa Gerado")]
    public Dictionary<Vector2Int, GameObject> mapaGerado = new Dictionary<Vector2Int, GameObject>();

    private int tamanhoX;
    private int tamanhoY;

    private GameObject playerInstanciado;

    private void Awake()
    {
        tamanhoX = PlayerPrefs.GetInt("mapX");
        tamanhoY = PlayerPrefs.GetInt("mapY");

        Debug.Log($"Gerando mapa de tamanho {tamanhoX}x{tamanhoY} na ClassicScene");

        GerarMapa();
        GarantirSalaSeguraEm00();
        SpawnarPlayer();
    }

    public void GerarMapa()
    {
        LimparMapa();

        for (int x = 0; x < tamanhoX; x++)
        {
            for (int y = 0; y < tamanhoY; y++)
            {
                Vector3 posicao = new Vector3(x * espacoEntreSalas, 0, y * espacoEntreSalas);
                Vector2Int posicaoGrid = new Vector2Int(x, y);

                GameObject salaInstanciada;

                bool temPoco = Random.value < 0.2f;

                if (temPoco)
                {
                    salaInstanciada = Instantiate(salaComPocoPrefab, posicao, Quaternion.identity, paiDasSalas);
                }
                else
                {
                    salaInstanciada = Instantiate(salaPrefab, posicao, Quaternion.identity, paiDasSalas);
                }

                salaInstanciada.name = $"Sala ({x},{y})";
                mapaGerado.Add(posicaoGrid, salaInstanciada);
            }
        }
    }

    private void GarantirSalaSeguraEm00()
    {
        Vector2Int posicaoInicial = new Vector2Int(0, 0);

        if (mapaGerado.ContainsKey(posicaoInicial))
        {
            GameObject salaNaPosicao = mapaGerado[posicaoInicial];

            if (salaNaPosicao.CompareTag("SalaP") || salaNaPosicao.name.Contains("SalaP"))
            {
                Debug.LogWarning("Sala (0,0) tinha poço! Substituindo por sala comum.");

                Destroy(salaNaPosicao);
                mapaGerado.Remove(posicaoInicial);

                Vector3 posicao = new Vector3(0, 0, 0);
                GameObject novaSala = Instantiate(salaPrefab, posicao, Quaternion.identity, paiDasSalas);
                novaSala.name = "Sala (0,0)";

                mapaGerado.Add(posicaoInicial, novaSala);
            }
        }
    }

    private void SpawnarPlayer()
    {
        Vector3 posicaoSala = new Vector3(0, 0, 0);

        string personagem = GameSessionManager.instancia.personagemEscolhido;
        Debug.Log("Personagem selecionado para spawn: " + personagem);

        GameObject prefabSelecionado = null;

        switch (personagem)
        {
            case "arqueiro":
                prefabSelecionado = prefabArqueiro;
                break;

            case "amazona":
                prefabSelecionado = prefabAmazona;
                break;

            default:
                Debug.LogError("Personagem não reconhecido. Verifique a string.");
                return;
        }

        Vector3 offsetDoPlayer = CalcularOffsetDoPlayer(prefabSelecionado);
        Vector3 posicaoFinal = posicaoSala + offsetCentroSala + offsetDoPlayer;

        playerInstanciado = Instantiate(prefabSelecionado, posicaoFinal, Quaternion.identity, paiDoPlayer);

        Debug.Log($"{personagem} spawnado na sala (0,0) na posição {posicaoFinal}");

        // Configurar a câmera para seguir o CameraTarget dentro do prefab
        CameraFollow cameraFollow = FindFirstObjectByType<CameraFollow>();
        if (cameraFollow != null)
        {
            Transform cameraTarget = playerInstanciado.transform.Find("CameraTarget");

            if (cameraTarget != null)
            {
                cameraFollow.DefinirAlvo(cameraTarget);
                Debug.Log("CameraFollow configurado com CameraTarget.");
            }
            else
            {
                Debug.LogWarning("CameraTarget não encontrado dentro do prefab do player.");
                cameraFollow.DefinirAlvo(playerInstanciado.transform);
            }
        }
        else
        {
            Debug.LogWarning("CameraFollow não encontrado na cena!");
        }

        // Configura o LightManager
        if (LightManager.instancia != null)
        {
            LightManager.instancia.DefinirPlayer(playerInstanciado.transform);
        }
        else
        {
            Debug.LogWarning("LightManager não encontrado na cena!");
        }

        // Configura o ParticleManager
        if (ParticleManager.instancia != null)
        {
            ParticleManager.instancia.DefinirPlayer(playerInstanciado.transform);
        }
        else
        {
            Debug.LogWarning("ParticleManager não encontrado na cena!");
        }

        // Registra o player no GameManager
        if (GameManager.instancia != null)
        {
            GameManager.instancia.DefinirPlayer(playerInstanciado);
        }
    }

    private Vector3 CalcularOffsetDoPlayer(GameObject prefab)
    {
        Collider col = prefab.GetComponentInChildren<Collider>();

        if (col != null)
        {
            Bounds bounds = col.bounds;

            Vector3 centroXZ = new Vector3(bounds.center.x, 0, bounds.center.z);
            float altura = bounds.extents.y;

            Vector3 offset = -centroXZ - new Vector3(0, altura, 0);

            Debug.Log($"Offset calculado para player: {offset}");

            return offset;
        }
        else
        {
            Debug.LogWarning("O prefab do player não tem Collider. Usando offset padrão.");
            return Vector3.zero;
        }
    }

    public void LimparMapa()
    {
        foreach (Transform filho in paiDasSalas)
        {
            Destroy(filho.gameObject);
        }
        mapaGerado.Clear();
    }
}
