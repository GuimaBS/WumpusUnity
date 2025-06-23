using System.Collections.Generic;
using UnityEngine;

public class SalaManager : MonoBehaviour
{
    public static SalaManager instancia;
    public Dictionary<Vector2Int, GameObject> mapaGerado;

    private void Awake()
    {
        if (instancia == null) instancia = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        PlayerGridGenerator.OnMapaGerado += CarregarMapa;
    }

    private void OnDisable()
    {
        PlayerGridGenerator.OnMapaGerado -= CarregarMapa;
    }

    private void CarregarMapa()
    {
        mapaGerado = PlayerGridGenerator.instancia.mapaGerado;
        Debug.Log("SalaManager: mapaGerado carregado.");
    }

    public void AtualizarSalasAtivas(Vector2Int posicaoAtual)
    {
        if (mapaGerado == null)
        {
            return;
        }

        foreach (var kvp in mapaGerado)
        {
            bool ativa = kvp.Key == posicaoAtual;
            kvp.Value.SetActive(ativa);
        }
    }

    public void AtualizarSalaEAdjacentes(Vector2Int posicaoAtual)
    {
        if (mapaGerado == null)
        {
            return;
        }

        foreach (var kvp in mapaGerado)
        {
            bool ativa = Mathf.Abs(kvp.Key.x - posicaoAtual.x) <= 1 &&
                         Mathf.Abs(kvp.Key.y - posicaoAtual.y) <= 1;
            kvp.Value.SetActive(ativa);
        }
    }
}
