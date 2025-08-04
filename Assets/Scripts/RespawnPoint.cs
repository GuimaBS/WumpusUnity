using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    public static RespawnPoint instancia;

    private void Awake()
    {
        // sem conflito
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
    }
}
