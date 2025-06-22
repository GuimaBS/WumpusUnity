using UnityEngine;

public class LightManager : MonoBehaviour
{
    public static LightManager instancia;

    [Header("Configurações de Luz")]
    public float tamanhoTile = 10f;
    public Light luzPrincipal;

    private Transform player;
    private Vector2Int ultimaPosicaoPlayer;

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

    public void DefinirPlayer(Transform novoPlayer)
    {
        player = novoPlayer;
        ultimaPosicaoPlayer = ConverterPosicao(player.position);
        AtualizarLuzes();
    }

    private void Update()
    {
        if (player == null) return;

        Vector2Int posicaoAtual = ConverterPosicao(player.position);
        if (posicaoAtual != ultimaPosicaoPlayer)
        {
            ultimaPosicaoPlayer = posicaoAtual;
            AtualizarLuzes();
        }
    }

    private Vector2Int ConverterPosicao(Vector3 posicao)
    {
        int x = Mathf.RoundToInt(posicao.x / tamanhoTile);
        int y = Mathf.RoundToInt(posicao.z / tamanhoTile);
        return new Vector2Int(x, y);
    }

    private void AtualizarLuzes()
    {
        if (luzPrincipal != null && player != null)
        {
            luzPrincipal.transform.position = new Vector3(
                player.position.x,
                luzPrincipal.transform.position.y,
                player.position.z
            );
        }
    }
}
