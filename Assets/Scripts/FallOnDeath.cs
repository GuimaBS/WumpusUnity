using UnityEngine;
using System.Collections;

public class FallOnDeath : MonoBehaviour
{
    [Header("Config da Queda")]
    public float yDestino = -4f;     
    public float duracao = 0.45f;     
    public AnimationCurve curva = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Rigidbody rb;
    CharacterController cc;

    void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out cc);
    }

    /// Chamar quando o player MORRER EM PO�O
    public IEnumerator ExecutarQueda()
    {
        bool ccWasEnabled = cc ? cc.enabled : false;
        if (cc) cc.enabled = false;

        bool hadRb = rb != null;
        Vector3 oldVel = Vector3.zero;
        Vector3 oldAng = Vector3.zero;
        bool oldUseGravity = false;
        bool oldIsKinematic = false;

        if (hadRb)
        {
            oldVel = rb.linearVelocity;
            oldAng = rb.angularVelocity;
            oldUseGravity = rb.useGravity;
            oldIsKinematic = rb.isKinematic;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        Vector3 start = transform.position;
        Vector3 end = new Vector3(start.x, yDestino, start.z);

        float t = 0f;
        float D = Mathf.Max(0.01f, duracao);

        while (t < 1f)
        {
            t += Time.deltaTime / D;
            float e = curva.Evaluate(Mathf.Clamp01(t));
            transform.position = Vector3.LerpUnclamped(start, end, e);
            yield return null;
        }
        transform.position = end;

        // volta os estados (o respawn reposiciona logo depois)
        if (cc) cc.enabled = ccWasEnabled;
        if (hadRb)
        {
            rb.useGravity = oldUseGravity;
            rb.isKinematic = oldIsKinematic;
            rb.linearVelocity = oldVel;
            rb.angularVelocity = oldAng;
        }
    }

    /// Chamar no RESPWAN do player (depois de setar a posi��o)
    public void ResetarYParaZero()
    {
        Vector3 p = transform.position;
        p.y = 0f;           // centro da sala no plano
        transform.position = p;
    }
}
