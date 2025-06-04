using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager instancia;

    // Estrutura para armazenar informações de cada tile
    public class TileInfo
    {
        public GameObject tileObject;
        public bool temPoco;
        public bool temBrisa;
        public bool temFedor;
        public bool temOuro;
    }

    private Dictionary<Vector2Int, TileInfo> tilesInfo = new Dictionary<Vector2Int, TileInfo>();

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


    /// Registra uma tile com todas as informações úteis.

    public void RegistrarTile(Vector2Int posicao, GameObject tileObj, bool temPoco = false, bool temBrisa = false, bool temFedor = false, bool temOuro = false)
    {
        if (!tilesInfo.ContainsKey(posicao))
        {
            TileInfo novaInfo = new TileInfo
            {
                tileObject = tileObj,
                temPoco = temPoco,
                temBrisa = temBrisa,
                temFedor = temFedor,
                temOuro = temOuro
            };

            tilesInfo.Add(posicao, novaInfo);
        }
    }


    /// Retorna o GameObject da tile em determinada posição, se existir.

    public GameObject ObterTileEm(Vector2Int posicao)
    {
        if (tilesInfo.ContainsKey(posicao))
        {
            return tilesInfo[posicao].tileObject;
        }
        return null;
    }

 
    /// Retorna todas as posições registradas.
 
    public List<Vector2Int> ObterTodasAsPosicoes()
    {
        return new List<Vector2Int>(tilesInfo.Keys);
    }


    /// Retorna todas as informações (TileInfo) de uma posição.

    public TileInfo ObterInfoDaTile(Vector2Int posicao)
    {
        if (tilesInfo.ContainsKey(posicao))
        {
            return tilesInfo[posicao];
        }
        return null;
    }

   
    /// Atualiza dinamicamente as propriedades de uma tile (caso algo mude depois).
 
    public void AtualizarInfoDaTile(Vector2Int posicao, bool? temPoco = null, bool? temBrisa = null, bool? temFedor = null, bool? temOuro = null)
    {
        if (tilesInfo.ContainsKey(posicao))
        {
            TileInfo info = tilesInfo[posicao];

            if (temPoco.HasValue) info.temPoco = temPoco.Value;
            if (temBrisa.HasValue) info.temBrisa = temBrisa.Value;
            if (temFedor.HasValue) info.temFedor = temFedor.Value;
            if (temOuro.HasValue) info.temOuro = temOuro.Value;
        }
    }
}
