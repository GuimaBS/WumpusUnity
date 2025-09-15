using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Agent3PerfectRunner : MonoBehaviour
{
    [Header("Refer�ncias")]
    public GridGenerator grid;                 // opcional: encontra sozinho
    public GameObject agente3Prefab;           // prefab do Agente 3
    public Transform spawnPai;                 // opcional

    [Header("Movimenta��o")]
    public float moveSpeed = 6f;
    public float rotateSpeed = 720f;
    public float stepPause = 0.02f;

    [Header("Pontua��o (regras do Lab)")]
    public int score { get; private set; } = 0;
    public int passosTotais { get; private set; } = 0;
    public int mortes { get; private set; } = 0;

    [Header("Eventos p/ painel de LOG")]
    public UnityEvent<string> OnLog;

    [Header("Op��es de Log")]
    public bool logNoConsole = true;
    public bool logPercepcao = true;
    public bool logMovimento = true;
    public bool logResumoFinal = true;

    [Header("Prefab: seguran�a")]
    [Tooltip("Desativa todos os MonoBehaviours do prefab instanciado (exceto componentes visuais) para evitar IAs/Updates paralelos.")]
    public bool desativarScriptsDoPrefab = true;

    [Tooltip("Faz o Rigidbody (se houver) ficar kinematic e sem gravidade.")]
    public bool travarRigidbody = true;

    [Header("Eventos de ciclo")]
    public UnityEvent OnFinished;              // dispara quando termina

    // Estado
    private GameObject agenteGO;
    private Transform agente;
    private Vector2Int pos; // grid
    private bool todosWumpusEliminados = false;
    private bool ouroJaColetado = false;
    private bool _executando = false;
    private bool _finalizado = false;

    // Auxiliares
    private float CellSize => GridGenerator.instancia != null ? GridGenerator.instancia.tileSize : 1.7f;
    private int MaxX => GridGenerator.tamanhoX;
    private int MaxY => GridGenerator.tamanhoY;

    // ===== Ciclo de vida =====
    private void OnDisable() { _executando = false; StopAllCoroutines(); }
    private void OnDestroy() { _executando = false; StopAllCoroutines(); }

    // ===== API =====
    public void RunBenchmark()
    {
        if (_executando) { Emit("[Coringa] J� est� em execu��o."); return; }
#if UNITY_2023_1_OR_NEWER
        if (grid == null) grid = Object.FindFirstObjectByType<GridGenerator>(FindObjectsInactive.Exclude);
#else
        if (grid == null) grid = FindObjectOfType<GridGenerator>();
#endif
        _finalizado = false;           // <<<<<< CORRE��O (antes estava true)
        _executando = true;
        StopAllCoroutines();
        StartCoroutine(ExecBenchmark());
    }

    private void ResetarEstado()
    {
        score = 0;
        passosTotais = 0;
        mortes = 0;
        todosWumpusEliminados = false;
        ouroJaColetado = GridGenerator.ouroColetado; // normalmente false

        if (agenteGO != null) { Destroy(agenteGO); agenteGO = null; }
        agente = null; // evita acessar Transform destru�do
    }

    private IEnumerator ExecBenchmark()
    {
        if (GridGenerator.instancia == null)
        {
            Emit("[Coringa] GridGenerator.instancia n�o encontrado.");
            _executando = false;
            yield break;
        }

        ResetarEstado();

        // 1) Spawn em (0,0)
        Vector2Int start = new Vector2Int(0, 0);
        Vector3 worldStart = ParaMundo(start);
        agenteGO = Instantiate(agente3Prefab, worldStart, Quaternion.identity, spawnPai);

        // Aplicar travas do prefab (IA externas e f�sica)
        if (desativarScriptsDoPrefab) DesativarScriptsDoPrefab(agenteGO);
        if (travarRigidbody)
        {
            foreach (var rb in agenteGO.GetComponentsInChildren<Rigidbody>())
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        agente = agenteGO.transform;
        agente.position = worldStart; // for�a posi��o precisa
        pos = start;

        Emit("=== [Coringa] In�cio da execu��o perfeita ===");
        Emit($"Spawn em {pos}. Score={score}");
        LogPercepcoes(pos);

        // 2) Eliminar TODOS os Wumpus
        yield return StartCoroutine(EliminarTodosWumpus());

        // 3) Ir at� o ouro e coletar (se ainda existir)
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
            Emit("[Coleta] Ouro j� estava coletado.");
        }

        // 4) Voltar � casa (0,0)
        yield return StartCoroutine(CaminharSeguro(pos, start));

        // 5) Vit�ria + b�nus
        if (todosWumpusEliminados && ouroJaColetado && pos == start)
        {
            score += 2000;
            Emit($"[Vit�ria] Retorno � {start} com Wumpus morto(s) e ouro coletado. B�nus aplicado. Score={score}");
        }

        if (logResumoFinal)
            Emit($"=== [Coringa] Fim === Passos={passosTotais}, Mortes={mortes}, Score final={score} ===");

        // Encerramento forte
        _finalizado = true;            // <<<<<< marca fim para travar descontos
        _executando = false;
        OnFinished?.Invoke();
        StopAllCoroutines();
        yield break;
    } // <<< fecha ExecBenchmark()

    // ===== Wumpus =====
    private IEnumerator EliminarTodosWumpus()
    {
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
            if (path != null) { alvoAdj = a; break; }
        }

        if (alvoAdj == null)
        {
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

    private IEnumerator AtirarNoWumpus(Vector2Int wPos, Vector2Int shooter)
    {
        if (!GridGenerator.posicoesWumpus.Contains(wPos))
        {
            Emit($"[A��o] Wumpus em {wPos} j� n�o est� presente.");
            yield break;
        }

        Vector2Int dir = DirecaoCardinal(wPos - shooter);
        yield return StartCoroutine(RotacionarAteDir(dir));

        Emit($"[A��o] Tiro do {shooter} em dire��o ao Wumpus {wPos}.");
        GridGenerator.EliminarWumpusNaPosicao(wPos);
        score += 1000;
        Emit($"[Resultado] Wumpus eliminado em {wPos}. Score={score}");

        LogPercepcoes(pos);
        yield return null;
    }

    private Vector2Int? EncontrarPontoLOS(Vector2Int from, Vector2Int target)
    {
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

    // ===== Caminhada segura + pontua��o/log =====
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
            if (agente == null || _finalizado) yield break; // destru�do ou j� finalizado

            Vector2Int prox = path[i];
            Vector3 destino = ParaMundo(prox);

            if (logMovimento) Emit($"[Mov] {pos} -> {prox}");

            Vector3 dir = (destino - agente.position);
            yield return StartCoroutine(RotacionarAte(dir));
            if (agente == null || _finalizado) yield break;

            yield return StartCoroutine(MoverAte(destino));
            if (agente == null || _finalizado) yield break;

            // conta passo s� se mudou de tile
            Vector2Int posAntes = pos;
            pos = prox;
            if (pos != posAntes)
            {
                passosTotais++;
                DescontarPassoSeAtivo();   // <<<<<< usa trava global
            }

            if (EhPoco(pos))
            {
                mortes++;
                score -= 1000;
                Emit($"[Morte] Caiu em po�o em {pos}. Score={score}");
            }

            LogPercepcoes(pos);
            yield return new WaitForSeconds(stepPause);
        }
    }

    private void DescontarPassoSeAtivo()
    {
        if (!_executando || _finalizado) return;
        score -= 1;
    }

    // ===== Percep��es =====
    private void LogPercepcoes(Vector2Int p)
    {
        if (!logPercepcao) return;

        var info = TileManager.instancia?.ObterInfoDaTile(p);
        bool emPoco = info != null && info.temPoco;
        bool temBrisa = info != null && info.temBrisa;
        bool temFedor = info != null && info.temFedor;
        bool temBrilho = (!GridGenerator.ouroColetado && p == GridGenerator.posicaoOuro);

        var feats = new List<string>();
        if (emPoco) feats.Add("PO�O(!)");
        if (temBrisa) feats.Add("brisa");
        if (temFedor) feats.Add("fedor");
        if (temBrilho) feats.Add("brilho");

        if (feats.Count == 0) Emit($"[Percep��o] {p}: (sem sensa��es)");
        else Emit($"[Percep��o] {p}: " + string.Join(", ", feats));
    }

    // ===== Utilidades de grid =====
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

    // ===== Movimento/Rota��o =====
    private IEnumerator MoverAte(Vector3 worldTarget)
    {
        while (true)
        {
            if (agente == null || _finalizado) yield break;

            Vector3 posAtual = agente.position;
            if ((posAtual - worldTarget).sqrMagnitude <= 0.0004f)
                break;

            agente.position = Vector3.MoveTowards(posAtual, worldTarget, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    private IEnumerator RotacionarAte(Vector3 worldDir)
    {
        if (agente == null || _finalizado) yield break;
        worldDir.y = 0f;
        if (worldDir.sqrMagnitude < 1e-6f) yield break;

        Quaternion targetRot = Quaternion.LookRotation(worldDir.normalized, Vector3.up);
        while (true)
        {
            if (agente == null || _finalizado) yield break;

            if (Quaternion.Angle(agente.rotation, targetRot) <= 0.5f)
                break;

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

    // ===== A* =====
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

    private void DesativarScriptsDoPrefab(GameObject root)
    {
        // Desativa todos os MonoBehaviours, exceto componentes �visuais/comuns�
        var behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var b in behaviours)
        {
            if (b == null) continue;
            if (b is Animator) continue;  // mantenha anima��es
            b.enabled = false;
        }
    }
}
