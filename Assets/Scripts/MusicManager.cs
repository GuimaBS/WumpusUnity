using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[AddComponentMenu("Audio/BGM Manager (Menus + Gameplay)")]
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Clipes")]
    public AudioClip menuMusic;
    public AudioClip gameplayMusic;

    [Header("Volumes")]
    [Range(0f, 1f)] public float menuVolume = 0.6f;
    [Range(0f, 1f)] public float gameplayVolume = 0.7f;

    [Header("Cena específica")]
    [Tooltip("Trilha exclusiva para a cena de buildIndex = 2")]
    public AudioClip scene2Music;                      // NOVO
    [Range(0f, 1f)] public float scene2Volume = 0.7f;   // NOVO

    [Header("Transição")]
    [Tooltip("Duração do crossfade (segundos)")]
    public float crossfadeDuration = 0.8f;
    [Tooltip("Se verdadeiro, retoma do ponto onde parou ao voltar para a mesma trilha")]
    public bool resumeFromLastPosition = true;

    [Header("Cenas")]
    public List<int> menuScenes = new List<int> { 0, 1, 3, 5, 7 };
    public List<int> gameplayScenes = new List<int> { 4, 6 };

    // ---- internos ----
    private AudioSource _a, _b;          // duas fontes para crossfade
    private AudioSource _active;          // a fonte atualmente audível
    private Coroutine _xfade;
    private float _menuTime;              // posição salva da música de menu
    private float _gameplayTime;          // posição salva da música de gameplay
    private float _scene2Time;            // >>> NOVO: posição salva da música da cena 2

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _a = gameObject.AddComponent<AudioSource>();
        _b = gameObject.AddComponent<AudioSource>();
        ConfigSource(_a);
        ConfigSource(_b);

        SceneManager.sceneLoaded += OnSceneLoaded;

        // Decide imediatamente pela cena atual (útil ao dar Play direto numa cena)
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void ConfigSource(AudioSource s)
    {
        s.playOnAwake = false;
        s.loop = true;
        s.spatialBlend = 0f;           // 2D
        s.ignoreListenerPause = true;  // toca mesmo com Time.timeScale=0
        s.volume = 0f;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        int idx = scene.buildIndex;

        // >>> NOVO: prioridade para a cena 2
        if (idx == 2 && scene2Music != null)
        {
            SwitchToScene2();
            return;
        }

        if (menuScenes.Contains(idx))
            SwitchToMenu();
        else if (gameplayScenes.Contains(idx))
            SwitchToGameplay();
        else
            FadeOutAndStopAll(); // cenas neutras (se houver)
    }

    // -------- switches públicos (se quiser chamar manualmente) --------
    public void SwitchToMenu() => SwitchToClip(menuMusic, menuVolume, ref _menuTime);
    public void SwitchToGameplay() => SwitchToClip(gameplayMusic, gameplayVolume, ref _gameplayTime);

    // >>> NOVO: Switch específico para a cena 2
    public void SwitchToScene2() => SwitchToClip(scene2Music, scene2Volume, ref _scene2Time);

    // ---------------- núcleo de troca com crossfade -------------------
    private void SwitchToClip(AudioClip clip, float targetVol, ref float savedTime)
    {
        if (clip == null) return;

        // Se já estamos nessa trilha com volume alvo, nada a fazer
        if (_active != null && _active.clip == clip)
        {
            // garante volume alvo (pode estar baixo se veio de um fade anterior)
            StartXFade(_active, targetVol, keepPlaying: true);
            return;
        }

        // Escolhe a fonte "próxima" (a que NÃO está ativa)
        AudioSource next = (_active == _a) ? _b : _a;

        // Prepara a fonte próxima com o clip correto
        PrepareSource(next, clip, resumeFromLastPosition ? savedTime : 0f);

        // Inicia o crossfade
        StartXFade(next, targetVol, keepPlaying: false);
    }

    private void PrepareSource(AudioSource src, AudioClip clip, float startTime)
    {
        if (src.clip != clip) src.clip = clip;

        // Ajuste seguro do time (evita erro se >= length)
        float t = Mathf.Clamp(startTime, 0f, Mathf.Max(0f, clip.length - 0.01f));
        src.time = t;

        if (!src.isPlaying) src.Play();
        src.volume = 0f;
    }

    private void StartXFade(AudioSource next, float nextTargetVol, bool keepPlaying)
    {
        if (_xfade != null) StopCoroutine(_xfade);
        _xfade = StartCoroutine(CrossfadeRoutine(next, nextTargetVol, keepPlaying));
    }

    private IEnumerator CrossfadeRoutine(AudioSource next, float nextTargetVol, bool keepPlaying)
    {
        AudioSource prev = _active;
        float dur = Mathf.Max(0.01f, crossfadeDuration);
        float t = 0f;

        float prevStart = prev ? prev.volume : 0f;
        float nextStart = next.volume;

        // Garante que a próxima está tocando
        if (!next.isPlaying) next.Play();

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);

            if (prev) prev.volume = Mathf.Lerp(prevStart, 0f, k);
            if (next) next.volume = Mathf.Lerp(nextStart, nextTargetVol, k);

            yield return null;
        }

        // Finaliza volumes
        if (prev)
        {
            // Salva posição antes de parar
            SaveTime(prev);
            if (!keepPlaying) { prev.Stop(); prev.volume = 0f; }
        }

        if (next) next.volume = nextTargetVol;

        _active = next;
        _xfade = null;
    }

    private void SaveTime(AudioSource src)
    {
        if (src == null || src.clip == null) return;
        if (src.clip == menuMusic) _menuTime = src.time;
        else if (src.clip == gameplayMusic) _gameplayTime = src.time;
        else if (src.clip == scene2Music) _scene2Time = src.time; // >>> NOVO
    }

    private void FadeOutAndStopAll()
    {
        if (_xfade != null) StopCoroutine(_xfade);
        SaveTime(_a);
        SaveTime(_b);
        if (_a.isPlaying) _a.Stop();
        if (_b.isPlaying) _b.Stop();
        _a.volume = 0f;
        _b.volume = 0f;
        _active = null;
    }
}
