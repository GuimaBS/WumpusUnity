using System.Collections.Generic;
using UnityEngine;

public class PlayerGridGenerator : MonoBehaviour
{
    public static PlayerGridGenerator instancia;
    public static System.Action OnMapaGerado;

    [Header("Prefabs das Salas")]
    public GameObject salaPrefab;
    public GameObject salaComPocoPrefab;

    [Header("Prefab da Linha de Chegada")]
    public GameObject linhaDeChegadaPrefab;

    [Header("Prefabs dos Personagens")]
    public GameObject prefabArqueiro;
    public GameObject prefabAmazona;

    [Header("Offset Específico por Personagem")]
    public Vector3 offsetArqueiro = Vector3.zero;
    public Vector3 offsetAmazona = Vector3.zero;

    [Header("Prefab de Sensações")]
    public GameObject prefabBrisa;
    public GameObject prefabFedor;
    public GameObject prefabBrilho;

    [Header("Prefab do Wumpus e do Ouro")]
    public GameObject prefabWumpus;
    public GameObject prefabOuro;
    public float rotacaoYWumpus = 0f;
    public float rotacaoYOuro = 0f;

    [Header("Prefab de Bloqueio")]
    public GameObject prefabBloqueio;

    [Header("Offset dos Bloqueios")]
    public Vector3 offsetBloqueioX = new Vector3(5f, 0f, 0f);  // Direita/esquerda
    public Vector3 offsetBloqueioZ = new Vector3(0f, 0f, 5f);

    [Header("Configuração do Mapa")]
    public float espacoEntreSalas = 10f;
    public Transform paiDasSalas;
    public Transform paiDoPlayer;

    [Header("Offset para Centralizar na Sala")]
    public Vector3 offsetCentroSala = new Vector3(5, 0, 5);

    [Header("Mapa Gerado")]
    public Dictionary<Vector2Int, GameObject> mapaGerado = new Dictionary<Vector2Int, GameObject>();

    [Header("Mapa Lógico")]
    public Dictionary<Vector2Int, TileInfo> gridInfo = new Dictionary<Vector2Int, TileInfo>();

    public Vector2Int posicaoWumpus;
    public Vector2Int posicaoOuro;
    public bool wumpusMorto => wumpusMortos >= totalWumpusAlvo;
    public bool ouroColetado => ourosColetadosCount >= totalOurosAlvo;
    public bool linhaInstanciada = false;
    public List<Vector2Int> posicoesWumpus = new List<Vector2Int>();
    public List<Vector2Int> posicoesOuro = new List<Vector2Int>();

    private int totalWumpusAlvo = 1;
    private int totalOurosAlvo = 1;
    private int wumpusMortos = 0;
    private int ourosColetadosCount = 0;

    [System.Serializable]
    public class TileInfo
    {
        public bool temPoco = false;
        public bool temBrisa = false;
        public bool temFedor = false;
        public bool temOuro = false;
        public bool temWumpus;
        public bool foiVisitada = false;
    }

    private void Awake()
    {
        if (instancia == null) instancia = this;
        else { Destroy(gameObject); return; }

        tamanhoX = PlayerPrefs.GetInt("mapX");
        tamanhoY = PlayerPrefs.GetInt("mapY");

        Debug.Log($"Gerando mapa {tamanhoX}x{tamanhoY}");

        if (tamanhoX > 10 || tamanhoY > 10)
        {
            totalWumpusAlvo = 2;
            totalOurosAlvo = 2;
        }
        else
        {
            totalWumpusAlvo = 1;
            totalOurosAlvo = 1;
        }


        if (MapaVisualPlayer.instancia != null)
        {
            MapaVisualPlayer.instancia.InicializarMapa(tamanhoX, tamanhoY);
        }

        GerarMapa();
        GarantirSalaSeguraEm00();
        AplicarBrisaNosPocos();
        InstanciarVariosWumpus(totalWumpusAlvo);
        InstanciarVariosOuros(totalOurosAlvo);
        SpawnarPlayer();
        TimerPontuacaoController.Reiniciar();

        OnMapaGerado?.Invoke();

    }

