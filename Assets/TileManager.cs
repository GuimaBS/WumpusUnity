using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager instancia;

    // Estrutura para armazenar informa��es de cada tile
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

    // Registrar uma tile
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

    // Obter GameObject da tile
    public GameObject ObterTileEm(Vector2Int posicao)
    {
        if (tilesInfo.ContainsKey(posicao))
        {
            return tilesInfo[posicao].tileObject;
        }
        return null;
    }

    // Obter todas as posi��es
    public List<Vector2Int> ObterTodasAsPosicoes()
    {
        return new List<Vector2Int>(tilesInfo.Keys);
    }

    // Obter informa��es completas da tile
    public TileInfo ObterInfoDaTile(Vector2Int posicao)
    {
        if (tilesInfo.ContainsKey(posicao))
        {
            return tilesInfo[posicao];
        }
        return null;
    }

    // Atualizar dinamicamente informa��es de uma tile
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

    // Verificar se h� po�o na posi��o
    public bool PocoNaPosicao(Vector2Int posicao)
    {
        return tilesInfo.ContainsKey(posicao) && tilesInfo[posicao].temPoco;
    }

    // Obter TileP na posi��o (tile de po�o)
    public TileP ObterTilePNaPosicao(Vector2Int posicao)
    {
        if (tilesInfo.ContainsKey(posicao))
        {
            var info = tilesInfo[posicao];

            if (info.temPoco && info.tileObject != null)
            {
                return info.tileObject.GetComponent<TileP>();
            }
        }
        return null;
    }
}
