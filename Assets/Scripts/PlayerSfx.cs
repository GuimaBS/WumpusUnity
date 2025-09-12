using UnityEngine;
using UnityEngine.Audio;

[AddComponentMenu("Audio/Player SFX")]
public class PlayerSfx : MonoBehaviour
{
    [Header("Mixer (opcional)")]
    public AudioMixerGroup mixerGroup;

    [Header("Volumes")]
    [Range(0f, 1f)] public float volume = 0.9f;   // volume mestre

    [Header("Clipes – Básicos")]
    public AudioClip[] stepClips;       // passos
    public AudioClip rotateClip;        // girar
    public AudioClip shootClip;         // atirar
    public AudioClip dieClip;           // morrer (genérico)
    public AudioClip collectGoldClip;   // coletar ouro
    public AudioClip blockedClip;       // bloqueado
    public AudioClip respawnClip;       // respawn

    [Header("Clipes – Eventos Especiais")]
    public AudioClip victorySfx;        // vitória
    public AudioClip roarclip;          // rugido do wumpus (morte)
    public AudioClip roarnearclip;      // rugido baixo (sala com fedor)

    [Header("Variações")]
    public Vector2 stepPitchRange = new Vector2(0.95f, 1.05f);
    public bool ignoreListenerPause = true; // tocar mesmo em pause

    [Header("Volumes Especiais")]
    [Range(0f, 1f)] public float victoryVolume = 1f;
    [Range(0f, 1f)] public float roarKillVolume = 0.9f;
    [Range(0f, 1f)] public float roarAmbientVolume = 0.35f;

    private AudioSource _src;

    void Awake()
    {
        _src = GetComponent<AudioSource>();
        if (!_src) _src = gameObject.AddComponent<AudioSource>();

        _src.playOnAwake = false;
        _src.loop = false;
        _src.spatialBlend = 0f;                 // 2D
        _src.ignoreListenerPause = ignoreListenerPause;
        if (mixerGroup) _src.outputAudioMixerGroup = mixerGroup;
    }

    // ----------------- BÁSICOS -----------------
    public void PlayStep()
    {
        var c = Pick(stepClips);
        if (!c) return;
        float oldPitch = _src.pitch;
        _src.pitch = Random.Range(stepPitchRange.x, stepPitchRange.y);
        _src.PlayOneShot(c, volume);
        _src.pitch = oldPitch;
    }

    // se estiver usando evento de animação:
    public void Anim_Step() => PlayStep();

    public void PlayRotate() => PlayOne(rotateClip);
    public void PlayShoot() => PlayOne(shootClip);
    public void PlayDie() => PlayOne(dieClip);
    public void PlayCollectGold() => PlayOne(collectGoldClip);
    public void PlayBlocked() => PlayOne(blockedClip);
    public void PlayRespawn() => PlayOne(respawnClip);

    // ----------------- ESPECIAIS -----------------
    public void PlayWin() => PlayOne(victorySfx, victoryVolume);
    public void PlayWumpusRoarKill() => PlayOne(roarclip, roarKillVolume);
    public void PlayWumpusRoarAmbient() => PlayOne(roarnearclip, roarAmbientVolume);

    // ----------------- HELPERS -----------------
    private void PlayOne(AudioClip c, float volumeScale = 1f, float pitch = 1f)
    {
        if (!c) return;
        float old = _src.pitch;
        _src.pitch = pitch;
        _src.PlayOneShot(c, Mathf.Clamp01(volume * volumeScale));
        _src.pitch = old;
    }

    private AudioClip Pick(AudioClip[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        return arr[Random.Range(0, arr.Length)];
    }
}
