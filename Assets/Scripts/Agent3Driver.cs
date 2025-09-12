using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Agent3Brain))]
public class Agent3Driver : MonoBehaviour
{
    [Header("Movimento")]
    public float stepDelay = 0.15f;
    public float cellSize = 1.7f;

    [Header("Altura / Chão")]
    [SerializeField] float groundY = 0f;
    [SerializeField] bool useRaycastGround = false;
    [SerializeField] LayerMask groundMask = ~0;
    [SerializeField] float yOffset = 0f;

    [Header("Offset de centragem no tile (opcional)")]
    [SerializeField] Vector2 worldOffset = Vector2.zero; // X/Z

    private Agent3Brain brain;
    private bool missionComplete = false;
    private bool victoryRewardGiven = false;

    void Awake()
    {
        brain = GetComponent<Agent3Brain>();
    }

    void Start()
    {
        if (useRaycastGround)
        {
            if (Physics.Raycast(new Vector3(0, 100f, 0), Vector3.down, out var hit, 300f, groundMask))
                groundY = hit.point.y + yOffset;
        }
        StopAllCoroutines();
        StartCoroutine(Run());
    }

    Vector3 GridToWorld(Vector2Int p)
        => new Vector3(p.x * cellSize + worldOffset.x, groundY, p.y * cellSize + worldOffset.y);

    IEnumerator Run()
    {
        int W = Mathf.Max(0, GridGenerator.tamanhoX);
        int H = Mathf.Max(0, GridGenerator.tamanhoY);
        if (W <= 0 || H <= 0)
        {
            Debug.LogError("[Agent3Driver] tamanhoX/Y inválidos. Abortando.");
            yield break;
        }

        Vector2Int pos = new(0, 0);
        transform.position = GridToWorld(pos);

        while (true)
        {
            if (missionComplete)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            var info = TileManager.instancia?.ObterInfoDaTile(pos);
            bool brisa = info != null && info.temBrisa;
            bool fedor = info != null && info.temFedor;
            bool brilho = info != null && info.temOuro && !GridGenerator.ouroColetado;

            var obs = new Agent3GA.Observation
            {
                ouro = info != null && info.temOuro && !GridGenerator.ouroColetado,

                fU = GridGenerator.posicoesWumpus != null && GridGenerator.posicoesWumpus.Contains(pos + Vector2Int.up),
                fD = GridGenerator.posicoesWumpus != null && GridGenerator.posicoesWumpus.Contains(pos + Vector2Int.down),
                fL = GridGenerator.posicoesWumpus != null && GridGenerator.posicoesWumpus.Contains(pos + Vector2Int.left),
                fR = GridGenerator.posicoesWumpus != null && GridGenerator.posicoesWumpus.Contains(pos + Vector2Int.right),

                bU = TileManager.instancia && TileManager.instancia.PocoNaPosicao(pos + Vector2Int.up),
                bD = TileManager.instancia && TileManager.instancia.PocoNaPosicao(pos + Vector2Int.down),
                bL = TileManager.instancia && TileManager.instancia.PocoNaPosicao(pos + Vector2Int.left),
                bR = TileManager.instancia && TileManager.instancia.PocoNaPosicao(pos + Vector2Int.right),

                canUp = pos.y + 1 < H,
                canDown = pos.y - 1 >= 0,
                canLeft = pos.x - 1 >= 0,
                canRight = pos.x + 1 < W,

                goalReturn = ((GridGenerator.posicoesWumpus == null || GridGenerator.posicoesWumpus.Count == 0)
                   && GridGenerator.ouroColetado)
            };

            // >>> decisão vem do Brain com os pesos do campeão
            var action = brain.Decide(obs);

            Vector2Int delta = Vector2Int.zero;
            switch (action)
            {
                case Agent3GA.Agent3Action.MoveUp: delta = Vector2Int.up; break;
                case Agent3GA.Agent3Action.MoveDown: delta = Vector2Int.down; break;
                case Agent3GA.Agent3Action.MoveLeft: delta = Vector2Int.left; break;
                case Agent3GA.Agent3Action.MoveRight: delta = Vector2Int.right; break;

                case Agent3GA.Agent3Action.ShootUp: TryShoot(pos + Vector2Int.up); break;
                case Agent3GA.Agent3Action.ShootDown: TryShoot(pos + Vector2Int.down); break;
                case Agent3GA.Agent3Action.ShootLeft: TryShoot(pos + Vector2Int.left); break;
                case Agent3GA.Agent3Action.ShootRight: TryShoot(pos + Vector2Int.right); break;

                case Agent3GA.Agent3Action.Collect: TryCollect(pos); break;
            }

            if (delta != Vector2Int.zero)
            {
                var alvo = pos + delta;
                if (alvo.x >= 0 && alvo.x < W && alvo.y >= 0 && alvo.y < H)
                {
                    pos = alvo;
                    transform.position = GridToWorld(pos); // fixa Y
                }
                PontuacaoManager.instancia?.AlterarPontuacao(-1);
            }

            // morte por poço/wumpus
            bool caiuEmPoco = TileManager.instancia && TileManager.instancia.PocoNaPosicao(pos);
            bool tocouWumpus = GridGenerator.posicoesWumpus != null && GridGenerator.posicoesWumpus.Contains(pos);
            if (caiuEmPoco || tocouWumpus)
            {
                PontuacaoManager.instancia?.AlterarPontuacao(-1000);
                Destroy(gameObject);
                yield break;
            }

            // vitória: matar wumpus + coletar ouro + voltar à (0,0)
            bool wumpusVivo = GridGenerator.posicoesWumpus != null && GridGenerator.posicoesWumpus.Count > 0;
            if (!wumpusVivo && GridGenerator.ouroColetado && pos == Vector2Int.zero)
            {
                if (!victoryRewardGiven)
                {
                    PontuacaoManager.instancia?.AlterarPontuacao(+2000);
                    victoryRewardGiven = true;
                }
                missionComplete = true;
            }

            yield return new WaitForSeconds(stepDelay);
        }
    }

    void TryShoot(Vector2Int alvo)
    {
        if (GridGenerator.posicoesWumpus != null && GridGenerator.posicoesWumpus.Contains(alvo))
        {
            GridGenerator.EliminarWumpusNaPosicao(alvo);
            PontuacaoManager.instancia?.AlterarPontuacao(+1000);
        }
        else
        {
            PontuacaoManager.instancia?.AlterarPontuacao(-500);
        }
    }

    void TryCollect(Vector2Int p)
    {
        var info = TileManager.instancia?.ObterInfoDaTile(p);
        if (info != null && info.temOuro && !GridGenerator.ouroColetado)
        {
            GridGenerator.ColetarOuroNaPosicao(p);
            PontuacaoManager.instancia?.AlterarPontuacao(+1000);
        }
        else
        {
            PontuacaoManager.instancia?.AlterarPontuacao(-500);
        }
    }
}
