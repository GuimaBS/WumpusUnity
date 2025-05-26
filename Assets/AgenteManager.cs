using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AgenteManager : MonoBehaviour
{
    public GameObject agentePrefab;
    public Transform agentePai;

    private Toggle autoSpawnToggle;
    private bool autoSpawnAtivo = false;
    private List<GameObject> agentesVivos = new List<GameObject>();

    private void Start()
    {
        GameObject toggleObj = GameObject.FindGameObjectWithTag("AutoSpawnToggle");
        if (toggleObj != null)
        {
            autoSpawnToggle = toggleObj.GetComponent<Toggle>();
            autoSpawnToggle.onValueChanged.AddListener(delegate { autoSpawnAtivo = autoSpawnToggle.isOn; });
        }
    }

    public void CriarAgente1()
    {
        GameObject novoAgente = Instantiate(agentePrefab, new Vector3(0, 0.5f, 0), Quaternion.identity, agentePai);
        agentesVivos.Add(novoAgente);

        AgenteReativo script = novoAgente.GetComponent<AgenteReativo>();
        if (script != null)
        {
            script.onMorte += () =>
            {
                agentesVivos.Remove(novoAgente);
                if (autoSpawnAtivo)
                {
                    CriarAgente1(); // Gera outro automaticamente
                }
            };
        }
        LogManager.instancia.AdicionarLog("Agente 1 criado com sucesso na posição (0,0).");
    }

    public void RemoverTodosAgentes()
    {
        foreach (GameObject agente in agentesVivos)
        {
            if (agente != null)
                Destroy(agente);
        }
        agentesVivos.Clear();
    }
}
