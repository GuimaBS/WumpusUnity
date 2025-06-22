using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager instancia;

    [Header("Partículas do Jogador")]
    public GameObject efeitoColetarOuro;
    public GameObject efeitoFlechaDisparada;
    public GameObject efeitoImpacto;
    public GameObject efeitoMorte;

    private Transform player;

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
    }

    public void SpawnEfeitoColetarOuro()
    {
        if (efeitoColetarOuro != null && player != null)
            Instantiate(efeitoColetarOuro, player.position, Quaternion.identity);
    }

    public void SpawnEfeitoFlecha()
    {
        if (efeitoFlechaDisparada != null && player != null)
            Instantiate(efeitoFlechaDisparada, player.position, Quaternion.identity);
    }

    public void SpawnEfeitoImpacto(Vector3 posicao)
    {
        if (efeitoImpacto != null)
            Instantiate(efeitoImpacto, posicao, Quaternion.identity);
    }

    public void SpawnEfeitoMorte()
    {
        if (efeitoMorte != null && player != null)
            Instantiate(efeitoMorte, player.position, Quaternion.identity);
    }
}
