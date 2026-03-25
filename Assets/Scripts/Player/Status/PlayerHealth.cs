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

    private void Awake()
    {
        _stats = GetComponent<BaseStats>();
    }

    private void Start()
    {
        UpdateMaxHealth();
    }

    // Calcula o HP Máximo baseado na Vitalidade
    public float GetMaxHealth()
    {
        int vit = _stats.GetStatValue(vitalityStat);
        return vit * 10f; // Cada ponto de Vitalidade dá 10 de HP
    }

    public void UpdateMaxHealth()
    {
        _currentHealth = GetMaxHealth();
        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        // Se estiver em I-Frame, ignora o dano completamente
        if (isInvulnerable) 
        {
            Debug.Log("<color=white>Dano desviado! (I-Frame)</color>");
            return; 
        }
        
        // Usamos a mesma lógica de defesa percentual que criamos para o inimigo!
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
        Debug.LogError("Player Morreu!");

    }

    public void SetHealthBar(Slider bar)
    {
        healthBar = bar;
        UpdateUI();
    }
}