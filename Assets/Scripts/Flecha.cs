using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flecha : MonoBehaviour
{
    [Header("Configurações")]
    public float velocidade = 20f;
    public float alcanceMaximo = 50f;
    public float tempoDeVida = 3f;

    [Header("Efeito Visual")]
    public GameObject efeitoDeTrilha; // Partícula ou trilha que ficará presa na flecha
    public GameObject efeitoDeImpacto; // Efeito ao colidir

    private Vector3 pontoDeOrigem;

    void Start()
    {
        pontoDeOrigem = transform.position;

        if (efeitoDeTrilha != null)
        {
            Instantiate(efeitoDeTrilha, transform.position, transform.rotation, transform);
        }

        // Destrói após um tempo de segurança, mesmo que não bata em nada
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
        // Aqui você pode colocar condições, como detectar se é o Wumpus, parede, etc.
        Debug.Log("Flecha colidiu com " + other.name);

        if (efeitoDeImpacto != null)
        {
            Instantiate(efeitoDeImpacto, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}
