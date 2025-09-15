using UnityEngine;
using UnityEngine.Audio;

[AddComponentMenu("Audio/Tower Player SFX")]
public class TowerPlayerSfx : MonoBehaviour
{
    [Header("Mixer (opcional)")]
    public AudioMixerGroup mixerGroup;

    [Header("Volumes")]
    [Range(0f, 1f)] public float volume = 0.9f;

    [Header("Clipes – Básicos")]
    public AudioClip[] stepClips;
    public AudioClip rotateClip;
    public AudioClip shootClip;
    public AudioClip dieClip;
    public AudioClip collectGoldClip;
    public AudioClip blockedClip;
    public AudioClip floorUnlockedClip; // escada/andar liberado
    public AudioClip respawnClip;

    [Header("Clipes – Especiais (Wumpus)")]
    public AudioClip roarKillClip;      // rugido ao matar o player
    public AudioClip roarAmbientClip;   // rugido baixo quando há fedor por perto

    [Header("Volumes Especiais")]
    [Range(0f, 1f)] public float roarKillVolume = 0.9f;
    [Range(0f, 1f)] public float roarAmbientVolume = 0.35f;

    [Header("Variações (passo)")]
    public Vector2 stepPitchRange = new Vector2(0.95f, 1.05f);
    public float minStepInterval = 0.12f;

    private AudioSource _src;
    private float _lastStepTime;

    void Awake()
    {
        _src = GetComponent<AudioSource>();
        if (!_src) _src = gameObject.AddComponent<AudioSource>();

        _src.playOnAwake = false;
        _src.loop = false;
        _src.spatialBlend = 0f;             // 2D
        _src.ignoreListenerPause = true;
        if (mixerGroup) _src.outputAudioMixerGroup = mixerGroup;
    }

    // --------- BÁSICOS ----------
    public void PlayStep()
    {
        if (Time.unscaledTime - _lastStepTime < minStepInterval) return;
        _lastStepTime = Time.unscaledTime;

        var c = Pick(stepClips);
        if (!c) return;

        float oldPitch = _src.pitch;
        _src.pitch = Random.Range(stepPitchRange.x, stepPitchRange.y);
        _src.PlayOneShot(c, volume);
        _src.pitch = oldPitch;
    }
    public void Anim_Step() => PlayStep();

    public void PlayRotate() => PlayOne(rotateClip);
    public void PlayShoot() => PlayOne(shootClip);
    public void PlayDie() => PlayOne(dieClip);
    public void PlayCollectGold() => PlayOne(collectGoldClip);
    public void PlayBlocked() => PlayOne(blockedClip);
    public void PlayFloorUnlocked() => PlayOne(floorUnlockedClip);
    public void PlayRespawn() => PlayOne(respawnClip);

    // --------- ESPECIAIS (Wumpus) ----------
    public void PlayWumpusRoarKill() => PlayOne(roarKillClip, roarKillVolume);
    public void PlayWumpusRoarAmbient() => PlayOne(roarAmbientClip, roarAmbientVolume);

    // --------- HELPERS ----------
    private void PlayOne(AudioClip c, float volScale = 1f, float pitch = 1f)
    {
        if (!c) return;
        float oldPitch = _src.pitch;
        _src.pitch = pitch;
        _src.PlayOneShot(c, Mathf.Clamp01(volume * volScale));
        _src.pitch = oldPitch;
    }

    private AudioClip Pick(AudioClip[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        return arr[Random.Range(0, arr.Length)];
    }
}
