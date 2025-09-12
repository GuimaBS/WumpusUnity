using System;
using UnityEngine;

[Serializable]
public class Agent3Genome
{
    // 8 estados (B,F,L bitmask 0..7) x 9 ações
    public float[,] weights = new float[8, 9];

    public const int NUM_STATES = 8;
    public const int NUM_ACTIONS = 9;

    public Agent3Genome(bool randomInit = true)
    {
        if (randomInit)
            Randomize();
    }

    public void Randomize()
    {
        var rnd = UnityEngine.Random.value;
        for (int s = 0; s < NUM_STATES; s++)
        {
            for (int a = 0; a < NUM_ACTIONS; a++)
            {
                // pequena variação inicial, centrada
                weights[s, a] = UnityEngine.Random.Range(-0.5f, 0.5f);
            }
        }
    }

    // estado = bitmask: (brisa?1:0) | (fedor?2:0) | (brilho?4:0)
    public int StateIndex(bool brisa, bool fedor, bool brilho)
    {
        int s = 0;
        if (brisa) s |= 1;
        if (fedor) s |= 2;
        if (brilho) s |= 4;
        return s;
    }

    public int PickAction(bool brisa, bool fedor, bool brilho)
    {
        int s = StateIndex(brisa, fedor, brilho);
        // argmax
        int best = 0;
        float bestV = weights[s, 0];
        for (int a = 1; a < NUM_ACTIONS; a++)
        {
            if (weights[s, a] > bestV)
            {
                bestV = weights[s, a];
                best = a;
            }
        }
        return best;
    }

    public Agent3Genome Clone()
    {
        var g = new Agent3Genome(false);
        Array.Copy(weights, g.weights, weights.Length);
        return g;
    }
}
