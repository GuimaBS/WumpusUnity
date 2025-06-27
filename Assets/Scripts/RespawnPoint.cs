using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    public static RespawnPoint instancia;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
            Debug.Log($"[RespawnPoint] Registrado em {transform.position}");
        }
        else
        {
            Debug.LogWarning("[RespawnPoint] J� existe uma inst�ncia ativa! Esta ser� ignorada.");
        }
    }
}
