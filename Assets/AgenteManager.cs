using UnityEngine;

public class AgenteManager : MonoBehaviour
{
    public GameObject agente1Prefab;
    public GameObject brilhoAgentePrefab;

    private GameObject agenteInstanciado;

    public void SpawnAgente1()
    {
        if (agenteInstanciado != null)
        {
            Destroy(agenteInstanciado);
        }

        Vector3 posicaoInicial = new Vector3(0, 0.5f, 0); // posição inicial
        agenteInstanciado = Instantiate(agente1Prefab, posicaoInicial, Quaternion.identity);

        // Adiciona o brilho nos pés
        GameObject brilho = Instantiate(brilhoAgentePrefab, agenteInstanciado.transform);
        brilho.transform.localPosition = Vector3.zero; // centraliza nos pés
    }
}

