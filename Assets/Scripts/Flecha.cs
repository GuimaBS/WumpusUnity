using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flecha : MonoBehaviour
{
    [Header("Configura��es")]
    public float velocidade = 20f;
    public float alcanceMaximo = 50f;
    public float tempoDeVida = 3f;

    [Header("Efeito Visual")]
    public GameObject efeitoDeTrilha; // Part�cula ou trilha que ficar� presa na flecha
    public GameObject efeitoDeImpacto; // Efeito ao colidir

    private Vector3 pontoDeOrigem;

    void Start()
    {
        pontoDeOrigem = transform.position;

        if (efeitoDeTrilha != null)
        {
            Instantiate(efeitoDeTrilha, transform.position, transform.rotation, transform);
        }

        // Destr�i ap�s um tempo de seguran�a, mesmo que n�o bata em nada
        Destroy(gameObject, tempoDeVida);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * velocidade * Time.deltaTime);

        float distanciaPercorrida = Vector3.Distance(pontoDeOrigem, transform.position);
        if (distanciaPercorrida >= alcanceMaximo)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Aqui voc� pode colocar condi��es, como detectar se � o Wumpus, parede, etc.
        Debug.Log("Flecha colidiu com " + other.name);

        if (efeitoDeImpacto != null)
        {
            Instantiate(efeitoDeImpacto, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