    private int tamanhoX;
    private int tamanhoY;

    public void GerarMapa()
    {
        LimparMapa();

        for (int x = 0; x < tamanhoX; x++)
        {
            for (int y = 0; y < tamanhoY; y++)
            {
                Vector3 pos = new Vector3(x * espacoEntreSalas, 0, y * espacoEntreSalas);
                Vector2Int gridPos = new Vector2Int(x, y);

                GameObject sala;
                bool temPoco = Random.value < 0.2f;

                if (temPoco)
                {
                    sala = Instantiate(salaComPocoPrefab, pos, Quaternion.identity, paiDasSalas);
                    sala.tag = "SalaP";
                    
                }
                else
                {
                    sala = Instantiate(salaPrefab, pos, Quaternion.identity, paiDasSalas);
                }

                sala.name = $"Sala ({x},{y})";
                mapaGerado.Add(gridPos, sala);

                TileInfo info = new TileInfo { temPoco = temPoco };
                gridInfo.Add(gridPos, info);

                if (temPoco)
                RegistrarSensacao(gridPos, "poco");
            }
        }

        GerarBloqueiosDeBorda();
    }

    private void GerarBloqueiosDeBorda()
    {
        foreach (var kvp in mapaGerado)
        {
            Vector2Int pos = kvp.Key;
            GameObject sala = kvp.Value;

            Vector3 salaPos = sala.transform.position;

            Vector2Int[] direcoes = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            Vector3[] rotacoes = {
            new Vector3(0, 0, 0),    // cima (Z+)
            new Vector3(0, 180, 0),  // baixo (Z-)
            new Vector3(0, -90, 0),  // esquerda (X-)
            new Vector3(0, 90, 0)    // direita (X+)
        };

            for (int i = 0; i < direcoes.Length; i++)
            {
                Vector2Int direcao = direcoes[i];
                Vector2Int destino = pos + direcao;

                if (!gridInfo.ContainsKey(destino))
                {
                    Vector3 offset;
                    if (direcao == Vector2Int.left || direcao == Vector2Int.right)
                        offset = direcao == Vector2Int.left ? -offsetBloqueioX : offsetBloqueioX;
                    else
                        offset = direcao == Vector2Int.down ? -offsetBloqueioZ : offsetBloqueioZ;

                    GameObject bloqueio = Instantiate(
                        prefabBloqueio,
                        salaPos + offset,
                        Quaternion.Euler(rotacoes[i]),
                        sala.transform
                    );
                    bloqueio.name = $"Bloqueio_{pos}_{direcao}";
                }
            }
        }
    }


    public void EliminarWumpusNaPosicao(Vector2Int posicao)
    {
        if (!gridInfo.ContainsKey(posicao))
        {
            Debug.LogWarning($"[PlayerGrid] Tentativa de eliminar Wumpus em posição inválida: {posicao}");
            return;
        }

        // Só prossegue se realmente havia Wumpus aqui
        if (!gridInfo[posicao].temWumpus)
        {
            Debug.Log($"[PlayerGrid] Não há Wumpus na sala {posicao} para eliminar.");
            return;
        }

        // Estado lógico
        gridInfo[posicao].temWumpus = false;
        posicoesWumpus.Remove(posicao);
        RemoverSensacao(posicao, "wumpus");

        // Visual: destrói o objeto wumpus presente nesta sala
        if (mapaGerado.TryGetValue(posicao, out GameObject sala))
        {
            foreach (Transform child in sala.transform)
            {
                if (child.CompareTag("wumpus"))
                {
                    Destroy(child.gameObject);
                    Debug.Log($"[PlayerGrid] Wumpus destruído visualmente na sala {posicao}");
                    break;
                }
            }
        }

        // Recalcular fedor nas células afetadas (somente nas adjacentes ao morto)
        RecalcularFedorAdjacencias(posicao);

        // UI/pontuação e contagem
        UIManager.instancia?.AtualizarDWumpus(1); // mantém seu padrão
        wumpusMortos++;

        Debug.Log($"[PlayerGrid] Wumpus eliminado na {posicao}. Restantes: {totalWumpusAlvo - wumpusMortos}");

        VerificarCondicaoParaLinhaDeChegada();
    }

