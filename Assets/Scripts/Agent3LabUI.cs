using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class Agent3LabUI : MonoBehaviour
{
    [Header("Raiz do Painel (Janela do Lab)")]
    [SerializeField] GameObject panelRoot;

    [Header("Botões")]
    [SerializeField] Button btnOpen;
    [SerializeField] Button btnClose;
    [SerializeField] Button btnEvoluir;        // evoluir N gerações
    [SerializeField] Button btnTorneioTop2;    // 1 geração com top2
    [SerializeField] Button btnLancar;         // lançar best em cena
    [SerializeField] Button btnEliminar;       // destruir runtimes em cena

    [Header("Inputs (TMP)")]
    [SerializeField] TMP_InputField inpPop;
    [SerializeField] TMP_InputField inpGens;
    [SerializeField] TMP_InputField inpCrossover;   // 0–100 (%)
    [SerializeField] TMP_InputField inpMutacao;     // 0–100 (%)
    [SerializeField] TMP_InputField inpMutStd;      // e.g. 0.15
    [SerializeField] TMP_InputField inpElitismo;    // int
    [SerializeField] TMP_InputField inpTorneioK;    // int
    [SerializeField] TMP_InputField inpStepBudget;  // int
    [SerializeField] TMP_InputField inpSeed;        // opcional

    [Header("Tabela/Ranking")]
    [SerializeField] Transform tableContent;
    [SerializeField] Agent3LabRow rowPrefab;
    [SerializeField] ScrollRect scrollRect;

    [Header("Spawner (GameScene)")]
    [Tooltip("Arraste aqui o Agent3RuntimeSpawner se ele estiver na mesma cena; se estiver em outra cena, este campo pode ficar vazio.")]
    public Agent3RuntimeSpawner spawner;

    Agent3GA ga;
    readonly List<Agent3LabRow> rows = new();
    readonly Dictionary<int, Agent3GA.GenStats> statsByGen = new();

    void Awake()
    {
        // Fallbacks
        if (!panelRoot) panelRoot = GameObject.Find("Panel_LabAgente3");
        if (!btnOpen) btnOpen = GameObject.Find("BotaoAbrirLab")?.GetComponent<Button>();
        if (!btnClose) btnClose = GameObject.Find("BotaoFecharLab")?.GetComponent<Button>();
        if (!btnEvoluir) btnEvoluir = GameObject.Find("BtnEvoluir")?.GetComponent<Button>();
        if (!btnTorneioTop2) btnTorneioTop2 = GameObject.Find("BtnTorneioTop2")?.GetComponent<Button>();
        if (!btnLancar) btnLancar = GameObject.Find("BtnLancarNoGrid")?.GetComponent<Button>();
        if (!btnEliminar) btnEliminar = GameObject.Find("BtnEliminarAgentes")?.GetComponent<Button>();

        if (panelRoot) panelRoot.SetActive(false);

        if (btnOpen) btnOpen.onClick.AddListener(Abrir);
        if (btnClose) btnClose.onClick.AddListener(Fechar);
        if (btnEvoluir) btnEvoluir.onClick.AddListener(OnEvoluir);
        if (btnTorneioTop2) btnTorneioTop2.onClick.AddListener(OnTorneioTop2);
        if (btnLancar) btnLancar.onClick.AddListener(OnLancarMelhor);
        if (btnEliminar) btnEliminar.onClick.AddListener(EliminarAgentes);

#if UNITY_2023_1_OR_NEWER
        ga = Agent3GA.instancia ?? UnityEngine.Object.FindFirstObjectByType<Agent3GA>();
#else
        ga = Agent3GA.instancia ?? UnityEngine.Object.FindObjectOfType<Agent3GA>();
#endif

        if (!scrollRect) scrollRect = GetComponentInChildren<ScrollRect>(true);

        if (!ga)
        {
            var go = new GameObject("Agent3GA_Auto");
            ga = go.AddComponent<Agent3GA>(); // tem DontDestroyOnLoad no Awake dele
            // valores padrão seguros:
            ga.populationSize = 30;
            ga.crossoverRate = 0.85f;
            ga.mutationRate = 0.05f;
            ga.mutationStd = 0.15f;
            ga.elitism = 1;
            ga.tournamentK = 3;
            ga.stepBudgetPerEval = 300;
        }
    }

    void OnEnable()
    {
#if UNITY_2023_1_OR_NEWER
        if (!ga) ga = Agent3GA.instancia ?? UnityEngine.Object.FindFirstObjectByType<Agent3GA>();
#else
        if (!ga) ga = Agent3GA.instancia ?? UnityEngine.Object.FindObjectOfType<Agent3GA>();
#endif
        if (ga)
        {
            ga.OnGenerationAdvanced -= OnGenAdvanced; // evita duplicar
            ga.OnGenerationAdvanced += OnGenAdvanced;
        }
    }

    void OnDisable()
    {
        if (ga) ga.OnGenerationAdvanced -= OnGenAdvanced;
    }

    public void Abrir()
    {
        if (!panelRoot) return;
        panelRoot.SetActive(true);
        SincronizarInputsComGA();
        StartCoroutine(ScrollTopNextFrame());
    }

    public void Fechar()
    {
        if (!panelRoot) return;
        panelRoot.SetActive(false);
    }

    void SincronizarInputsComGA()
    {
        if (!ga) return;
        SetIf(inpPop, ga.populationSize);
        SetIf(inpStepBudget, ga.stepBudgetPerEval);
        SetIf(inpCrossover, Mathf.RoundToInt(ga.crossoverRate * 100f));
        SetIf(inpMutacao, Mathf.RoundToInt(ga.mutationRate * 100f));
        SetIf(inpMutStd, ga.mutationStd);
        SetIf(inpElitismo, ga.elitism);
        SetIf(inpTorneioK, ga.tournamentK);
        // inpGens e inpSeed ficam a critério do usuário.
    }

    void AplicarUIAoGA()
    {
        if (!ga) return;

        ga.populationSize = ReadIntOrKeep(inpPop, ga.populationSize, 2, 50);
        ga.stepBudgetPerEval = ReadIntOrKeep(inpStepBudget, ga.stepBudgetPerEval, 50, 2000);

        ga.crossoverRate = ReadPctOrKeep(inpCrossover, ga.crossoverRate * 100f) / 100f;
        ga.mutationRate = ReadPctOrKeep(inpMutacao, ga.mutationRate * 100f) / 100f;

        ga.mutationStd = ReadFloatOrKeep(inpMutStd, ga.mutationStd, 0f, 2f);
        ga.elitism = ReadIntOrKeep(inpElitismo, ga.elitism, 0, 10);
        ga.tournamentK = ReadIntOrKeep(inpTorneioK, ga.tournamentK, 1, 10);
        ga.evalSeed = ReadIntOrKeep(inpSeed, ga.evalSeed, int.MinValue, int.MaxValue);
    }

    void OnEvoluir()
    {
        if (!ga) return;

        AplicarUIAoGA();

        int gens = 5;
        if (inpGens && int.TryParse(inpGens.text, out var g) && g >= 1 && g <= 1000)
            gens = g;

        int genAntes = ga.generation;
        ga.RunGenerations(gens);
        Debug.Log($"[LabUI] Evoluiu {gens} geração(ões). gen: {genAntes} -> {ga.generation}");
    }

    void OnTorneioTop2()
    {
        if (!ga) return;
        AplicarUIAoGA();
        ga.RunOneGenerationTop2();
    }

    void OnLancarMelhor()
    {
        if (spawner != null)
        {
            spawner.SpawnChampion();
        }
        else
        {
            Debug.LogWarning("[Agent3LabUI] Spawner não atribuído nesta cena. Carregue a GameScene com Spawner (autoSpawn) ou arraste a referência aqui.");
        }
    }

#if UNITY_2023_1_OR_NEWER
    void EliminarAgentes()
    {
        var agentes = Object.FindObjectsByType<Agent3Driver>(FindObjectsSortMode.None);
        for (int i = 0; i < agentes.Length; i++) Destroy(agentes[i].gameObject);
    }
#else
    void EliminarAgentes()
    {
        var agentes = Object.FindObjectsOfType<Agent3Driver>();
        for (int i = 0; i < agentes.Length; i++) Destroy(agentes[i].gameObject);
    }
#endif

    // ranking: mantém linhas ordenadas por Best desc
    void OnGenAdvanced(Agent3GA.GenStats s)
    {
        if (!rowPrefab || !tableContent) return;

        // Se recomeçou (gen 0), limpa tudo
        if (s.gen == 0 && statsByGen.Count > 0)
        {
            statsByGen.Clear();
            foreach (Transform c in tableContent) Destroy(c.gameObject);
            rows.Clear();
        }

        statsByGen[s.gen] = s;

        var ordered = statsByGen.Values
            .OrderByDescending(v => v.bestGen)   // rank principal
            .ThenByDescending(v => v.avg)        // desempate 1
            .ThenByDescending(v => v.gen)        // desempate 2
            .ToList();

        // Preserva posição do usuário no scroll
        float vpos = scrollRect ? scrollRect.verticalNormalizedPosition : 1f;

        // Rebuild total (simples e robusto)
        foreach (Transform c in tableContent) Destroy(c.gameObject);
        rows.Clear();

        for (int i = 0; i < ordered.Count; i++)
        {
            var row = Instantiate(rowPrefab, tableContent);
            row.gameObject.SetActive(true);
            row.Set(i + 1, ordered[i]);
            rows.Add(row);
        }

        if (scrollRect) scrollRect.verticalNormalizedPosition = vpos;
    }

    System.Collections.IEnumerator ScrollTopNextFrame()
    {
        yield return null; // espera 1 frame
        if (tableContent is RectTransform rt)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        if (scrollRect)
            scrollRect.verticalNormalizedPosition = 1f; // topo
    }

    static int ReadIntOrKeep(TMP_InputField f, int current, int min, int max)
    {
        if (!f) return current;
        var txt = f.text?.Trim();
        if (string.IsNullOrEmpty(txt)) return current;
        if (!int.TryParse(txt, out var x)) return current;
        return Mathf.Clamp(x, min, max);
    }

    static float ReadFloatOrKeep(TMP_InputField f, float current, float min, float max)
    {
        if (!f) return current;
        var txt = f.text?.Trim();
        if (string.IsNullOrEmpty(txt)) return current;
        if (!float.TryParse(txt, out var x)) return current;
        return Mathf.Clamp(x, min, max);
    }

    static float ReadPctOrKeep(TMP_InputField f, float current)  // 0..100
    {
        if (!f) return current;
        var txt = f.text?.Trim();
        if (string.IsNullOrEmpty(txt)) return current;
        if (!float.TryParse(txt, out var x)) return current;
        return Mathf.Clamp(x, 0f, 100f);
    }

    // ------- utils -------
    static void SetIf(TMP_InputField f, int v) { if (f) f.text = v.ToString(); }
    static void SetIf(TMP_InputField f, float v) { if (f) f.text = v.ToString("0.##"); }
}