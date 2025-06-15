using UnityEngine;
using EasyTransition;

public class TrocaDeCena : MonoBehaviour
{
    [Header("Configura��es de Transi��o")]
    public TransitionSettings transitionSettings;
    public float transitionTime = 1.2f;

    public void TrocarCena(string nomeCena)
    {
        TransitionManager.Instance().Transition(nomeCena, transitionSettings, transitionTime);
    }
}
