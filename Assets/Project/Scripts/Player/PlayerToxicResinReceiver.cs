using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerStamina))]
[RequireComponent(typeof(BaseStats))]
public class PlayerToxicResinReceiver : MonoBehaviour
{
    [Header("Referências (auto)")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerStamina playerStamina;
    [SerializeField] private BaseStats baseStats;

    [Header("Atributo de Resistência")]
    [Tooltip("Arraste aqui o StatType 'Resistência'.")]
    [SerializeField] private StatType resistanceStat;

    [Header("Imunidade do Item")]
    [Tooltip("Quando true, soma +1 na resistência efetiva (item coletado).")]
    [SerializeField] private bool hasToxicResinImmunityItem = false;

    // Estado interno
    private readonly HashSet<HazardToxicResin> activeHazards = new HashSet<HazardToxicResin>();
    private float tickTimer = 0f;
    private float currentSpeedMultiplier = 1f;

    // Para outros scripts consultarem (ex.: PlayerController para aplicar slow)
    public float CurrentSpeedMultiplier => currentSpeedMultiplier;
    public bool IsInResin => activeHazards.Count > 0;

    private void Reset()
    {
        playerController = GetComponent<PlayerController>();
        playerHealth = GetComponent<PlayerHealth>();
        playerStamina = GetComponent<PlayerStamina>();
        baseStats = GetComponent<BaseStats>();
    }

    // Chamados pelo HazardToxicResin via OnTriggerEnter/Exit
    public void EnterHazard(HazardToxicResin hazard)
    {
        activeHazards.Add(hazard);
    }

    public void ExitHazard(HazardToxicResin hazard)
    {
        activeHazards.Remove(hazard);
        if (activeHazards.Count == 0)
        {
            currentSpeedMultiplier = 1f;
            tickTimer = 0f;
        }
    }

    // Método público para o item de imunidade ativar
    public void GrantImmunityItem()
    {
        hasToxicResinImmunityItem = true;
    }

    private void Update()
    {
        if (activeHazards.Count == 0) return;

        // Durante o dash, ignora completamente os efeitos
        if (playerController.IsDashing)
        {
            currentSpeedMultiplier = 1f;
            tickTimer = 0f;
            return;
        }

        // Se há sobreposição de poças, usa a mais letal (maior dano base)
        HazardToxicResin worst = null;
        foreach (var h in activeHazards)
        {
            if (worst == null || h.BaseDamagePerSecond > worst.BaseDamagePerSecond)
                worst = h;
        }
        if (worst == null) return;

        float mitigation = CalculateMitigation(worst);

        // Slow contínuo (aplicado a cada frame)
        currentSpeedMultiplier = Mathf.Lerp(worst.MaxSlowMultiplier, 1f, 1f - mitigation);

        // Tick discreto de dano + drenagem de stamina
        tickTimer += Time.deltaTime;
        if (tickTimer >= worst.TickInterval)
        {
            float damageThisTick = worst.BaseDamagePerSecond * mitigation * tickTimer;
            float staminaThisTick = worst.BaseStaminaDrainPerSecond * mitigation * tickTimer;

            playerHealth.TakeDamage(Mathf.RoundToInt(damageThisTick));
            playerStamina.TrySpendStamina(staminaThisTick);

            tickTimer = 0f;
        }
    }

    private float CalculateMitigation(HazardToxicResin hazard)
    {
        float charm = hasToxicResinImmunityItem ? 1f : 0f;
        int attribute = (resistanceStat != null) ? baseStats.GetStatValue(resistanceStat) : 0;
        float effectiveResistance = charm + attribute * hazard.AttributeContributionPerPoint;
        return 1f / (1f + effectiveResistance * hazard.MitigationCurveK);
    }
}