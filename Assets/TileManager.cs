using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager instancia;

    private Dictionary<Vector2Int, GameObject> tilesValidas = new Dictionary<Vector2Int, GameObject>();

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    /// Registra a posi��o de uma tile v�lida no dicion�rio.
   
    public void RegistrarTile(Vector2Int posicao, GameObject tile)
    {
        if (!tilesValidas.ContainsKey(posicao))
        {
            tilesValidas.Add(posicao, tile);
        }
    }


 
    /// Retorna a tile GameObject em determinada posi��o, se existir.
    public GameObject ObterTileEm(Vector2Int posicao)
    {
        if (tilesValidas.ContainsKey(posicao))
        {
            return tilesValidas[posicao];
        }
        return null;
    }


    /// Retorna todas as posi��es v�lidas registradas.
    public List<Vector2Int> ObterTodasAsPosicoes()
    {
        return new List<Vector2Int>(tilesValidas.Keys);
    }
}
