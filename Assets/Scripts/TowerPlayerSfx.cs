using UnityEngine;
using UnityEngine.Audio;

[AddComponentMenu("Audio/Tower Player SFX")]
public class TowerPlayerSfx : MonoBehaviour
{
    [Header("Mixer (opcional)")]
    public AudioMixerGroup mixerGroup;

    [Header("Volumes")]
    [Range(0f,1f)] public float volume = 0.9f;

    [Header("Clipes")]
    public AudioClip[] stepClips;
    public AudioClip rotateClip;
    public AudioClip shootClip;
    public AudioClip dieClip;
    public AudioClip collectGoldClip;
    public AudioClip blockedClip;
    public AudioClip floorUnlockedClip; // escada/andar liberado
    public AudioClip respawnClip;       // NOVO: respawn

    [Header("Variações")]
    public Vector2 stepPitchRange = new Vector2(0.95f, 1.05f);
    public float minStepInterval = 0.12f;

    private AudioSource _src;
    private float _lastStepTime;

    void Awake()
    {
        _src = gameObject.AddComponent<AudioSource>();
        _src.playOnAwake = false;
        _src.loop = false;
        _src.spatialBlend = 0f;
        _src.ignoreListenerPause = true;
        if (mixerGroup) _src.outputAudioMixerGroup = mixerGroup;
    }

    public void PlayStep()
    {
        if (Time.unscaledTime - _lastStepTime < minStepInterval) return;
        _lastStepTime = Time.unscaledTime;
        var c = Pick(stepClips);
        if (!c) return;
        _src.pitch = Random.Range(stepPitchRange.x, stepPitchRange.y);
        _src.PlayOneShot(c, volume);
    }

    public void PlayRotate()        => PlayOne(rotateClip);
    public void PlayShoot()         => PlayOne(shootClip);
    public void PlayDie()           => PlayOne(dieClip);
    public void PlayCollectGold()   => PlayOne(collectGoldClip);
    public void PlayBlocked()       => PlayOne(blockedClip);
    public void PlayFloorUnlocked() => PlayOne(floorUnlockedClip);
    public void PlayRespawn()       => PlayOne(respawnClip); // NOVO

    public void Anim_Step()         => PlayStep();

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
