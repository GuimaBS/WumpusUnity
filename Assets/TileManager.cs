using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager instancia;

    public List<Vector2Int> posicoesValidas = new List<Vector2Int>();

    void Awake()
    {
        if (instancia == null)
            instancia = this;
        else
            Destroy(gameObject);
    }

    public void RegistrarTile(Vector2Int pos)
    {
        if (!posicoesValidas.Contains(pos))
        {
            posicoesValidas.Add(pos);
        }
    }

    public bool TileExiste(Vector2Int pos)
    {
        return posicoesValidas.Contains(pos);
    }
}
