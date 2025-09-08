using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class UIButtonSfxHook : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Overrides (opcional)")]
    public AudioClip hoverOverride;
    public AudioClip clickOverride;

    [Header("Fallback local se manager faltar (raro)")]
    public bool localFallbackIfNoManager = true;
    [Range(0f, 1f)] public float localVolume = 0.85f;

    private AudioSource _localSrc; // usado só no fallback

    // Chame este método pelo Button.onClick no Inspector (100% garantido)
    public void OnUiClick()
    {
        if (UISfxManager.I != null) UISfxManager.I.PlayClick(clickOverride);
        else PlayLocal(clickOverride);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Em mobile, hover geralmente não dispara — tudo bem.
        if (UISfxManager.I != null) UISfxManager.I.PlayHover(hoverOverride);
        else PlayLocal(hoverOverride);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (UISfxManager.I != null) UISfxManager.I.PlayClick(clickOverride);
        else PlayLocal(clickOverride);
    }

    private void PlayLocal(AudioClip clip)
    {
        if (!localFallbackIfNoManager || clip == null) return;
        if (_localSrc == null)
        {
            _localSrc = gameObject.AddComponent<AudioSource>();
            _localSrc.playOnAwake = false;
            _localSrc.loop = false;
            _localSrc.spatialBlend = 0f;
        }
        _localSrc.PlayOneShot(clip, localVolume);
    }
}
