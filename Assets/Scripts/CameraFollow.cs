using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Configura��es de C�mera")]
    public Transform target;  // Alvo que a c�mera segue
    public Vector3 offset = new Vector3(0, 10, -7);
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        // Posi��o desejada da c�mera baseada no alvo
        Vector3 desiredPosition = target.position + offset;

        // Suaviza movimento
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // A c�mera olha diretamente para o target 
        Vector3 lookAtPoint = target.position + Vector3.up * 0.5f;
        transform.LookAt(lookAtPoint);
    }

    // Permite definir o alvo externamente
    public void DefinirAlvo(Transform novoAlvo)
    {
        target = novoAlvo;
    }

    // Permite mover a c�mera manualmente para um ponto na morte
    public void MoverParaPonto(Vector3 ponto)
    {
        Vector3 desiredPosition = ponto + offset;
        transform.position = desiredPosition;
        transform.LookAt(ponto + Vector3.up * 0.5f);
    }
}

