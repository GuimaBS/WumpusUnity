using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WumpusInteligente : MonoBehaviour
{
    private TileManager tileManager;
    private Vector2Int posicaoAtual;
    private HashSet<Vector2Int> visitadas = new HashSet<Vector2Int>();
    private float velocidade = 1f;
    private Vector2Int ultimaDirecao = Vector2Int.zero;
    private Animator anim;
    private Transform alvoAgente = null;

    private void Start()
    {
        tileManager = TileManager.instancia;

        anim = GetComponent<Animator>();
        if (anim == null)
            Debug.LogWarning("Animator não encontrado no Wumpus!");

        posicaoAtual = GridGenerator.ConverterPosicaoMundoParaGrid(transform.position);
        transform.position = new Vector3(posicaoAtual.x * 1.7f, transform.position.y, posicaoAtual.y * 1.7f);

        if (!GridGenerator.posicoesWumpus.Contains(posicaoAtual))
            GridGenerator.posicoesWumpus.Add(posicaoAtual);

        AtualizarFedor();

        StartCoroutine(ComportamentoIA());
    }

    private IEnumerator ComportamentoIA()
    {
        while (true)
        {
            DetectarAgenteProximo();

            Vector2Int proximaTile = EscolherProximaTile();

            if (proximaTile != posicaoAtual)
            {
                RemoverFedor();
                yield return StartCoroutine(MoverPara(proximaTile));
                AtualizarPosicaoWumpusNaGrid(posicaoAtual, proximaTile);
                posicaoAtual = proximaTile;
                AtualizarFedor();
                VerificarContatoComAgente();
            }

            yield return new WaitForSeconds(velocidade);
        }
    }

    private Vector2Int EscolherProximaTile()
    {
        if (alvoAgente != null)
        {
            Vector2Int posAgente = GridGenerator.ConverterPosicaoMundoParaGrid(alvoAgente.position);
            List<Vector2Int> direcoesPossiveis = Direcoes();

            Vector2Int melhorDirecao = ultimaDirecao;
            float menorDistancia = float.MaxValue;

            foreach (Vector2Int dir in direcoesPossiveis)
            {
                Vector2Int vizinho = posicaoAtual + dir;

                if (tileManager.ObterTileEm(vizinho) != null)
                {
                    float distancia = Vector2Int.Distance(vizinho, posAgente);
                    if (distancia < menorDistancia)
                    {
                        menorDistancia = distancia;
                        melhorDirecao = dir;
                    }
                }
            }

            ultimaDirecao = melhorDirecao;
            return posicaoAtual + melhorDirecao;
        }

        List<Vector2Int> candidatasNaoVisitadas = new List<Vector2Int>();
        List<Vector2Int> candidatasVisitadas = new List<Vector2Int>();

        foreach (Vector2Int dir in Direcoes())
        {
            Vector2Int vizinho = posicaoAtual + dir;

            if (tileManager.ObterTileEm(vizinho) != null)
            {
                if (!visitadas.Contains(vizinho))
                    candidatasNaoVisitadas.Add(dir);
                else
                    candidatasVisitadas.Add(dir);
            }
        }

        if (candidatasNaoVisitadas.Count > 0)
        {
            Vector2Int direcaoEscolhida = candidatasNaoVisitadas[Random.Range(0, candidatasNaoVisitadas.Count)];
            ultimaDirecao = direcaoEscolhida;
            return posicaoAtual + direcaoEscolhida;
        }

        List<Vector2Int> candidatasVisitadasSemReverso = candidatasVisitadas.FindAll(dir => dir != -ultimaDirecao);

        if (candidatasVisitadasSemReverso.Count > 0)
        {
            Vector2Int direcaoEscolhida = candidatasVisitadasSemReverso[Random.Range(0, candidatasVisitadasSemReverso.Count)];
            ultimaDirecao = direcaoEscolhida;
            return posicaoAtual + direcaoEscolhida;
        }

        if (candidatasVisitadas.Count > 0)
        {
            Vector2Int direcaoEscolhida = candidatasVisitadas[Random.Range(0, candidatasVisitadas.Count)];
            ultimaDirecao = direcaoEscolhida;
            return posicaoAtual + direcaoEscolhida;
        }

        foreach (Vector2Int dir in Direcoes())
        {
            Vector2Int vizinho = posicaoAtual + dir;
            if (tileManager.ObterTileEm(vizinho) != null)
            {
                ultimaDirecao = dir;
                return vizinho;
            }
        }

        ultimaDirecao = Direcoes()[Random.Range(0, Direcoes().Count)];
        return posicaoAtual + ultimaDirecao;
    }

    private IEnumerator MoverPara(Vector2Int destino)
    {
        Vector3 destinoMundo = new Vector3(destino.x * 1.7f, transform.position.y, destino.y * 1.7f);

        Vector3 direcao = destinoMundo - transform.position;
        if (direcao != Vector3.zero)
        {
            Quaternion rotacaoAlvo = Quaternion.LookRotation(direcao.normalized);
            float t = 0f;
            float tempoRotacao = 0.15f;
            Quaternion rotacaoInicial = transform.rotation;

            while (t < 1f)
            {
                t += Time.deltaTime / tempoRotacao;
                transform.rotation = Quaternion.Slerp(rotacaoInicial, rotacaoAlvo, t);
                yield return null;
            }
        }

        transform.position = destinoMundo;
        visitadas.Add(destino);
        yield break;
    }

    private void VerificarContatoComAgente()
    {
        Collider[] colisores = Physics.OverlapSphere(transform.position, 0.4f);
        foreach (var col in colisores)
        {
            if (col.CompareTag("agente1") || col.CompareTag("agente2"))
            {
                if (anim != null)
                    anim.SetTrigger("wattack");

                Destroy(col.gameObject);
                LogManager.instancia?.AdicionarLog("<color=red><b>O Wumpus matou um agente!</b></color>");
                SistemaDePontuacao.instancia?.AdicionarDerrota();
            }
        }
    }

    private void AtualizarFedor()
    {
        GridGenerator.instancia.AdicionarFedorNasAdjacentes(posicaoAtual);
    }

    private void RemoverFedor()
    {
        GridGenerator.RemoverFedor(posicaoAtual);
    }

    private void AtualizarPosicaoWumpusNaGrid(Vector2Int antiga, Vector2Int nova)
    {
        if (GridGenerator.posicoesWumpus.Contains(antiga))
            GridGenerator.posicoesWumpus.Remove(antiga);

        if (!GridGenerator.posicoesWumpus.Contains(nova))
            GridGenerator.posicoesWumpus.Add(nova);
    }

    private List<Vector2Int> Direcoes()
    {
        return new List<Vector2Int>
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };
    }

    private void DetectarAgenteProximo()
    {
        Collider[] agentes = Physics.OverlapSphere(transform.position, 5f);

        foreach (var col in agentes)
        {
            if (col.CompareTag("agente1") || col.CompareTag("agente2"))
            {
                alvoAgente = col.transform;
                return;
            }
        }

        alvoAgente = null;
    }
}