    private void RecalcularFedorAdjacencias(Vector2Int origemMorto)
    {
        // Para cada adjacente à posição onde o Wumpus morreu,
        // mantém fedor se ainda houver ALGUM outro wumpus adjacente a essa célula.
        foreach (var adj in Adjacentes4(origemMorto))
        {
            if (!gridInfo.ContainsKey(adj)) continue;

            bool deveTerFedor = false;
            foreach (var wPos in posicoesWumpus)
            {
                // fedor aparece nas células adjacentes ao wumpus vivo
                if (wPos == adj + Vector2Int.up ||
                    wPos == adj + Vector2Int.down ||
                    wPos == adj + Vector2Int.left ||
                    wPos == adj + Vector2Int.right)
                {
                    deveTerFedor = true;
                    break;
                }
            }

            gridInfo[adj].temFedor = deveTerFedor;

            if (mapaGerado.TryGetValue(adj, out var salaAdj))
            {
                var fedorTF = salaAdj.transform.Find("Fedor");
                if (deveTerFedor)
                {
                    if (fedorTF == null)
                    {
                        Vector3 posFedor = salaAdj.transform.position + new Vector3(0, 1.5f, 0);
                        Instantiate(prefabFedor, posFedor, Quaternion.identity, salaAdj.transform).name = "Fedor";
                    }
                    RegistrarSensacao(adj, "fedor");
                }
                else
                {
                    if (fedorTF != null) Destroy(fedorTF.gameObject);
                    RemoverSensacao(adj, "fedor");
                }
            }
        }
    }

