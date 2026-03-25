using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int startingHealth = 3;
    [SerializeField] private GameObject deathVFXPrefab;
    [SerializeField] private float knockBackThrust = 15f;
    
    [Header("Resistência")]
    [SerializeField] private StatType defenseStatType;

    [Header("Recompensa")]
    [SerializeField] private int soulsDropped = 50; // <--- ADICIONE ESTA LINHA

    private EnemyHealthBar _healthBar;
    private int currentHealth;
    private KnockBack knockBack;
    private Flash flash;
    private BaseStats _myStats; 

    private void Awake()
    {
        _healthBar = GetComponentInChildren<EnemyHealthBar>();
        flash = GetComponent<Flash>();
        knockBack = GetComponent<KnockBack>();
        _myStats = GetComponent<BaseStats>();

        if (_myStats == null) 
        {
            Debug.LogWarning($"BaseStats não encontrado no {gameObject.name}. O inimigo terá 0 de defesa.");
        }
    }

    private void Start()
    {
        currentHealth = startingHealth;
    }

    public void TakeDamage(int damage)
{
    float finalDamage = damage;

    if (_myStats != null && defenseStatType != null)
    {
        int defenseValue = _myStats.GetStatValue(defenseStatType);
        float mitigation = defenseValue / 100f;
        mitigation = Mathf.Clamp(mitigation, 0f, 0.9f);
        finalDamage = damage * (1f - mitigation);

        Debug.Log($"{gameObject.name} mitigou o dano de {damage} para {Mathf.RoundToInt(finalDamage)}");
    }

    // 1. Primeiro subtraímos a vida
    currentHealth -= Mathf.RoundToInt(finalDamage);

    // 2. DEPOIS atualizamos a barra (com o valor já reduzido)
    if (_healthBar != null)
    {
        _healthBar.UpdateHealthBar(currentHealth, startingHealth);
    }
    
    // (Knockback, Flash, Death Check...)
    if(knockBack) knockBack.GetKnockedBack(PlayerController.Instance.transform, knockBackThrust);
    if(flash) StartCoroutine(flash.FlashRoutine());
    StartCoroutine(CheckDetectDeathRoutine());
}

    private IEnumerator CheckDetectDeathRoutine()
    {
        // Se houver flash, espera. Se não, checa a morte direto.
        float waitTime = flash != null ? flash.GetRestoreMatTime() : 0.1f;
        yield return new WaitForSeconds(waitTime);
        DetectDeath();
    }

    public void DetectDeath()
    {
        if (currentHealth <= 0)
        {
            // Entrega das Almas
            if (PlayerController.Instance != null)
            {
                PlayerProgression playerXP = PlayerController.Instance.GetComponent<PlayerProgression>();
                if (playerXP != null)
                {
                    playerXP.AddSouls(soulsDropped);
                }
            }

            // VFX e Destruição
            if (deathVFXPrefab) Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}