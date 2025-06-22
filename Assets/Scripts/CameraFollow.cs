using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Configurações de Câmera")]
    public Transform target;  // Arraste aqui o CameraTarget
    public Vector3 offset = new Vector3(0, 10, -7);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        // Posição desejada da câmera baseada no alvo fixo
        Vector3 desiredPosition = target.position + offset;

        // Suaviza movimento
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // A câmera olha diretamente para o target (um pouco mais alto se quiser)
        Vector3 lookAtPoint = target.position + Vector3.up * 0.5f;
        transform.LookAt(lookAtPoint);
    }

    // Permite definir o alvo externamente
    public void DefinirAlvo(Transform novoAlvo)
    {
        target = novoAlvo;
    }
}