    public void RemoverParticulasDeFedor(Vector2Int posicao)
    {
        if (mapaGerado.TryGetValue(posicao, out GameObject sala))
        {
            Transform fedorTransform = sala.transform.Find("Fedor(Clone)");
            if (fedorTransform != null)
                Destroy(fedorTransform.gameObject);
        }

        Vector2Int[] direcoes = {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (var dir in direcoes)
        {
            Vector2Int vizinha = posicao + dir;
            if (mapaGerado.TryGetValue(vizinha, out GameObject salaVizinha))
            {
                Transform fedorVizinho = salaVizinha.transform.Find("Fedor(Clone)");
                if (fedorVizinho != null)
                    Destroy(fedorVizinho.gameObject);
            }
        }
    }

    private void GarantirSalaSeguraEm00()
    {
        Vector2Int posInicial = new Vector2Int(0, 0);

        // Remove qualquer RespawnPoint antigo da cena
        foreach (var rp in FindObjectsByType<RespawnPoint>(FindObjectsSortMode.None))
        {
            DestroyImmediate(rp.gameObject);
        }

        // Garante que a sala (0,0) seja segura
        if (gridInfo[posInicial].temPoco)
        {
            Destroy(mapaGerado[posInicial]);
            mapaGerado.Remove(posInicial);
            gridInfo[posInicial].temPoco = false;

            GameObject novaSala = Instantiate(salaPrefab, Vector3.zero, Quaternion.identity, paiDasSalas);
            novaSala.name = "Sala (0,0)";
            mapaGerado.Add(posInicial, novaSala);

            Debug.Log("[PlayerGridGenerator] Poço removido e sala (0,0) regenerada sem poço.");
        }

        // Instancia RespawnPoint na sala (0,0), sempre no centro
        GameObject sala00 = mapaGerado[posInicial];

        GameObject marcador = new GameObject("RespawnMarker");
        marcador.transform.SetParent(sala00.transform);
        marcador.transform.localPosition = Vector3.zero;
        marcador.AddComponent<RespawnPoint>();

        Debug.Log($"[PlayerGridGenerator] RespawnPoint posicionado na sala (0,0) em {marcador.transform.position}");
    }

    public Vector2Int ConverterPosicaoMundoParaGrid(Vector3 pos)
    {
        int x = Mathf.RoundToInt(pos.x / espacoEntreSalas);
        int y = Mathf.RoundToInt(pos.z / espacoEntreSalas);
        return new Vector2Int(x, y);
    }

    private void AplicarBrisaNosPocos()
    {
        foreach (var kvp in gridInfo)
        {
            Vector2Int pos = kvp.Key;
            if (kvp.Value.temPoco)
            {
                Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

                foreach (Vector2Int dir in dirs)
                {
                    Vector2Int adj = pos + dir;
                    if (gridInfo.ContainsKey(adj) && !gridInfo[adj].temPoco)
                    {
                        gridInfo[adj].temBrisa = true;
                        RegistrarSensacao(adj, "brisa");

                        GameObject salaAdj = mapaGerado[adj];
                        if (salaAdj.transform.Find("Brisa") == null)
                        {
                            Vector3 posBrisa = salaAdj.transform.position + new Vector3(0, 1.5f, 0);
                            Instantiate(prefabBrisa, posBrisa, Quaternion.identity, salaAdj.transform).name = "Brisa";
                        }
                    }
                }
            }
        }
    }

    private void InstanciarWumpus()
    {
        do
        {
            posicaoWumpus = new Vector2Int(Random.Range(0, tamanhoX), Random.Range(0, tamanhoY));
        } while (posicaoWumpus == Vector2Int.zero || gridInfo[posicaoWumpus].temPoco);

        Vector3 pos = mapaGerado[posicaoWumpus].transform.position + new Vector3(0, 0.5f, 0);
        Quaternion rot = Quaternion.Euler(0f, rotacaoYWumpus, 0f);
        Instantiate(prefabWumpus, pos, rot, mapaGerado[posicaoWumpus].transform).tag = "wumpus";

        RegistrarSensacao(posicaoWumpus, "wumpus");
        gridInfo[posicaoWumpus].temWumpus = true;

        AplicarFedorNoWumpus(posicaoWumpus);
    }

    private void AplicarFedorNoWumpus(Vector2Int origem)
    {
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int dir in dirs)
        {
            Vector2Int adj = origem + dir;
            if (gridInfo.ContainsKey(adj))
            {
                gridInfo[adj].temFedor = true;
                RegistrarSensacao(adj, "fedor");

                GameObject salaAdj = mapaGerado[adj];
                if (salaAdj.transform.Find("Fedor") == null)
                {
                    Vector3 posFedor = salaAdj.transform.position + new Vector3(0, 1.5f, 0);
                    Instantiate(prefabFedor, posFedor, Quaternion.identity, salaAdj.transform).name = "Fedor";
                }
            }
        }
    }

    private void InstanciarVariosWumpus(int quantidade)
    {
        posicoesWumpus.Clear();

        int tentativasMax = tamanhoX * tamanhoY * 8;
        int tries = 0;

        while (posicoesWumpus.Count < quantidade && tries++ < tentativasMax)
        {
            Vector2Int p = new Vector2Int(Random.Range(0, tamanhoX), Random.Range(0, tamanhoY));

            if (p == Vector2Int.zero) continue;                 // não em (0,0)
            if (gridInfo[p].temPoco) continue;                  // não em poço
            if (gridInfo[p].temWumpus) continue;                // não duplicar
            if (gridInfo[p].temOuro) continue;                  // opcional: evitar ouro na mesma célula

            // Marca lógica
            gridInfo[p].temWumpus = true;
            posicaoWumpus = p;
            posicoesWumpus.Add(p);

            // Visual
            Vector3 pos = mapaGerado[p].transform.position + new Vector3(0, 0.5f, 0);
            Quaternion rot = Quaternion.Euler(0f, rotacaoYWumpus, 0f);
            var go = Instantiate(prefabWumpus, pos, rot, mapaGerado[p].transform);
            go.tag = "wumpus";

            // Sensações
            RegistrarSensacao(p, "wumpus");
            AplicarFedorNoWumpus(p);
        }
    }

