using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configurações de Status")]
    [SerializeField] private StatType vitalityStat;
    [SerializeField] private StatType defenseStat;

    [Header("UI")]
    [SerializeField] private Slider healthBar;

    [Header("Configurações do Efeito de Pisco")]
    [SerializeField] private float tempoDePisco = 0.08f;   // Tempo de cada piscada (ligado/desligado)
    [SerializeField] private int quantidadeDePiscos = 3;  // Quantas vezes ela vai piscar ao tomar dano

    [Header("Transição de Morte (Vídeos)")]
    public UnityEngine.Video.VideoClip deathEnterClip; // Vídeo de entrada (ex: tela fechando em vermelho)
    public UnityEngine.Video.VideoClip deathExitClip;  // Vídeo de saída (ex: tela abrindo na fogueira)

    private float _currentHealth;
    private BaseStats _stats;
    private bool _isDead = false; 

    // VARIÁVEIS NOVAS: Para controlar o visual do pisco da Fiorella
    private SpriteRenderer _mySpriteRenderer; 
    private Coroutine _piscoCoroutine;        

    public bool isInvulnerable { get; set; }

    private void Awake()
    {
        _stats = GetComponent<BaseStats>();
        // Captura o componente de imagem (SpriteRenderer) da Fiorella
        _mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        UpdateMaxHealth();
    }

    public float GetMaxHealth()
    {
        int vit = _stats.GetStatValue(vitalityStat);
        return vit * 10f; 
    }

    public void UpdateMaxHealth()
    {
        _currentHealth = GetMaxHealth();
        UpdateUI();
    }

    // CORREÇÃO AQUI: Agora a função aceita o dano E uma cor opcional (blinkColor)
    public void TakeDamage(int damage, Color? blinkColor = null)
    {
        if (_isDead) return;

        if (isInvulnerable) 
        {
            Debug.Log("<color=white>Dano desviado! (I-Frame)</color>");
            return; 
        }
        
        int def = _stats.GetStatValue(defenseStat);
        float mitigation = Mathf.Clamp(def / 100f, 0f, 0.9f);
        
        float finalDamage = damage * (1f - mitigation);
        _currentHealth -= finalDamage;

        Debug.Log($"Player recebeu {finalDamage} de dano. HP: {_currentHealth}");

        UpdateUI();

        // ==========================================
        // 1. NOVO: TREMOR DE CÂMERA AO TOMAR DANO
        // ==========================================
        if (CameraShake.Instance != null && _currentHealth > 0)
        {
            CameraShake.Instance.Shake(0.2f, 0.35f); // Duração: 0.2s | Força: 0.35f
        }

        // ==========================================
        // 2. NOVO: EFEITO VISUAL DE PISCO COLORIDO
        // ==========================================
        if (_currentHealth > 0)
        {
            // Se nenhuma cor for enviada, usamos vermelho padrão por segurança
            Color corFinal = blinkColor ?? Color.red;

            // Se já estava piscando antes, para a antiga e começa uma nova limpa
            if (_piscoCoroutine != null) StopCoroutine(_piscoCoroutine);
            _piscoCoroutine = StartCoroutine(BlinkRoutine(corFinal));
        }

        if (_currentHealth <= 0) Die();
    }

    // ROTINA DO PISCO: Alterna as cores no tempo correto
    private IEnumerator BlinkRoutine(Color corDoInimigo)
    {
        if (_mySpriteRenderer == null) yield break;

        Color corOriginal = Color.white; // Branco na Unity redefine o sprite para a cor padrão dele

        for (int i = 0; i < quantidadeDePiscos; i++)
        {
            _mySpriteRenderer.color = corDoInimigo; // Fica com a cor da resina/inimigo
            yield return new WaitForSeconds(tempoDePisco);

            _mySpriteRenderer.color = corOriginal;  // Volta ao normal
            yield return new WaitForSeconds(tempoDePisco);
        }

        _piscoCoroutine = null; 
    }

    private void UpdateUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = GetMaxHealth();
            healthBar.value = _currentHealth;
        }
    }

    private void Die()
    {
        _isDead = true; 
        
        // Para o pisco imediatamente se ela morrer para não bugar a animação de morte
        if (_piscoCoroutine != null) StopCoroutine(_piscoCoroutine);
        if (_mySpriteRenderer != null) _mySpriteRenderer.color = Color.white;

        GetComponent<PlayerProgression>().DropSoulsOnDeath();
        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        Debug.Log("Fiorella Iniciou a animação de morte...");

        PlayerController playerController = GetComponent<PlayerController>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        
        // Bloqueia os controles e para o movimento imediatamente
        if (playerController != null) playerController.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero; 
       
        // Tempo para a animação da Fiorella caindo no chão tocar (ajuste se precisar)
        yield return new WaitForSeconds(2.0f);

        // ==========================================
        // NOVO: Chama o SceneFader para a Morte!
        // ==========================================
        if (SceneFader.Instance != null && deathEnterClip != null)
        {
            SceneFader.Instance.PlayVideoTransition(deathEnterClip, deathExitClip, () => 
            {
                // TUDO AQUI DENTRO RODA NO "PONTO CEGO" (Quando a tela estiver coberta pelo vídeo)
                
                if (BonfireManager.Instance != null)
                {
                    BonfireManager.Instance.RespawnPlayerAtLastBonfire();
                }
                else
                {
                    transform.position = Vector3.zero; 
                }

                _currentHealth = GetMaxHealth();
                UpdateUI();
                
                if (playerController != null) playerController.enabled = true;
                _isDead = false; 
            });
        }
        else
        {
            // Sistema de Segurança (Fallback): Se você esquecer de colocar o vídeo ou o Fader
            if (BonfireManager.Instance != null) BonfireManager.Instance.RespawnPlayerAtLastBonfire();
            else transform.position = Vector3.zero;

            _currentHealth = GetMaxHealth();
            UpdateUI();
            
            if (playerController != null) playerController.enabled = true;
            _isDead = false; 
        }
    }

    public void SetHealthBar(Slider bar)
    {
        healthBar = bar;
        UpdateUI();
    }
}