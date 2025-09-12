using UnityEngine;
using System.Collections.Generic;

public class Agent3Brain : MonoBehaviour
{
    [Header("Modo")]
    public bool training = false;
    [Range(0f, 1f)] public float epsilon = 0f;
    public bool allowRandomFallback = false;

    [Header("Debug")]
    public bool logAppliedChampion = true;

    // Vetor achatado de pesos:
    // [ACTIONS] biases + [ACTIONS * FEATS] pesos
    [SerializeField] private float[] weights;

    // === Injeção do campeão (chamado pelo Spawner) ===
    public void ApplyChampion(float[] flat)
    {
        if (flat == null || flat.Length == 0)
        {
            Debug.LogError("[Agent3Brain] ApplyChampion recebeu vetor nulo/vazio.");
            return;
        }

        weights = (float[])flat.Clone();
        training = false;
        epsilon = 0f;
        allowRandomFallback = false;

        if (logAppliedChampion)
            Debug.Log($"[Agent3Brain] Campeão aplicado. Pesos={weights.Length}");
    }

    void Start()
    {
        if (training)
        {
            if (weights == null || weights.Length == 0)
            {
                int A = Agent3GA.Genome.ACTIONS;
                int F = Agent3GA.Genome.FEATS;
                int n = A + A * F; // biases + pesos
                weights = new float[n];
                for (int i = 0; i < n; i++) weights[i] = Random.Range(-1f, 1f);
            }
        }
        // Em modo de inferência (training=false) não tocamos nos pesos.
    }

    // ===== API de decisão pública =====
    public Agent3GA.Agent3Action Decide(Agent3GA.Observation obs)
    {
        if (!training && !allowRandomFallback)
            return GreedyAction(obs);

        if (Random.value < epsilon)
            return RandomAction(obs);

        return GreedyAction(obs);
    }

    // Mesma lógica do Genome.PickAction, mas usando 'weights' do campeão
    private Agent3GA.Agent3Action GreedyAction(Agent3GA.Observation obs)
    {
        if (weights == null || weights.Length == 0)
        {
            Debug.LogWarning("[Agent3Brain] Pesos vazios; retornando ação padrão.");
            return Agent3GA.Agent3Action.MoveRight;
        }

        int A = Agent3GA.Genome.ACTIONS;
        int F = Agent3GA.Genome.FEATS;

        // offsets iguais aos do GA
        int BiasOffset = 0;
        int WeightsOffset = A;
        System.Func<int, int, int> WIndex = (a, k) => WeightsOffset + a * F + k;

        // mesmas 14 features do GA
        float[] x = new float[F];
        int k = 0;
        x[k++] = obs.ouro ? 1f : 0f;
        x[k++] = obs.fU ? 1f : 0f; x[k++] = obs.fD ? 1f : 0f; x[k++] = obs.fL ? 1f : 0f; x[k++] = obs.fR ? 1f : 0f;
        x[k++] = obs.bU ? 1f : 0f; x[k++] = obs.bD ? 1f : 0f; x[k++] = obs.bL ? 1f : 0f; x[k++] = obs.bR ? 1f : 0f;
        x[k++] = obs.canUp ? 1f : 0f; x[k++] = obs.canDown ? 1f : 0f; x[k++] = obs.canLeft ? 1f : 0f; x[k++] = obs.canRight ? 1f : 0f;
        x[k++] = obs.goalReturn ? 1f : 0f;

        const float SOFT_LETHAL_MASK = 600f; // igual ao do GA

        int best = 0;
        float bestV = float.NegativeInfinity;

        for (int a = 0; a < A; a++)
        {
            // 1) valor linear
            float v = weights[BiasOffset + a];
            for (int j = 0; j < F; j++)
                v += weights[WIndex(a, j)] * x[j];

            // 2) máscara dura (fora da grade)
            if (a == (int)Agent3GA.Agent3Action.MoveUp && !obs.canUp) v = -1e6f;
            else if (a == (int)Agent3GA.Agent3Action.MoveDown && !obs.canDown) v = -1e6f;
            else if (a == (int)Agent3GA.Agent3Action.MoveLeft && !obs.canLeft) v = -1e6f;
            else if (a == (int)Agent3GA.Agent3Action.MoveRight && !obs.canRight) v = -1e6f;
            else
            {
                // 3) máscara suave (direção letal conhecida)
                if (a == (int)Agent3GA.Agent3Action.MoveUp && (obs.fU || obs.bU)) v -= SOFT_LETHAL_MASK;
                if (a == (int)Agent3GA.Agent3Action.MoveDown && (obs.fD || obs.bD)) v -= SOFT_LETHAL_MASK;
                if (a == (int)Agent3GA.Agent3Action.MoveLeft && (obs.fL || obs.bL)) v -= SOFT_LETHAL_MASK;
                if (a == (int)Agent3GA.Agent3Action.MoveRight && (obs.fR || obs.bR)) v -= SOFT_LETHAL_MASK;
            }

            if (v > bestV) { bestV = v; best = a; }
        }

        return (Agent3GA.Agent3Action)best;
    }

    private Agent3GA.Agent3Action RandomAction(Agent3GA.Observation obs)
    {
        var cand = new List<Agent3GA.Agent3Action>();
        if (obs.canUp) cand.Add(Agent3GA.Agent3Action.MoveUp);
        if (obs.canDown) cand.Add(Agent3GA.Agent3Action.MoveDown);
        if (obs.canLeft) cand.Add(Agent3GA.Agent3Action.MoveLeft);
        if (obs.canRight) cand.Add(Agent3GA.Agent3Action.MoveRight);
        cand.Add(Agent3GA.Agent3Action.Collect);
        cand.Add(Agent3GA.Agent3Action.ShootUp);
        cand.Add(Agent3GA.Agent3Action.ShootDown);
        cand.Add(Agent3GA.Agent3Action.ShootLeft);
        cand.Add(Agent3GA.Agent3Action.ShootRight);

        return cand[Random.Range(0, cand.Count)];
    }
}