    private void InstanciarOuro()
    {
        do
        {
            posicaoOuro = new Vector2Int(Random.Range(0, tamanhoX), Random.Range(0, tamanhoY));
        }
        while (posicaoOuro == Vector2Int.zero ||
               gridInfo[posicaoOuro].temPoco ||
               posicaoOuro == posicaoWumpus);

        Vector3 pos = mapaGerado[posicaoOuro].transform.position + new Vector3(0, 0.5f, 0);
        Quaternion rot = Quaternion.Euler(0f, rotacaoYOuro, 0f);
        GameObject ouroObj = Instantiate(prefabOuro, pos, rot, mapaGerado[posicaoOuro].transform);
        ouroObj.name = "ouro";
        ouroObj.tag = "ouro";

        Vector3 posBrilho = pos + new Vector3(0, 0.5f, 0);
        Instantiate(prefabBrilho, posBrilho, Quaternion.identity, mapaGerado[posicaoOuro].transform).name = "Brilho";

        gridInfo[posicaoOuro].temOuro = true;
        RegistrarSensacao(posicaoOuro, "brilho");

    }

    private void InstanciarVariosOuros(int quantidade)
    {
        posicoesOuro.Clear();

        int tentativasMax = tamanhoX * tamanhoY * 8;
        int tries = 0;

        while (posicoesOuro.Count < quantidade && tries++ < tentativasMax)
        {
            Vector2Int p = new Vector2Int(Random.Range(0, tamanhoX), Random.Range(0, tamanhoY));

            if (p == Vector2Int.zero) continue;                 // não em (0,0)
            if (gridInfo[p].temPoco) continue;                  // não em poço
            if (gridInfo[p].temOuro) continue;                  // não duplicar
            if (gridInfo[p].temWumpus) continue;                // opcional: evitar wumpus na mesma célula

            // Marca lógica
            gridInfo[p].temOuro = true;
            posicaoOuro = p; // compatibilidade
            posicoesOuro.Add(p);

            // Visual
            Vector3 pos = mapaGerado[p].transform.position + new Vector3(0, 0.5f, 0);
            Quaternion rot = Quaternion.Euler(0f, rotacaoYOuro, 0f);
            GameObject ouroObj = Instantiate(prefabOuro, pos, rot, mapaGerado[p].transform);
            ouroObj.name = "ouro";
            ouroObj.tag = "ouro";

            Vector3 posBrilho = pos + new Vector3(0, 0.5f, 0);
            Instantiate(prefabBrilho, posBrilho, Quaternion.identity, mapaGerado[p].transform).name = "Brilho";

            RegistrarSensacao(p, "brilho");
        }
    }

    public void ColetarOuroNaPosicao(Vector2Int posicao)
    {
        if (!gridInfo.ContainsKey(posicao))
        {
            Debug.LogWarning($"[PlayerGrid] Tentativa de coletar ouro em posição inválida: {posicao}");
            return;
        }

        if (!gridInfo[posicao].temOuro)
        {
            Debug.Log($"[PlayerGrid] Não há ouro na sala {posicao} para coletar.");
            return;
        }

        gridInfo[posicao].temOuro = false;
        posicoesOuro.Remove(posicao);
        RemoverSensacao(posicao, "brilho");

        if (mapaGerado.TryGetValue(posicao, out GameObject sala))
        {
            // Destrói o 'ouro' e o 'Brilho' daquela sala
            Transform ouroTF = null;
            Transform brilhoTF = null;

            foreach (Transform child in sala.transform)
            {
                if (child.name == "ouro") ouroTF = child;
                if (child.name == "Brilho") brilhoTF = child;
            }

            if (ouroTF != null) Destroy(ouroTF.gameObject);
            if (brilhoTF != null) Destroy(brilhoTF.gameObject);
        }

        ourosColetadosCount++;

        Debug.Log($"[PlayerGrid] Ouro coletado em {posicao}. Restantes: {totalOurosAlvo - ourosColetadosCount}");

        VerificarCondicaoParaLinhaDeChegada();
    }

