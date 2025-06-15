using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EasyTransition
{
    public class Transition : MonoBehaviour
    {
        [Header("Configura��es da Transi��o")]
        public TransitionSettings transitionSettings;

        [Header("Pain�is de Transi��o")]
        public Transform transitionPanelIN;
        public Transform transitionPanelOUT;

        [Header("Canvas da Transi��o")]
        public CanvasScaler transitionCanvas;

        [Header("Materiais")]
        public Material multiplyColorMaterial;
        public Material additiveColorMaterial;

        private bool hasTransitionTriggeredOnce;

        private void Start()
        {
            // Configurar a resolu��o do Canvas da transi��o
            if (transitionSettings != null)
                transitionCanvas.referenceResolution = transitionSettings.refrenceResolution;

            transitionPanelIN.gameObject.SetActive(false);
            transitionPanelOUT.gameObject.SetActive(false);

            // Verificar se o prefab est� atribu�do corretamente
            if (transitionSettings == null || transitionSettings.transitionIn == null)
            {
                Debug.LogError("[EasyTransition] Transition In n�o est� atribu�do no TransitionSettings!");
                return;
            }

            // Ativar painel e instanciar transi��o de entrada
            transitionPanelIN.gameObject.SetActive(true);
            GameObject transitionIn = Instantiate(transitionSettings.transitionIn);
            transitionIn.transform.SetParent(transitionPanelIN, false);

            transitionIn.AddComponent<CanvasGroup>().blocksRaycasts = transitionSettings.blockRaycasts;

            ApplyMaterialsAndSettings(transitionIn);
            ApplyTransformSettings(transitionIn);

            SceneManager.sceneLoaded += OnSceneLoad;
        }

        public void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            if (hasTransitionTriggeredOnce) return;

            transitionPanelIN.gameObject.SetActive(false);

            if (transitionSettings == null || transitionSettings.transitionOut == null)
            {
                Debug.LogError("[EasyTransition] Transition Out n�o est� atribu�do no TransitionSettings!");
                return;
            }

            transitionPanelOUT.gameObject.SetActive(true);
            GameObject transitionOut = Instantiate(transitionSettings.transitionOut);
            transitionOut.transform.SetParent(transitionPanelOUT, false);

            transitionOut.AddComponent<CanvasGroup>().blocksRaycasts = transitionSettings.blockRaycasts;

            ApplyMaterialsAndSettings(transitionOut);
            ApplyTransformSettings(transitionOut);

            hasTransitionTriggeredOnce = true;

            float destroyTime = transitionSettings.destroyTime;
            if (transitionSettings.autoAdjustTransitionTime && transitionSettings.transitionSpeed != 0)
                destroyTime = destroyTime / transitionSettings.transitionSpeed;

            Destroy(gameObject, destroyTime);
        }

        private void ApplyMaterialsAndSettings(GameObject transitionObj)
        {
            if (!transitionSettings.isCutoutTransition)
            {
                ApplyMaterialToImage(transitionObj.GetComponent<Image>());

                for (int i = 0; i < transitionObj.transform.childCount; i++)
                {
                    ApplyMaterialToImage(transitionObj.transform.GetChild(i).GetComponent<Image>());
                }
            }
        }

        private void ApplyMaterialToImage(Image image)
        {
            if (image == null) return;

            if (transitionSettings.colorTintMode == ColorTintMode.Multiply)
            {
                if (multiplyColorMaterial != null)
                {
                    image.material = multiplyColorMaterial;
                    image.material.SetColor("_Color", transitionSettings.colorTint);
                }
                else
                {
                    Debug.LogWarning("[EasyTransition] Multiply Material n�o est� atribu�do!");
                }
            }
            else if (transitionSettings.colorTintMode == ColorTintMode.Add)
            {
                if (additiveColorMaterial != null)
                {
                    image.material = additiveColorMaterial;
                    image.material.SetColor("_Color", transitionSettings.colorTint);
                }
                else
                {
                    Debug.LogWarning("[EasyTransition] Additive Material n�o est� atribu�do!");
                }
            }
        }

        private void ApplyTransformSettings(GameObject transitionObj)
        {
            if (transitionObj == null) return;

            Vector3 scale = transitionObj.transform.localScale;

            if (transitionSettings.flipX)
                scale.x *= -1;
            if (transitionSettings.flipY)
                scale.y *= -1;

            transitionObj.transform.localScale = scale;

            if (transitionObj.TryGetComponent(out Animator parentAnim) && transitionSettings.transitionSpeed != 0)
            {
                parentAnim.speed = transitionSettings.transitionSpeed;
            }
            else
            {
                for (int c = 0; c < transitionObj.transform.childCount; c++)
                {
                    if (transitionObj.transform.GetChild(c).TryGetComponent(out Animator childAnim) && transitionSettings.transitionSpeed != 0)
                    {
                        childAnim.speed = transitionSettings.transitionSpeed;
                    }
                }
            }
        }
    }
}
