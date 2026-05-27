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

    private float _currentHealth;
    private BaseStats _stats;
    public bool isInvulnerable { get; set; }
    
    // CORREÇÃO: Variável de controle para evitar que Die() seja chamado múltiplas vezes
    private bool _isDead = false; 

    private void Awake()
    {
        _stats = GetComponent<BaseStats>();
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

    public void TakeDamage(int damage)
    {
        // Se já estiver morta, ignora novos danos
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

        if (_currentHealth <= 0) Die();
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
        _isDead = true; // Trava o estado de morte
        
        // 1. Drop das almas (Pólen) no local da morte
        GetComponent<PlayerProgression>().DropSoulsOnDeath();

        // 2. Inicia o processo de animação e espera antes do Respawn
        StartCoroutine(DeathSequenceRoutine());
    }

    private IEnumerator DeathSequenceRoutine()
    {
        Debug.Log("Fiorella Iniciou a animação de morte...");

        // 3. Desativa os controles e física da Fiorella para ela não andar morta
        PlayerController playerController = GetComponent<PlayerController>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Animator animator = GetComponent<Animator>();

        if (playerController != null) playerController.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero; // Faz ela parar imediatamente no chão
        
        // 4. Toca o gatilho de animação de morte no seu Animator (Crie um Trigger chamado "Die" lá)
       
        // 5. TEMPO DE ESPERA (ex: 2.5 segundos para o jogador ver a Fiorella caída)
        yield return new WaitForSeconds(2.5f);

        // 6. Opcional: Chamar o fade de tela preta aqui se tiver (SceneFader)
        // Ex: if (SceneFader.Instance != null) yield return SceneFader.Instance.FadeIn();

        // 7. Agora sim, faz o teleporte para o checkpoint
        if (BonfireManager.Instance != null)
        {
            BonfireManager.Instance.RespawnPlayerAtLastBonfire();
        }
        else
        {
            // Fallback de segurança se o Manager não existir
            transform.position = Vector3.zero; 
        }

        // 8. Restaura a vida e reativa os controles após o teleporte concluir
        _currentHealth = GetMaxHealth();
        UpdateUI();
        
        if (playerController != null) playerController.enabled = true;
        _isDead = false; // Permite que ela possa morrer novamente no futuro
    }

    public void SetHealthBar(Slider bar)
    {
        healthBar = bar;
        UpdateUI();
    }
}