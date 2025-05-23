using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    //  O GridGenerator procura
    public static TileManager instancia;

    // Lista de posi��es v�lidas das tiles
    public List<Vector3> tilesValidas = new List<Vector3>();

    void Awake()
    {
        instancia = this;
    }

    // M�todo para registrar a posi��o de uma tile v�lida
    public void RegistrarTile(Vector2Int pos)
    {
        // Ajuste de posi��o para multiplicar pelo tamanho padr�o das tiles
        Vector3 tilePos = new Vector3(pos.x * 1.7f, 0, pos.y * 1.7f);
        if (!tilesValidas.Contains(tilePos))
        {
            tilesValidas.Add(tilePos);
        }
    }
}
