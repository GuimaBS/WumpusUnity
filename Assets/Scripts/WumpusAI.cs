using System.Collections;
using UnityEngine;

public class WumpusAI : MonoBehaviour
{
    [Header("Setup")]
    public float moveDistance = 10f;                // igual ao player (espac¸o entre salas)
    public float moveSpeed = 20f;                   // igual ao player
    public float rotationSpeed = 400f;              // igual ao player
    public bool caçadaAtiva = false;               // setado pelo WumpusHuntController
    public Transform visual;                       // opcional: para girar o visual

    private PlayerGridGenerator grid;
    private Transform player;
    private Vector3 targetPos;
    private Quaternion targetRot;
    private bool isMoving = false;
    private Vector2Int posAtual;                   // pos do wumpus no grid

    private void Awake()
    {
        grid = PlayerGridGenerator.instancia;
    }

    private void OnEnable()
    {
        PlayerMovement.OnPlayerStepped += NoPassoDoPlayer;
    }

    private void OnDisable()
    {
        PlayerMovement.OnPlayerStepped -= NoPassoDoPlayer;
    }

    private void Start()
    {
        if (grid == null) grid = PlayerGridGenerator.instancia;
        var p = FindFirstObjectByType<PlayerMovement>();
        if (p != null) player = p.transform;

        // calcula pos inicial do wumpus baseada na posição atual do objeto
        posAtual = grid.ConverterPosicaoMundoParaGrid(transform.position);

        // garante alinhamento exato ao centro da sala
        targetPos = grid.mapaGerado[posAtual].transform.position + grid.offsetCentroSala;
        transform.position = targetPos;
        targetRot = transform.rotation;

        // moveDistance costuma ser igual a espacoEntreSalas
        moveDistance = grid.espacoEntreSalas;
    }

    private void Update()
    {
        // interpola movimento/rotação suave
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }

    private void NoPassoDoPlayer(Vector2Int posPlayer)
    {
        if (!caçadaAtiva) return;
        if (grid == null || player == null) return;
        if (isMoving) return; // 1 passo por passo

        // Decide um passo que reduza a distância Manhattan até o player.
        Vector2Int melhorPasso = DecidirPasso(posAtual, posPlayer);

        if (melhorPasso == Vector2Int.zero) return; // sem movimento possível

        Vector2Int destino = posAtual + melhorPasso;
        if (!grid.gridInfo.ContainsKey(destino)) return; // fora do mapa

        // Rotaciona para a direção escolhida
        Vector3 dir = new Vector3(melhorPasso.x, 0f, melhorPasso.y);
        if (dir.sqrMagnitude > 0.1f)
            targetRot = Quaternion.LookRotation(dir);

        // Solicita ao Grid mover o wumpus (atualiza fedor e flags)
        grid.MoverWumpus(posAtual, destino);
        posAtual = destino;

        // Define target de posição
        targetPos = grid.mapaGerado[posAtual].transform.position + grid.offsetCentroSala;

        // Anima/flag
        StartCoroutine(MarcarMovimentoCurto());
    }

    private IEnumerator MarcarMovimentoCurto()
    {
        isMoving = true;
        // espera chegar no tile (opcionalmente use um tempo fixo curto se preferir)
        while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            yield return null;

        isMoving = false;
    }

    // Tenta reduzir a distância Manhattan; preferindo eixo com maior diferença.
    private Vector2Int DecidirPasso(Vector2Int de, Vector2Int alvo)
    {
        Vector2Int delta = alvo - de;
        int ax = Mathf.Abs(delta.x);
        int ay = Mathf.Abs(delta.y);

        // ordem de tentativas: maior eixo primeiro, depois o outro
        if (ax >= ay)
        {
            Vector2Int primeira = new Vector2Int(Mathf.Clamp(delta.x, -1, 1), 0);
            if (PodeIr(de + primeira)) return primeira;

            Vector2Int segunda = new Vector2Int(0, Mathf.Clamp(delta.y, -1, 1));
            if (PodeIr(de + segunda)) return segunda;
        }
        else
        {
            Vector2Int primeira = new Vector2Int(0, Mathf.Clamp(delta.y, -1, 1));
            if (PodeIr(de + primeira)) return primeira;

            Vector2Int segunda = new Vector2Int(Mathf.Clamp(delta.x, -1, 1), 0);
            if (PodeIr(de + segunda)) return segunda;
        }

        // fallback: tenta as outras duas direções ortogonais (caso a primeira esteja bloqueada por borda)
        Vector2Int[] alternativas = new Vector2Int[]
        {
            new Vector2Int( 1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int( 0, 1),
            new Vector2Int( 0,-1),
        };
        foreach (var a in alternativas)
            if (PodeIr(de + a)) return a;

        return Vector2Int.zero;
    }

    private bool PodeIr(Vector2Int destino)
    {
        // Wumpus ignora poços (não morre), mas respeita limites do mapa
        return grid.gridInfo.ContainsKey(destino);
    }

    // Permite o controlador ligar/desligar a caçada
    public void SetCaçada(bool ativa) => caçadaAtiva = ativa;
}
