using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SummonerBossController : MonoBehaviour
{
    // ==========================================
    // [NOVO] SISTEMA DE ATIVAÇÃO
    // ==========================================
    [Header("Ativação do Boss")]
    [Tooltip("Distância que a Fiorella precisa chegar para a batalha começar.")]
    public float raioDeAtivacao = 8f;
    private bool bossAtivo = false; // O boss começa "a dormir"

    [Header("Sistema de Invocação (Spawner)")]
    public GameObject minionPrefab;
    public float tempoDeSpawn = 5f;
    public float raioDeSpawn = 3f;
    
    private List<GameObject> minionsAtivos = new List<GameObject>();
    private float timerSpawn;

    [Header("Ataque de Afastamento")]
    public float raioDeAtaque = 2.5f;
    public float forcaAfastamento = 15f;
    public int danoAfastamento = 10;
    public float tempoRecargaAtaque = 3f;
    
    [Tooltip("Tempo que o círculo vermelho fica visível antes de dar o dano.")]
    public float tempoDeAviso = 0.6f; 
    [Tooltip("Arraste o objeto do Círculo Vermelho (Filho do Boss) para cá.")]
    public GameObject circuloAvisoVisual; 
    
    private float timerAtaque;
    private bool executandoAtaque = false; 

    [Header("Recompensas (Drop)")]
    public GameObject itemDropPrefab;

    void Start()
    {
        timerSpawn = tempoDeSpawn;
        timerAtaque = tempoRecargaAtaque;
        
        if (circuloAvisoVisual != null)
        {
            circuloAvisoVisual.SetActive(false);
        }
    }

    void Update()
    {
        // [NOVO] Se o boss estiver a dormir, ele só procura pela Fiorella e não faz mais nada!
        if (!bossAtivo)
        {
            VerificarAtivacao();
            return; 
        }

        if (executandoAtaque) return;

        ControlarInvocacao();
        ControlarAtaque();
    }

    // ==========================================
    // [NOVO] LÓGICA DE DETETAR A FIORELLA
    // ==========================================
    private void VerificarAtivacao()
    {
        // Passa o "radar" usando o raio de ativação
        Collider2D[] objetos = Physics2D.OverlapCircleAll(transform.position, raioDeAtivacao);

        foreach (Collider2D objeto in objetos)
        {
            if (objeto.CompareTag("Player"))
            {
                bossAtivo = true; // Acorda o Boss!
                Debug.Log("<color=magenta>[Boss] A Fiorella entrou na arena! O Invocador despertou!</color>");

                // ==========================================
                // [NOVO] Avisa o script de vida para mostrar a barra na tela!
                // ==========================================
                EnemyHealth vidaDoBoss = GetComponent<EnemyHealth>();
                if (vidaDoBoss != null)
                {
                    vidaDoBoss.AtivarBarraBoss();
                }

                break; // Não precisamos de continuar a procurar
            }
        }
    }

    private void ControlarInvocacao()
    {
        timerSpawn -= Time.deltaTime;

        if (timerSpawn <= 0f)
        {
            if (minionPrefab != null)
            {
                Vector2 posicaoAleatoria = (Vector2)transform.position + (Random.insideUnitCircle * raioDeSpawn);
                GameObject novoMinion = Instantiate(minionPrefab, posicaoAleatoria, Quaternion.identity);
                minionsAtivos.Add(novoMinion);
            }
            timerSpawn = tempoDeSpawn;
        }
    }

    private void ControlarAtaque()
    {
        if (timerAtaque > 0)
        {
            timerAtaque -= Time.deltaTime;
            return; 
        }

        Collider2D[] objetosAtingidos = Physics2D.OverlapCircleAll(transform.position, raioDeAtaque);

        foreach (Collider2D objeto in objetosAtingidos)
        {
            if (objeto.CompareTag("Player"))
            {
                StartCoroutine(RotinaDeAtaque());
                break; 
            }
        }
    }

    private IEnumerator RotinaDeAtaque()
    {
        executandoAtaque = true; 

        if (circuloAvisoVisual != null)
        {
            circuloAvisoVisual.SetActive(true);
        }
        
        yield return new WaitForSeconds(tempoDeAviso);

        if (circuloAvisoVisual != null)
        {
            circuloAvisoVisual.SetActive(false); 
        }

        Collider2D[] objetosNoImpacto = Physics2D.OverlapCircleAll(transform.position, raioDeAtaque);
        
        foreach (Collider2D objeto in objetosNoImpacto)
        {
            if (objeto.CompareTag("Player"))
            {
                var playerHealth = objeto.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(danoAfastamento);
                }

                var playerController = objeto.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    Vector2 direcaoEmpurrao = (objeto.transform.position - transform.position).normalized;
                    playerController.ApplyKnockback(direcaoEmpurrao, forcaAfastamento);
                }
                break; 
            }
        }

        timerAtaque = tempoRecargaAtaque;
        executandoAtaque = false; 
    }

    public void OnBossDeath()
    {
        foreach (GameObject minion in minionsAtivos)
        {
            if (minion != null) Destroy(minion);
        }
        minionsAtivos.Clear();

        if (itemDropPrefab != null) Instantiate(itemDropPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // [NOVO] Desenha um círculo amarelo para mostrar a área de Ativação do Boss
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, raioDeAtivacao);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, raioDeSpawn);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, raioDeAtaque);
    }
}