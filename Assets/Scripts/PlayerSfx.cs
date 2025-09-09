using UnityEngine;
using UnityEngine.Audio;

[AddComponentMenu("Audio/Player SFX")]
public class PlayerSfx : MonoBehaviour
{
    [Header("Mixer (opcional)")]
    public AudioMixerGroup mixerGroup;

    [Header("Volumes")]
    [Range(0f,1f)] public float volume = 0.9f;

    [Header("Clipes")]
    public AudioClip[] stepClips;       // passos
    public AudioClip rotateClip;        // girar
    public AudioClip shootClip;         // atirar
    public AudioClip dieClip;           // morrer
    public AudioClip collectGoldClip;   // coletar ouro
    public AudioClip blockedClip;       // bloqueado
    public AudioClip respawnClip;       // NOVO: respawn

    [Header("Variações")]
    public Vector2 stepPitchRange = new Vector2(0.95f, 1.05f);
    public bool ignoreListenerPause = true; // tocar mesmo em pause

    private AudioSource _src;
    private float _lastStep;

    void Awake()
    {
        _src = gameObject.AddComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.loop = false;
        _src.spatialBlend = 0f;
        _src.ignoreListenerPause = ignoreListenerPause;
        if (mixerGroup) _src.outputAudioMixerGroup = mixerGroup;
    }

    public void PlayStep()
    {
        var c = Pick(stepClips);
        if (!c) return;
        _src.pitch = Random.Range(stepPitchRange.x, stepPitchRange.y);
        _src.PlayOneShot(c, volume);
    }

    public void PlayRotate()      => PlayOne(rotateClip);
    public void PlayShoot()       => PlayOne(shootClip);
    public void PlayDie()         => PlayOne(dieClip);
    public void PlayCollectGold() => PlayOne(collectGoldClip);
    public void PlayBlocked()     => PlayOne(blockedClip);

    // NOVO: som de respawn
    public void PlayRespawn()     => PlayOne(respawnClip);

    // Se usar animação de caminhada:
    public void Anim_Step()       => PlayStep();

    private void PlayOne(AudioClip c, float pitch = 1f)
    {
        if (!c) return;
        _src.pitch = pitch;
        _src.PlayOneShot(c, volume);
    }

    private AudioClip Pick(AudioClip[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        return arr[Random.Range(0, arr.Length)];
    }
}
