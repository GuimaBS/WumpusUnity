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


    /// Registra a posição de uma tile válida no dicionário.
   
    public void RegistrarTile(Vector2Int posicao, GameObject tile)
    {
        if (!tilesValidas.ContainsKey(posicao))
        {
            tilesValidas.Add(posicao, tile);
        }
    }


 
    /// Retorna a tile GameObject em determinada posição, se existir.
    public GameObject ObterTileEm(Vector2Int posicao)
    {
        if (tilesValidas.ContainsKey(posicao))
        {
            return tilesValidas[posicao];
        }
        return null;
    }


    /// Retorna todas as posições válidas registradas.
    public List<Vector2Int> ObterTodasAsPosicoes()
    {
        return new List<Vector2Int>(tilesValidas.Keys);
    }
}
