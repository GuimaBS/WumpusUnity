using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-300)]
[AddComponentMenu("Audio/UISfxManager")]
public class UISfxManager : MonoBehaviour
{
    public static UISfxManager I { get; private set; }

    [Header("Clipes padrão (opcionais)")]
    public AudioClip defaultHover, defaultClick, defaultBack;

    [Header("Áudio")]
    [Range(0f, 1f)] public float volume = 0.85f;
    public AudioMixerGroup mixerGroup;
    public bool ignoreListenerPause = true;

    private AudioSource _src;

    void Awake()
    {

        if (I != null && I != this)
        {
            Destroy(this);
            return;
        }

        I = this;

        _src = GetComponent<AudioSource>();
        if (_src == null) _src = gameObject.AddComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.loop = false;
        _src.spatialBlend = 0f; // 2D
        _src.ignoreListenerPause = ignoreListenerPause;
        if (mixerGroup) _src.outputAudioMixerGroup = mixerGroup;

        // Só persiste entre cenas se for OBJETO DE RAIZ
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning(
                "[UISfxManager] Coloque este componente em um GameObject de RAIZ (não filho do Canvas/Button) " +
                "para persistir entre cenas e evitar mover/remover UI.", this);
        }
    }

    public void PlayHover(AudioClip clip = null, float? vol = null, float pitch = 1f) => Play(clip ? clip : defaultHover, vol, pitch);
    public void PlayClick(AudioClip clip = null, float? vol = null, float pitch = 1f) => Play(clip ? clip : defaultClick, vol, pitch);
    public void PlayBack(AudioClip clip = null, float? vol = null, float pitch = 1f) => Play(clip ? clip : defaultBack, vol, pitch);

    private void Play(AudioClip clip, float? vol, float pitch)
    {
        if (!_src || !clip) return;
        _src.pitch = pitch;
        _src.PlayOneShot(clip, Mathf.Clamp01(vol ?? volume));
    }

    // Atalhos estáticos (opcionais)
    public static void SPlayClick(AudioClip clip = null, float? vol = null, float pitch = 1f) { if (I) I.PlayClick(clip, vol, pitch); }
    public static void SPlayHover(AudioClip clip = null, float? vol = null, float pitch = 1f) { if (I) I.PlayHover(clip, vol, pitch); }
    public static void SPlayBack(AudioClip clip = null, float? vol = null, float pitch = 1f) { if (I) I.PlayBack(clip, vol, pitch); }
}
