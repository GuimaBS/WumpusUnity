using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    //  O GridGenerator procura
    public static TileManager instancia;

    // Lista de posições válidas das tiles
    public List<Vector3> tilesValidas = new List<Vector3>();

    void Awake()
    {
        instancia = this;
    }

    // Método para registrar a posição de uma tile válida
    public void RegistrarTile(Vector2Int pos)
    {
        // Ajuste de posição para multiplicar pelo tamanho padrão das tiles
        Vector3 tilePos = new Vector3(pos.x * 1.7f, 0, pos.y * 1.7f);
        if (!tilesValidas.Contains(tilePos))
        {
            tilesValidas.Add(tilePos);
        }
    }
}
