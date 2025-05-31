using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgenteManager : MonoBehaviour
{
    public GameObject agente1Prefab;
    public GameObject agente2Prefab;
    public Transform agentePai;
    public Toggle autoSpawnToggle;

    private bool autoSpawnAtivo = false;
    private List<GameObject> agentesVivos = new List<GameObject>();

    private void Start()
    {
        if (autoSpawnToggle != null)
        {
            autoSpawnToggle.onValueChanged.AddListener(delegate { autoSpawnAtivo = autoSpawnToggle.isOn; });
        }
    }

    public void CriarAgente1()
    {
        CriarAgente(agente1Prefab, "Agente1");
    }

    public void CriarAgente2()
    {
        CriarAgente(agente2Prefab, "Agente2");
    }

    private void CriarAgente(GameObject prefab, string nomeAgente)
    {
        GameObject novoAgente = Instantiate(prefab, new Vector3(0, 0.5f, 0), Quaternion.identity, agentePai);
        agentesVivos.Add(novoAgente);

        if (nomeAgente == "Agente1")
        {
            var script = novoAgente.GetComponent<AgenteReativo>();
            if (script != null)
            {
                script.onMorte += () =>
                {
                    agentesVivos.Remove(novoAgente);
                    if (autoSpawnAtivo) CriarAgente1();
                };
            }
        }
        else if (nomeAgente == "Agente2")
        {
            var script = novoAgente.GetComponent<AgenteInteligente>();
            if (script != null)
            {
                script.onMorte += () =>
                {
                    agentesVivos.Remove(novoAgente);
                    if (autoSpawnAtivo) CriarAgente2();
                };
            }
        }

        LogManager.instancia?.AdicionarLog($"{nomeAgente} criado com sucesso.");
    }

    public void RemoverTodosAgentes()
    {
        foreach (var agente in agentesVivos)
        {
            if (agente != null)
                Destroy(agente);
        }
        agentesVivos.Clear();
    }
}