    public void RegistrarWumpusMorto()
    {
        wumpusMortos++;
        VerificarCondicaoParaLinhaDeChegada();
    }

    public void RegistrarOuroColetado()
    {
        ourosColetadosCount++;
        VerificarCondicaoParaLinhaDeChegada();
    }

    private void VerificarCondicaoParaLinhaDeChegada()
    {
        if (wumpusMorto && ouroColetado && !linhaInstanciada)
        {
            linhaInstanciada = true;
            Vector3 posicao = new Vector3(0, 0.01f, 0);
            Instantiate(linhaDeChegadaPrefab, posicao, Quaternion.identity);
            Debug.Log("[PlayerGrid] Condições cumpridas: todos os Wumpus mortos e todos os Ouros coletados. Linha de chegada criada.");
        }
    }


    private void SpawnarPlayer()
    {
        string personagem = GameSessionManager.instancia.personagemEscolhido;
        GameObject prefab = personagem == "arqueiro" ? prefabArqueiro : prefabAmazona;
        Vector3 offset = personagem == "arqueiro" ? offsetArqueiro : offsetAmazona;

        Vector3 pos = Vector3.zero + offsetCentroSala + CalcularOffsetDoPlayer(prefab) + offset;
        GameObject player = Instantiate(prefab, pos, Quaternion.identity, paiDoPlayer);

        CameraFollow cam = FindFirstObjectByType<CameraFollow>();
        if (cam) cam.DefinirAlvo(player.transform.Find("CameraTarget") ?? player.transform);

        if (GameManager.instancia != null) GameManager.instancia.DefinirPlayer(player);
    }

    private Vector3 CalcularOffsetDoPlayer(GameObject prefab)
    {
        Collider col = prefab.GetComponentInChildren<Collider>();
        if (col != null)
        {
            Bounds b = col.bounds;
            return -new Vector3(b.center.x, b.extents.y, b.center.z);
        }
        return Vector3.zero;
    }

    public void LimparMapa()
    {
        foreach (Transform t in paiDasSalas) Destroy(t.gameObject);
        mapaGerado.Clear();
        gridInfo.Clear();
    }

    public static class TimerPontuacaoController
    {
        public static float tempoInicio;
        public static float tempoPegarOuro;
        public static float tempoMatarWumpus;
        public static float tempoFinal;
        public static int pontuacaoFinal;

        public static void Reiniciar()
        {
            tempoInicio = Time.time;
            tempoPegarOuro = -1;
            tempoMatarWumpus = -1;
            tempoFinal = -1;
            pontuacaoFinal = 0;
        }

        public static float TempoTotal() => tempoFinal - tempoInicio;
    }

    public Dictionary<Vector2Int, List<string>> sensacoesPorPosicao = new Dictionary<Vector2Int, List<string>>();

    public void RegistrarSensacao(Vector2Int pos, string tipo)
    {
        if (!sensacoesPorPosicao.ContainsKey(pos))
            sensacoesPorPosicao[pos] = new List<string>();

        if (!sensacoesPorPosicao[pos].Contains(tipo))
            sensacoesPorPosicao[pos].Add(tipo);
    }

    private static readonly Vector2Int[] DIRS4 =
{
    Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
};

    private IEnumerable<Vector2Int> Adjacentes4(Vector2Int p)
    {
        foreach (var d in DIRS4) yield return p + d;
    }

    private void RemoverSensacao(Vector2Int pos, string tipo)
    {
        if (sensacoesPorPosicao.TryGetValue(pos, out var lista))
        {
            if (lista.Contains(tipo)) lista.Remove(tipo);
            if (lista.Count == 0) sensacoesPorPosicao.Remove(pos);
        }
    }
}