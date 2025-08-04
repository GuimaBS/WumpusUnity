using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlayerMovement : MonoBehaviour
{
    [Header("Movimentação")]
    public float moveDistance = 10f;
    public float moveSpeed = 20f;
    public float rotationSpeed = 400f;

    [Header("Partículas")]
    public GameObject prefabParticulaMorte;
    public GameObject prefabParticulaRespawn;
    public GameObject prefabParticulaColetaOuro;
    public GameObject prefabParticulaAcerto;
    public GameObject prefabParticulaErro;
    public Vector3 offsetParticulaRespawn = new Vector3(0, 1f, 0);

    private bool isMoving = false;
    private bool isDying = false;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private TowerGridGenerator gridGen;

    private int flechas = 2;
    private int ouroColetado = 0;
    private int wumpusMortos = 0;
    private int vidas = 5;

    private Vector2Int ultimaSalaAtiva = new Vector2Int(-999, -999);

    private void Start()
    {
        gridGen = TowerGridGenerator.instancia;
        targetPosition = transform.position;
        targetRotation = transform.rotation;

        AtualizarSalaAtual();
        AtualizarUI();
    }

    private void Update()
    {
        if (isDying) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public void RotateLeft()
    {
        if (isMoving || isDying) return;
        targetRotation *= Quaternion.Euler(0, -90, 0);
    }

    public void RotateRight()
    {
        if (isMoving || isDying) return;
        targetRotation *= Quaternion.Euler(0, 90, 0);
    }

    public void MoveForward()
    {
        if (isMoving || isDying) return;

        Vector3 dir = new Vector3(
            Mathf.Round(transform.forward.x),
            0,
            Mathf.Round(transform.forward.z)
        );

        Vector3 destination = targetPosition + dir * moveDistance;

        if (!SalaExisteNaDirecao(dir))
        {
            Debug.Log("Tentativa de sair do mapa bloqueada!");
            return;
        }

        StartCoroutine(MoveToPosition(destination));
    }

    bool SalaExisteNaDirecao(Vector3 direcao)
    {
        Vector2Int posAtual = new Vector2Int(
            Mathf.RoundToInt(targetPosition.x / moveDistance),
            Mathf.RoundToInt(targetPosition.z / moveDistance)
        );

        Vector2Int destino = posAtual + new Vector2Int(
            Mathf.RoundToInt(direcao.x),
            Mathf.RoundToInt(direcao.z)
        );

        return gridGen.gridInfo.ContainsKey(destino);
    }

    IEnumerator MoveToPosition(Vector3 destination)
    {
        isMoving = true;
        targetPosition = destination;

        while (Vector3.Distance(transform.position, destination) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = destination;
        isMoving = false;

        AtualizarSalaAtual();

        if (EstaEmSalaComPoco())
        {
            StartCoroutine(MorrerNoPoco());
        }
        else if (EstaEmSalaComWumpus())
        {
            StartCoroutine(MorrerParaOWumpus());
        }
    }

    bool EstaEmSalaComPoco()
    {
        Vector2Int pos = PegarPosicaoGrid();
        return gridGen.gridInfo.ContainsKey(pos) && gridGen.gridInfo[pos].temPoco;
    }

    bool EstaEmSalaComWumpus()
    {
        Vector2Int pos = PegarPosicaoGrid();
        return gridGen.posicaoWumpus == pos;
    }

    IEnumerator MorrerNoPoco()
    {
        Debug.Log("O jogador caiu no poço!");
        isDying = true;
        vidas--;

        if (prefabParticulaMorte != null)
            Instantiate(prefabParticulaMorte, transform.position + Vector3.up * 1f, Quaternion.identity);

        AtualizarUI();

        yield return new WaitForSeconds(1f);
        RespawnarPlayer();
    }

    IEnumerator MorrerParaOWumpus()
    {
        Debug.Log("O jogador foi morto pelo Wumpus!");
        isDying = true;
        vidas--;

        if (prefabParticulaMorte != null)
            Instantiate(prefabParticulaMorte, transform.position + Vector3.up * 1f, Quaternion.identity);

        AtualizarUI();

        yield return new WaitForSeconds(1f);
        RespawnarPlayer();
    }

    void RespawnarPlayer()
    {
        Vector3 posRespawn = Vector3.zero + gridGen.offsetCentroSala + new Vector3(0.2f, 0.2f, -0.35f);

        transform.SetPositionAndRotation(posRespawn, Quaternion.identity);
        targetPosition = transform.position;
        targetRotation = transform.rotation;

        if (prefabParticulaRespawn != null)
            Instantiate(prefabParticulaRespawn, transform.position + offsetParticulaRespawn, Quaternion.identity);

        AtualizarSalaAtual();
        AtualizarUI();
        isDying = false;
    }

    void AtualizarSalaAtual()
    {
        Vector2Int pos = PegarPosicaoGrid();

        // Desativar sala anterior
        if (gridGen.mapaGerado.TryGetValue(ultimaSalaAtiva, out GameObject salaAnterior) && salaAnterior.activeSelf)
            salaAnterior.SetActive(false);

        // Ativar sala atual
        AtivarSala(pos);
        ultimaSalaAtiva = pos;

    }

    void AtivarSala(Vector2Int pos)
    {
        if (gridGen.mapaGerado.TryGetValue(pos, out GameObject sala) && !sala.activeSelf)
        {
            sala.SetActive(true);
        }
    }

    Vector2Int PegarPosicaoGrid()
    {
        return new Vector2Int(
            Mathf.RoundToInt(transform.position.x / moveDistance),
            Mathf.RoundToInt(transform.position.z / moveDistance)
        );
    }

    void AtualizarUI()
    {
        TowerUIManager.instancia?.AtualizarFlechas(flechas);
        TowerUIManager.instancia?.AtualizarOuro(ouroColetado);
        TowerUIManager.instancia.AtualizarDWumpus(wumpusMortos);
        TowerUIManager.instancia?.AtualizarVidas(vidas);
    }

    public void ColetarOuro()
    {
        Vector2Int pos = PegarPosicaoGrid();
        if (!gridGen.gridInfo.ContainsKey(pos)) return;

        if (gridGen.gridInfo[pos].temOuro)
        {
            ouroColetado++;
            gridGen.gridInfo[pos].temOuro = false;
            Debug.Log("Ouro coletado!");

            if (prefabParticulaColetaOuro != null)
                Instantiate(prefabParticulaColetaOuro, transform.position + Vector3.up * 1f, Quaternion.identity);

            AtualizarUI();
        }
        else
        {
            Debug.Log("Não há ouro nesta sala.");
        }
    }

    public void AtirarFlecha()
    {
        if (flechas <= 0)
        {
            Debug.Log("Sem flechas restantes!");
            return;
        }

        flechas--;

        Vector2Int posAtual = PegarPosicaoGrid();
        Vector2Int direcao = new Vector2Int(
            Mathf.RoundToInt(transform.forward.x),
            Mathf.RoundToInt(transform.forward.z)
        );

        Vector2Int alvo = posAtual + direcao;

        if (alvo == gridGen.posicaoWumpus)
        {
            Debug.Log("Wumpus atingido!");
            gridGen.EliminarWumpusNaPosicao(alvo);
            wumpusMortos++;

            if (prefabParticulaAcerto != null)
                Instantiate(prefabParticulaAcerto, transform.position + Vector3.up * 1f, Quaternion.identity);
        }
        else
        {
            Debug.Log("Flecha errada!");
            if (prefabParticulaErro != null)
                Instantiate(prefabParticulaErro, transform.position + Vector3.up * 1f, Quaternion.identity);
        }

        AtualizarUI();
    }
}
