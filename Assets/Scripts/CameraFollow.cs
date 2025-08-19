using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 4f, -6.3f);
    public float smoothSpeed = 7f;

    // dummy para foco temporário
    Transform focusDummy;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothed = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothed;

        transform.LookAt(target);
    }

    public void DefinirAlvo(Transform novoAlvo)
    {
        target = novoAlvo;
    }

    // NOVO: foca em um ponto fixo (centro da sala)
    public void FocarNoPonto(Vector3 worldPos)
    {
        if (focusDummy == null)
        {
            var go = new GameObject("CameraFocus");
            focusDummy = go.transform;
        }
        focusDummy.position = worldPos;
        target = focusDummy;
    }

    // NOVO: volta a seguir um alvo (player ou CameraTarget)
    public void RetomarFollow(Transform novoAlvo)
    {
        target = novoAlvo;
    }
}
