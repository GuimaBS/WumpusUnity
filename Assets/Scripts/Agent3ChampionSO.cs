// Assets/Scripts/Agent3/Agent3ChampionSO.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Wumpus/Agent3Champion")]
public class Agent3ChampionSO : ScriptableObject
{
    public int seed;
    public float fitness;
    public float[] weights;   // vetor linear com todos os pesos/limiares
}
