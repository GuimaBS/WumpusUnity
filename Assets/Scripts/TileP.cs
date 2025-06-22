using System.Collections;
using UnityEngine;

public class TileP : MonoBehaviour
{
    [Header("Prefab do Fantasma")]
    public GameObject prefabFantasma;

    private Transform spawnPoint;

    private void Start()
    {
        spawnPoint = transform.Find("SpawnFantasma");

        if (spawnPoint == null)
        {
            Debug.LogWarning("SpawnFantasma n�o encontrado na tile de po�o: " + gameObject.name);
        }
    }

    public void AtivarFantasma()
    {
        if (prefabFantasma == null)
        {
            Debug.LogError("Prefab do fantasma n�o foi atribu�do na TileP: " + gameObject.name);
            return;
        }

        StartCoroutine(EmitirFantasma());
    }

    private IEnumerator EmitirFantasma()
    {
        Vector3 posicaoSpawn = spawnPoint != null ? spawnPoint.position : transform.position + new Vector3(0, 0, -2);

        GameObject fantasma = Instantiate(prefabFantasma, posicaoSpawn, Quaternion.identity);
        yield return new WaitForSeconds(1f);
        Destroy(fantasma);
    }
}
