using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class Agent3PerfectRunner : MonoBehaviour
{
    [Header("Referências")]
    public GridGenerator grid;                 // arraste o GridGenerator da cena (opcional)
    public GameObject agente3Prefab;           // prefab do Agente 3
    public Transform spawnPai;                 // opcional: parent

    [Header("Movimentação")]
    public float moveSpeed = 6f;
    public float rotateSpeed = 720f;
    public float stepPause = 0.02f;

    [Header("Pontuação (regras do Lab)")]
    public int score { get; private set; } = 0;
    public int passosTotais { get; private set; } = 0;
    public int mortes { get; private set; } = 0;

    [Header("Eventos p/ painel de LOG")]
    public UnityEvent<string> OnLog;

    [Header("Opções de Log")]
    public bool logNoConsole = true;
    public bool logPercepcao = true;
    public bool logMovimento = true;
    public bool logResumoFinal = true;

    // Estado
    private GameObject agenteGO;
    private Transform agente;
    private Vector2Int pos; // grid
    private bool todosWumpusEliminados = false;
    private bool ouroJaColetado = false;

    private bool _executando = false;

    [Header("Eventos de ciclo")]
    public UnityEvent OnFinished; 

    // Auxiliares
    private float CellSize => GridGenerator.instancia != null ? GridGenerator.instancia.tileSize : 1.7f;
    private int MaxX => GridGenerator.tamanhoX;
    private int MaxY => GridGenerator.tamanhoY;

    // ----- API -----
    public void RunBenchmark()
    {
        if (grid == null) grid = FindObjectOfType<GridGenerator>();
        StopAllCoroutines();
        StartCoroutine(ExecBenchmark());
    }

    private void ResetarEstado()
    {
        score = 0;
        passosTotais = 0;
        mortes = 0;
        todosWumpusEliminados = false;
        ouroJaColetado = GridGenerator.ouroColetado; // caso mapa venha com ouro já coletado (não deveria)
        if (agenteGO != null) Destroy(agenteGO);
    }

    private IEnumerator ExecBenchmark()
    {
        if (GridGenerator.instancia == null)
        {
            Emit("[Coringa] GridGenerator.instancia não encontrado.");
            yield break;
        }

        ResetarEstado();

        // 1) Spawn em (0,0)
        Vector2Int start = new Vector2Int(0, 0);
        Vector3 worldStart = ParaMundo(start);
        agenteGO = Instantiate(agente3Prefab, worldStart, Quaternion.identity, spawnPai);
        agente = agenteGO.transform;
        pos = start;

        Emit("=== [Coringa] Início da execução perfeita ===");
        Emit($"Spawn em {pos}. Score={score}");
        LogPercepcoes(pos);

        // 2) Eliminar TODOS os Wumpus (lista pode ter 1 ou 2)
        yield return StartCoroutine(EliminarTodosWumpus());

        // 3) Ir até o ouro e coletar (se ainda existir)
        if (!GridGenerator.ouroColetado)
        {
            Vector2Int oPos = GridGenerator.posicaoOuro;
            yield return StartCoroutine(CaminharSeguro(pos, oPos));
            GridGenerator.ColetarOuroNaPosicao(oPos);
            ouroJaColetado = true;
            score += 1000;
            Emit($"[Coleta] Ouro coletado em {oPos}. Score={score}");
            LogPercepcoes(pos);
        }
        else
        {
            ouroJaColetado = true;
            Emit("[Coleta] Ouro já estava coletado.");
        }

        // 4) Voltar à casa (0,0)
        yield return StartCoroutine(CaminharSeguro(pos, start));

        // 5) Vitória + bônus
        if (todosWumpusEliminados && ouroJaColetado && pos == start)
        {
            score += 2000;
            Emit($"[Vitória] Retorno à {start} com Wumpus morto(s) e ouro coletado. Bônus aplicado. Score={score}");
        }

        if (logResumoFinal)
        {
            Emit($"=== [Coringa] Fim === Passos={passosTotais}, Mortes={mortes}, Score final={score} ===");
        }
    }

    // ----- Wumpus -----
    private IEnumerator EliminarTodosWumpus()
    {
        // Enquanto houver wumpus na lista, elimina o mais próximo do ponto atual
        while (GridGenerator.posicoesWumpus.Count > 0)
        {
            Vector2Int alvo = GridGenerator.posicoesWumpus
                .OrderBy(pw => Manhattan(pw, pos))
                .First();

            yield return StartCoroutine(EliminarWumpusComSeguranca(alvo));
        }
        todosWumpusEliminados = true;
        Emit("[Resultado] Todos os Wumpus foram eliminados.");
    }

    private IEnumerator EliminarWumpusComSeguranca(Vector2Int wPos)
    {
        // Tenta ficar adjacente (cardinal) ao Wumpus por rota segura (sem poços) e atirar
        Vector2Int[] adj = new[]
        {
            wPos + Vector2Int.up,
            wPos + Vector2Int.down,
            wPos + Vector2Int.left,
            wPos + Vector2Int.right
        };

        Vector2Int? alvoAdj = null;
        foreach (var a in adj)
        {
            if (!EstaDentro(a)) continue;
            if (EhPoco(a)) continue;

            var path = AStar(pos, a, IsWalkable);
            if (path != null)
            {
                alvoAdj = a;
                break;
            }
        }

        if (alvoAdj == null)
        {
            // fallback: procura um ponto LOS adjacente ao Wumpus alcançável
            var pontoLOS = EncontrarPontoLOS(pos, wPos);
            if (pontoLOS == null)
            {
                Emit($"[Coringa] Falha ao encontrar LOS seguro para o Wumpus em {wPos}.");
                yield break;
            }
            yield return StartCoroutine(CaminharSeguro(pos, pontoLOS.Value));
            yield return StartCoroutine(AtirarNoWumpus(wPos, pontoLOS.Value));
        }
        else
        {
            yield return StartCoroutine(CaminharSeguro(pos, alvoAdj.Value));
            yield return StartCoroutine(AtirarNoWumpus(wPos, alvoAdj.Value));
        }
    }

    public void RunBenchmark()
{
    if (_executando) { Emit("[Coringa] Já está em execução."); return; }
    if (grid == null) grid = FindObjectOfType<GridGenerator>();
    _executando = true;                 // <— marca como rodando
    StopAllCoroutines();                // saneia qualquer resto antigo
    StartCoroutine(ExecBenchmark());
}


    private IEnumerator AtirarNoWumpus(Vector2Int wPos, Vector2Int shooter)
    {
        if (!GridGenerator.posicoesWumpus.Contains(wPos))
        {
            Emit($"[Ação] Wumpus em {wPos} já não está presente (possível duplicidade).");
            yield break;
        }

        Vector2Int dir = DirecaoCardinal(wPos - shooter);
        yield return StartCoroutine(RotacionarAteDir(dir));

        Emit($"[Ação] Tiro do {shooter} em direção ao Wumpus {wPos}.");
        GridGenerator.EliminarWumpusNaPosicao(wPos); // método estático
        score += 1000;
        Emit($"[Resultado] Wumpus eliminado em {wPos}. Score={score}");

        // Atualiza percepções no tile atual
        LogPercepcoes(pos);
        yield return null;
    }

    if (logResumoFinal)
    Emit($"=== [Coringa] Fim === Passos={passosTotais}, Mortes={mortes}, Score final={score} ===");

// Sinaliza término e corta tudo
_executando = false;
OnFinished?.Invoke();
StopAllCoroutines();   // garante que nada continua rodando
yield break;


    private Vector2Int? EncontrarPontoLOS(Vector2Int from, Vector2Int target)
    {
        // Prioriza os adjacentes imediatos ao Wumpus
        Vector2Int[] cand = new[]
        {
            target + Vector2Int.up, target + Vector2Int.down,
            target + Vector2Int.left, target + Vector2Int.right
        };
        foreach (var c in cand)
        {
            if (!EstaDentro(c)) continue;
            if (EhPoco(c)) continue;
            var path = AStar(from, c, IsWalkable);
            if (path != null) return c;
        }
        return null;
    }

    // ----- Caminhada segura + pontuação/log -----
    private IEnumerator CaminharSeguro(Vector2Int de, Vector2Int ate)
    {
        var path = AStar(de, ate, IsWalkable);
        if (path == null)
        {
            Emit($"[Rota] Sem caminho seguro de {de} para {ate}.");
            yield break;
        }

        for (int i = 1; i < path.Count; i++)
        {
            Vector2Int prox = path[i];
            Vector3 destino = ParaMundo(prox);

            if (logMovimento) Emit($"[Mov] {pos} -> {prox}");

            // Rotação e deslocamento
            Vector3 dir = (destino - agente.position);
            yield return StartCoroutine(RotacionarAte(dir));
            yield return StartCoroutine(MoverAte(destino));

            pos = prox;

            // Pontuação por passo
            passosTotais++;
            score -= 1;

            // Segurança extra (não deveria ocorrer no coringa)
            if (EhPoco(pos))
            {
                mortes++;
                score -= 1000;
                Emit($"[Morte] Caiu em poço em {pos}. Score={score}");
            }

            LogPercepcoes(pos);
            yield return new WaitForSeconds(stepPause);
        }
    }

    // ----- Percepções -----
    private void LogPercepcoes(Vector2Int p)
    {
        if (!logPercepcao) return;

        var info = TileManager.instancia?.ObterInfoDaTile(p);
        bool emPoco = info != null && info.temPoco;
        bool temBrisa = info != null && info.temBrisa;
        bool temFedor = info != null && info.temFedor;
        bool temBrilho = (!GridGenerator.ouroColetado && p == GridGenerator.posicaoOuro);

        var feats = new List<string>();
        if (emPoco) feats.Add("POÇO(!)");
        if (temBrisa) feats.Add("brisa");
        if (temFedor) feats.Add("fedor");
        if (temBrilho) feats.Add("brilho");

        if (feats.Count == 0) Emit($"[Percepção] {p}: (sem sensações)");
        else Emit($"[Percepção] {p}: " + string.Join(", ", feats));
    }

    // ----- Utilidades de grid -----
    private bool EstaDentro(Vector2Int p) =>
        p.x >= 0 && p.x < MaxX && p.y >= 0 && p.y < MaxY;

    private bool EhPoco(Vector2Int p)
    {
        if (!EstaDentro(p)) return false;
        var info = TileManager.instancia?.ObterInfoDaTile(p);
        return info != null && info.temPoco;
    }

    private bool IsWalkable(Vector2Int p) => EstaDentro(p) && !EhPoco(p);

    private Vector3 ParaMundo(Vector2Int gridPos) =>
        new Vector3(gridPos.x * CellSize, 0f, gridPos.y * CellSize);

    private int Manhattan(Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    private Vector2Int DirecaoCardinal(Vector2Int delta)
    {
        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            return new Vector2Int(delta.x < 0 ? -1 : 1, 0);
        return new Vector2Int(0, delta.y < 0 ? -1 : 1);
    }

    // ----- Movimento/Rotação -----
    private IEnumerator MoverAte(Vector3 worldTarget)
    {
        while ((agente.position - worldTarget).sqrMagnitude > 0.0004f)
        {
            agente.position = Vector3.MoveTowards(agente.position, worldTarget, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator RotacionarAte(Vector3 worldDir)
    {
        worldDir.y = 0f;
        if (worldDir.sqrMagnitude < 1e-6f) yield break;
        Quaternion targetRot = Quaternion.LookRotation(worldDir.normalized, Vector3.up);
        while (Quaternion.Angle(agente.rotation, targetRot) > 0.5f)
        {
            agente.rotation = Quaternion.RotateTowards(agente.rotation, targetRot, rotateSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator RotacionarAteDir(Vector2Int dir)
    {
        if (dir == Vector2Int.zero) yield break;
        Vector3 worldDir = new Vector3(dir.x, 0f, dir.y);
        yield return StartCoroutine(RotacionarAte(worldDir));
    }

    private void Emit(string msg)
    {
        if (logNoConsole) Debug.Log(msg);
        OnLog?.Invoke(msg);
    }

    // ----- A* -----
    private static readonly Vector2Int[] cardinais = new[]
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    private List<Vector2Int> AStar(Vector2Int start, Vector2Int goal, System.Func<Vector2Int, bool> walkable)
    {
        var open = new PriorityQueue<Vector2Int>();
        var came = new Dictionary<Vector2Int, Vector2Int>();
        var g = new Dictionary<Vector2Int, int>();
        var f = new Dictionary<Vector2Int, int>();

        open.Enqueue(start, 0);
        g[start] = 0;
        f[start] = Heu(start, goal);

        while (open.Count > 0)
        {
            var current = open.Dequeue();
            if (current == goal)
                return Reconstruir(came, current, start);

            foreach (var d in cardinais)
            {
                var nb = current + d;
                if (!walkable(nb)) continue;
                int tentative = g[current] + 1;
                if (!g.ContainsKey(nb) || tentative < g[nb])
                {
                    came[nb] = current;
                    g[nb] = tentative;
                    int ff = tentative + Heu(nb, goal);
                    f[nb] = ff;
                    open.Enqueue(nb, ff);
                }
            }
        }
        return null;
    }

    private int Heu(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    private List<Vector2Int> Reconstruir(Dictionary<Vector2Int, Vector2Int> came, Vector2Int current, Vector2Int start)
    {
        var path = new List<Vector2Int> { current };
        while (came.ContainsKey(current))
        {
            current = came[current];
            path.Add(current);
        }
        path.Reverse();
        if (path.Count == 0 || path[0] != start) path.Insert(0, start);
        return path;
    }

    private class PriorityQueue<T>
    {
        private readonly List<(T item, int prio)> data = new();
        public int Count => data.Count;

        public void Enqueue(T item, int priority) => data.Add((item, priority));
        public T Dequeue()
        {
            int best = 0;
            for (int i = 1; i < data.Count; i++)
                if (data[i].prio < data[best].prio) best = i;
            var it = data[best].item;
            data.RemoveAt(best);
            return it;
        }
    }
}
