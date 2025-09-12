using UnityEngine;

public class Agent3RuntimeSpawner : MonoBehaviour
{
    public Agent3ChampionSO championSO;
    public GameObject agent3Prefab;
    public Transform spawnPoint; // qualquer posição segura (ex.: sala (0,0))

    public void SpawnChampion()
    {
        if (championSO.weights == null || championSO.weights.Length == 0)
        {
            Debug.LogError("[Spawner] Campeão sem pesos. Rode o laboratório e salve o campeão primeiro.");
            return;
        }

        UnityEngine.Random.InitState(championSO.seed);

        var go = Instantiate(agent3Prefab, spawnPoint.position, Quaternion.identity);
        var brain = go.GetComponent<Agent3Brain>();
        if (brain == null)
        {
            Debug.LogError("[Spawner] Prefab do Agente3 não possui Agent3Brain.");
            return;
        }

        brain.ApplyChampion(championSO.weights);
        brain.training = false;       // desliga treino
        brain.epsilon = 0f;           // zero aleatoriedade
        brain.allowRandomFallback = false; // nada de RandomAction() de “segurança”

        Debug.Log($"[Spawner] Campeão instanciado: fitness={championSO.fitness} seed={championSO.seed} pesos={championSO.weights.Length}");
    }
}
