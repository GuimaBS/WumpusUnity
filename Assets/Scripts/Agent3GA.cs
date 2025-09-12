// Assets/Scripts/Agent3GA.cs
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Agent3GA : MonoBehaviour
{
    public static Agent3GA instancia;

    [Header("GA Params")]
    [SerializeField] public int populationSize = 30;        // máx 50
    [SerializeField] public int stepBudgetPerEval = 200;    // orçamento de passos
    [SerializeField] public float crossoverRate = 0.85f;    // 85%
    [SerializeField] public float mutationRate = 0.05f;     // 5%
    [SerializeField] public float mutationStd = 0.15f;
    [SerializeField] public int elitism = 1;
    [SerializeField] public int tournamentK = 3;
    [SerializeField] public int evalSeed = 0;               // reprodutibilidade opcional

    public int generation { get; private set; }
    public float lastAvgFitness { get; private set; }
    public float bestFitness { get; private set; } = float.NegativeInfinity;
    public Genome bestGenome { get; private set; }
    public Genome bestGenomeThisGen { get; private set; }

    public Agent3ChampionSO championSO;

    private List<Genome> population;
    private System.Random evalRng;

    // melhor fitness DENTRO da geração atual (para ranking por geração)
    private float lastBestGen = float.NegativeInfinity;

    public struct GenStats
    {
        public int gen;
        public float avg;        // média da geração
        public float bestSoFar;  // melhor global até aqui
        public float bestGen;    // melhor dentro desta geração
        public float cx;
        public float mut;
    }
    public event Action<GenStats> OnGenerationAdvanced;

    void Awake()
    {
        if (instancia && instancia != this) { Destroy(gameObject); return; }
        instancia = this;
        DontDestroyOnLoad(gameObject);
    }

    // ===== API =====
    public void RunGenerations(int gens)
    {
        EnsurePopulation();
        EvaluatePopulation();   // avalia estado inicial
        Emit();                 // emite geração 0

        for (int g = 0; g < gens; g++)
        {
            var next = new List<Genome>(populationSize);

            // elitismo
            var ordered = new List<Genome>(population);
            ordered.Sort((a, b) => b.fitness.CompareTo(a.fitness));
            for (int e = 0; e < Mathf.Min(elitism, ordered.Count); e++)
                next.Add(ordered[e].Clone());

            // reprodução
            while (next.Count < populationSize)
            {
                var p1 = Tournament();
                var p2 = Tournament();

                Genome child = (UnityEngine.Random.value < crossoverRate)
                    ? Genome.Crossover(p1, p2)
                    : p1.Clone();

                child.Mutate(mutationRate, mutationStd);
                next.Add(child);
            }

            population = next;
            EvaluatePopulation();
            generation++;
            Emit();
        }
    }

    // 1 geração utilizando apenas os 2 melhores como pais
    public void RunOneGenerationTop2()
    {
        EnsurePopulation();
        EvaluatePopulation();

        var ordered = new List<Genome>(population);
        ordered.Sort((a, b) => b.fitness.CompareTo(a.fitness));
        var top1 = ordered[0];
        var top2 = ordered[Mathf.Min(1, ordered.Count - 1)];

        var next = new List<Genome>(populationSize);

        // elitismo
        for (int e = 0; e < Mathf.Min(elitism, ordered.Count); e++)
            next.Add(ordered[e].Clone());

        // cruzamentos com top1/top2
        while (next.Count < populationSize)
        {
            Genome child = (UnityEngine.Random.value < crossoverRate)
                ? Genome.Crossover(top1, top2)
                : (UnityEngine.Random.value < 0.5f ? top1.Clone() : top2.Clone());

            child.Mutate(mutationRate, mutationStd);
            next.Add(child);
        }

        population = next;
        EvaluatePopulation();
        generation++;
        Emit();
    }

    // ===== Internos =====
    void EnsurePopulation()
    {
        if (population == null) population = new List<Genome>();
        populationSize = Mathf.Clamp(populationSize, 2, 50);

        if (population.Count != populationSize)
        {
            population.Clear();
            for (int i = 0; i < populationSize; i++)
                population.Add(Genome.RandomGenome());
            generation = 0;
            bestFitness = float.NegativeInfinity;
            bestGenome = null;
        }
        evalRng = new System.Random(evalSeed);
    }

    void EvaluatePopulation()
    {
        float sum = 0f;
        float bestGenLocal = float.NegativeInfinity;
        Genome bestGenomeLocal = null;

        for (int idx = 0; idx < population.Count; idx++)
        {
            var g = population[idx];
            g.fitness = EvaluateGenome(g, stepBudgetPerEval);
            sum += g.fitness;

            if (g.fitness > bestGenLocal) { bestGenLocal = g.fitness; bestGenomeLocal = g; }

            if (g.fitness > bestFitness)
            {
                bestFitness = g.fitness;
                bestGenome = g.Clone(); // melhor global
            }
        }

        lastAvgFitness = (population.Count > 0) ? sum / population.Count : 0f;
        lastBestGen = (population.Count > 0) ? bestGenLocal : float.NegativeInfinity;
        bestGenomeThisGen = (bestGenomeLocal == null) ? null : bestGenomeLocal.Clone();

        // salva campeão global no SO (para o botão "Jogar na Grid")
        if (championSO != null && bestGenome != null)
        {
            championSO.seed = bestGenome.seed;
            championSO.fitness = bestGenome.fitness;
            championSO.weights = bestGenome.FlatCopy();
            // Debug.Log($"[GA] Campeão salvo fitness={bestGenome.fitness} seed={bestGenome.seed} pesos={championSO.weights?.Length}");
        }
    }

    Genome Tournament()
    {
        int n = population.Count;
        int k = Mathf.Clamp(tournamentK, 1, n);
        Genome best = null;
        for (int i = 0; i < k; i++)
        {
            int idx = UnityEngine.Random.Range(0, n);
            var cand = population[idx];
            if (best == null || cand.fitness > best.fitness) best = cand;
        }
        return best ?? population[0];
    }

    void Emit()
    {
        OnGenerationAdvanced?.Invoke(new GenStats
        {
            gen = generation,
            avg = lastAvgFitness,
            bestSoFar = bestFitness,
            bestGen = lastBestGen,
            cx = crossoverRate,
            mut = mutationRate
        });
    }

    // ===== Tipos do problema =====
    public enum Agent3Action
    {
        MoveUp, MoveDown, MoveLeft, MoveRight,
        ShootUp, ShootDown, ShootLeft, ShootRight,
        Collect
    }

    public struct Observation
    {
        public bool ouro;                        // está sobre o ouro?
        public bool fU, fD, fL, fR;              // fedor por direção
        public bool bU, bD, bL, bR;              // brisa por direção
        public bool canUp, canDown, canLeft, canRight;
        public bool goalReturn;                  // ouro coletado && wumpus morto
    }

    // ===== Genoma =====
    public class Genome
    {
        public int seed;
        public float fitness;

        public const int ACTIONS = 9;
        // FEATS: ouro(1) + fedor(4) + brisa(4) + can(4) + goalReturn(1) = 14
        public const int FEATS = 14;

        // flat = [bias por ação] + [ACTIONS * FEATS] pesos
        public float[] flat;

        public Genome()
        {
            flat = new float[ACTIONS + ACTIONS * FEATS];
            for (int i = 0; i < flat.Length; i++)
                flat[i] = UnityEngine.Random.Range(-1f, 1f);
            seed = UnityEngine.Random.Range(0, int.MaxValue);
        }

        public static Genome RandomGenome() => new Genome();

        public Genome Clone()
        {
            var g = new Genome();
            g.seed = seed;
            g.fitness = fitness;
            Array.Copy(flat, g.flat, flat.Length);
            return g;
        }

        public static Genome Crossover(Genome a, Genome b)
        {
            var c = new Genome();
            int cut = UnityEngine.Random.Range(1, a.flat.Length);
            for (int i = 0; i < c.flat.Length; i++)
                c.flat[i] = (i < cut) ? a.flat[i] : b.flat[i];
            return c;
        }

        public void Mutate(float rate, float std)
        {
            for (int i = 0; i < flat.Length; i++)
                if (UnityEngine.Random.value < rate)
                {
                    float noise = (UnityEngine.Random.Range(-std, std) + UnityEngine.Random.Range(-std, std)) * 0.5f;
                    flat[i] += noise;
                }
        }

        // ---------- política linear: bias + w·x ----------
        int BiasOffset() => 0;
        int WeightsOffset() => ACTIONS;                       // após biases
        int WIndex(int action, int feat) => WeightsOffset() + action * FEATS + feat;

        static void BuildFeatures(Observation o, float[] x)   // x.Length == FEATS
        {
            int k = 0;
            x[k++] = o.ouro ? 1f : 0f;
            x[k++] = o.fU ? 1f : 0f; x[k++] = o.fD ? 1f : 0f; x[k++] = o.fL ? 1f : 0f; x[k++] = o.fR ? 1f : 0f;
            x[k++] = o.bU ? 1f : 0f; x[k++] = o.bD ? 1f : 0f; x[k++] = o.bL ? 1f : 0f; x[k++] = o.bR ? 1f : 0f;
            x[k++] = o.canUp ? 1f : 0f; x[k++] = o.canDown ? 1f : 0f; x[k++] = o.canLeft ? 1f : 0f; x[k++] = o.canRight ? 1f : 0f;
            x[k++] = o.goalReturn ? 1f : 0f;
        }

        public Agent3Action PickAction(Observation obs)
        {
            var x = new float[FEATS];
            BuildFeatures(obs, x);

            int best = 0;
            float bestV = float.NegativeInfinity;

            const float SOFT_LETHAL_MASK = 600f; // mesma ideia do Brain

            for (int a = 0; a < ACTIONS; a++)
            {
                float v = flat[BiasOffset() + a];
                for (int k = 0; k < FEATS; k++)
                    v += flat[WIndex(a, k)] * x[k];

                // máscara dura: fora da grade
                if (a == (int)Agent3Action.MoveUp && !obs.canUp) v = -1e6f;
                else if (a == (int)Agent3Action.MoveDown && !obs.canDown) v = -1e6f;
                else if (a == (int)Agent3Action.MoveLeft && !obs.canLeft) v = -1e6f;
                else if (a == (int)Agent3Action.MoveRight && !obs.canRight) v = -1e6f;
                else
                {
                    // máscara suave: direção letal (wumpus/poço adjacente)
                    if (a == (int)Agent3Action.MoveUp && (obs.fU || obs.bU)) v -= SOFT_LETHAL_MASK;
                    if (a == (int)Agent3Action.MoveDown && (obs.fD || obs.bD)) v -= SOFT_LETHAL_MASK;
                    if (a == (int)Agent3Action.MoveLeft && (obs.fL || obs.bL)) v -= SOFT_LETHAL_MASK;
                    if (a == (int)Agent3Action.MoveRight && (obs.fR || obs.bR)) v -= SOFT_LETHAL_MASK;
                }

                if (v > bestV) { bestV = v; best = a; }
            }

            return (Agent3Action)best;
        }


        public float[] FlatCopy() => (float[])flat.Clone();
    }

    // ===== Simulador Offline =====
    struct SimWorld
    {
        public int W, H;
        public HashSet<Vector2Int> pits;
        public HashSet<Vector2Int> wumpus;
        public Vector2Int? goldPos; // null se não houver
    }

    float EvaluateGenome(Genome g, int steps)
    {
        // ===== setup do mundo a partir da cena =====
        int W = Mathf.Max(0, GridGenerator.tamanhoX);
        int H = Mathf.Max(0, GridGenerator.tamanhoY);
        if (W <= 0 || H <= 0)
        {
            Debug.LogWarning("[Agent3GA] GridGenerator.tamanhoX/Y inválidos. Definindo 10x10 como fallback.");
            W = H = 10;
        }

        var pits = new HashSet<Vector2Int>();
        var wumps = new HashSet<Vector2Int>();
        Vector2Int? gold = null;

        if (TileManager.instancia)
        {
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    var p = new Vector2Int(x, y);
                    if (TileManager.instancia.PocoNaPosicao(p)) pits.Add(p);
                    var info = TileManager.instancia.ObterInfoDaTile(p);
                    if (info != null && info.temOuro) gold = p;
                }
        }
        if (GridGenerator.posicoesWumpus != null)
            foreach (var wp in GridGenerator.posicoesWumpus) wumps.Add(wp);

        // ===== campos de distância (BFS) =====
        var shootCells = new HashSet<Vector2Int>();
        foreach (var w in wumps)
        {
            var nbs = new[] { w + Vector2Int.up, w + Vector2Int.down, w + Vector2Int.left, w + Vector2Int.right };
            foreach (var q in nbs)
                if (InBounds(q, W, H) && !pits.Contains(q) && !wumps.Contains(q)) shootCells.Add(q);
        }
        var blockedKill = new HashSet<Vector2Int>(pits);
        foreach (var w in wumps) blockedKill.Add(w);
        int[,] distToShoot = BuildDistField(W, H, blockedKill, shootCells);
        int[,] distToGold = BuildDistField(W, H, pits, gold.HasValue ? new HashSet<Vector2Int> { gold.Value } : new HashSet<Vector2Int>());
        int[,] distToHome = BuildDistField(W, H, pits, new HashSet<Vector2Int> { Vector2Int.zero });

        // ===== shaping constantes =====
        const float STEP_COST = 1.0f;
        const float NEW_TILE_BONUS = 0.6f;
        const float PINGPONG_PEN = 12.0f;

        const float SOFT_LETHAL_MOVE_PEN = 700f;
        const float SHOULD_SHOOT_BONUS = 450f;
        const float SHOULD_SHOOT_PEN = 420f;

        const float MISS_GOLD_HERE_PEN = 150f;

        const float KILL_GAIN = 1.2f;
        const float GOLD_GAIN = 1.2f;
        const float HOME_GAIN = 1.2f;

        const int NO_PROGRESS_WINDOW = 5;
        const float STAGNATION_PEN = 20f;

        // ===== estado =====
        Vector2Int pos = Vector2Int.zero;
        Vector2Int prevPos = new Vector2Int(int.MinValue, int.MinValue);
        var visitados = new HashSet<Vector2Int>();
        var visitsByStage = new Dictionary<(Vector2Int, int), int>();

        bool collected = false;
        var wumpusAlive = new HashSet<Vector2Int>(wumps);
        float score = 0f;

        int noProgress = 0;

        for (int t = 0; t < steps; t++)
        {
            bool wv = wumpusAlive.Count > 0;
            int stage = wv ? 0 : (!collected ? 1 : 2);

            var obs = new Observation
            {
                ouro = gold.HasValue && !collected && pos == gold.Value,

                fU = wumpusAlive.Contains(pos + Vector2Int.up),
                fD = wumpusAlive.Contains(pos + Vector2Int.down),
                fL = wumpusAlive.Contains(pos + Vector2Int.left),
                fR = wumpusAlive.Contains(pos + Vector2Int.right),

                bU = pits.Contains(pos + Vector2Int.up),
                bD = pits.Contains(pos + Vector2Int.down),
                bL = pits.Contains(pos + Vector2Int.left),
                bR = pits.Contains(pos + Vector2Int.right),

                canUp = pos.y + 1 < H,
                canDown = pos.y - 1 >= 0,
                canLeft = pos.x - 1 >= 0,
                canRight = pos.x + 1 < W,

                goalReturn = (!wv && collected)
            };

            // distâncias “antes” do passo
            int prevKillD = (stage == 0) ? distToShoot[pos.x, pos.y] : 0;
            int prevGoldD = (stage == 1 && gold.HasValue) ? distToGold[pos.x, pos.y] : 0;
            int prevHomeD = (stage == 2) ? distToHome[pos.x, pos.y] : 0;

            var a = g.PickAction(obs);
            Vector2Int delta = Vector2Int.zero;

            bool adjW = obs.fU || obs.fD || obs.fL || obs.fR;

            switch (a)
            {
                case Agent3Action.MoveUp:
                    if (obs.fU || obs.bU) score -= SOFT_LETHAL_MOVE_PEN;
                    delta = Vector2Int.up; break;
                case Agent3Action.MoveDown:
                    if (obs.fD || obs.bD) score -= SOFT_LETHAL_MOVE_PEN;
                    delta = Vector2Int.down; break;
                case Agent3Action.MoveLeft:
                    if (obs.fL || obs.bL) score -= SOFT_LETHAL_MOVE_PEN;
                    delta = Vector2Int.left; break;
                case Agent3Action.MoveRight:
                    if (obs.fR || obs.bR) score -= SOFT_LETHAL_MOVE_PEN;
                    delta = Vector2Int.right; break;

                case Agent3Action.ShootUp:
                    score += ShootCell(ref wumpusAlive, pos + Vector2Int.up);
                    if (obs.fU) score += SHOULD_SHOOT_BONUS; else if (adjW) score -= SHOULD_SHOOT_PEN;
                    break;
                case Agent3Action.ShootDown:
                    score += ShootCell(ref wumpusAlive, pos + Vector2Int.down);
                    if (obs.fD) score += SHOULD_SHOOT_BONUS; else if (adjW) score -= SHOULD_SHOOT_PEN;
                    break;
                case Agent3Action.ShootLeft:
                    score += ShootCell(ref wumpusAlive, pos + Vector2Int.left);
                    if (obs.fL) score += SHOULD_SHOOT_BONUS; else if (adjW) score -= SHOULD_SHOOT_PEN;
                    break;
                case Agent3Action.ShootRight:
                    score += ShootCell(ref wumpusAlive, pos + Vector2Int.right);
                    if (obs.fR) score += SHOULD_SHOOT_BONUS; else if (adjW) score -= SHOULD_SHOOT_PEN;
                    break;

                case Agent3Action.Collect:
                    if (gold.HasValue && !collected && pos == gold.Value) { collected = true; score += 1000f; }
                    else score -= 500f;
                    break;
            }

            // “está sobre o ouro e não coletou”
            if (gold.HasValue && !collected && pos == gold.Value && a != Agent3Action.Collect)
                score -= MISS_GOLD_HERE_PEN;

            if (delta != Vector2Int.zero)
            {
                var alvo = pos + delta;
                if (InBounds(alvo, W, H))
                {
                    if (alvo == prevPos) score -= PINGPONG_PEN;

                    prevPos = pos;
                    pos = alvo;

                    // exploração
                    if (visitados.Add(pos)) score += NEW_TILE_BONUS;

                    // gradientes por estágio
                    bool progressed = false;
                    if (stage == 0)
                    {
                        int nd = distToShoot[pos.x, pos.y];
                        if (prevKillD < 4000 && nd < 4000) { score += KILL_GAIN * (prevKillD - nd); progressed |= (nd < prevKillD); }
                    }
                    if (stage == 1 && gold.HasValue)
                    {
                        int nd = distToGold[pos.x, pos.y];
                        if (prevGoldD < 4000 && nd < 4000) { score += GOLD_GAIN * (prevGoldD - nd); progressed |= (nd < prevGoldD); }
                    }
                    if (stage == 2)
                    {
                        int nd = distToHome[pos.x, pos.y];
                        if (prevHomeD < 4000 && nd < 4000) { score += HOME_GAIN * (prevHomeD - nd); progressed |= (nd < prevHomeD); }
                    }

                    // estagnação
                    if (progressed) noProgress = 0;
                    else
                    {
                        noProgress++;
                        if (noProgress >= NO_PROGRESS_WINDOW)
                            score -= STAGNATION_PEN * (noProgress - NO_PROGRESS_WINDOW + 1);
                    }

                    // penaliza “rodar” no mesmo (pos, estágio)
                    var key = (pos, stage);
                    if (!visitsByStage.TryGetValue(key, out var cnt)) cnt = 0;
                    visitsByStage[key] = cnt + 1;
                    if (cnt >= 3) score -= 2f * (cnt - 2);
                }

                score -= STEP_COST;
            }

            // morte?
            if (pits.Contains(pos) || wumpusAlive.Contains(pos))
            {
                score -= 1500f; // pode subir para 3000f se quiser pressionar mais
                break;
            }

            // vitória?
            if (!wumpusAlive.Any() && collected && pos == Vector2Int.zero)
            {
                score += 2200f;
                break;
            }
        }

        return score;

        static float ShootCell(ref HashSet<Vector2Int> alive, Vector2Int alvo)
        {
            if (alive.Contains(alvo)) { alive.Remove(alvo); return +1000f; }
            return -500f;
        }
    }

    static bool InBounds(Vector2Int p, int W, int H) => p.x >= 0 && p.x < W && p.y >= 0 && p.y < H;

    // BFS multi-fonte: retorna distâncias (int.MaxValue/400000000 se inalcançável)
    static int[,] BuildDistField(int W, int H, HashSet<Vector2Int> blocked, HashSet<Vector2Int> goals)
    {
        int[,] dist = new int[W, H];
        for (int x = 0; x < W; x++) for (int y = 0; y < H; y++) dist[x, y] = 400000000;

        var q = new Queue<Vector2Int>();
        foreach (var g in goals)
        {
            if (!InBounds(g, W, H) || (blocked != null && blocked.Contains(g))) continue;
            dist[g.x, g.y] = 0; q.Enqueue(g);
        }

        var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        while (q.Count > 0)
        {
            var p = q.Dequeue();
            int d = dist[p.x, p.y];
            foreach (var ddir in dirs)
            {
                var n = p + ddir;
                if (!InBounds(n, W, H)) continue;
                if (blocked != null && blocked.Contains(n)) continue;
                if (dist[n.x, n.y] <= d + 1) continue;
                dist[n.x, n.y] = d + 1;
                q.Enqueue(n);
            }
        }
        return dist;
    }

    // helpers externos ao loop (mais simples para depurar)
    static float Shoot(ref SimWorld world, Vector2Int alvo)
    {
        if (world.wumpus.Contains(alvo))
        {
            world.wumpus.Remove(alvo);
            return +1000f;
        }
        return -500f;
    }

    static float Collect(ref SimWorld world, Vector2Int p, ref bool collectedFlag)
    {
        if (world.goldPos.HasValue && !collectedFlag && p == world.goldPos.Value)
        {
            collectedFlag = true;
            return +1000f;
        }
        return -500f;
    }
}