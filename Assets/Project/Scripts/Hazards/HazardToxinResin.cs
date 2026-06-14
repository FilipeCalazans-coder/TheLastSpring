using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HazardToxicResin : MonoBehaviour
{
    [Header("Detecção")]
    [Tooltip("Detecta apenas colliders marcados com PlayerFeetMarker (componente no filho FeetCollider do Player).")]
    [SerializeField] private bool requireFeetMarker = true;

    [Header("Efeitos Base (quando Resistência = 0)")]
    [Tooltip("Dano por segundo sofrido enquanto em cima da resina.")]
    [SerializeField] private float baseDamagePerSecond = 8f;

    [Tooltip("Stamina drenada por segundo enquanto em cima da resina.")]
    [SerializeField] private float baseStaminaDrainPerSecond = 25f;

    [Tooltip("Multiplicador de velocidade quando resistência = 0. 0.25 = anda a 25% da velocidade normal.")]
    [Range(0.05f, 1f)]
    [SerializeField] private float maxSlowMultiplier = 0.25f;

    [Header("Curva de Mitigação")]
    [Tooltip("Quanto cada ponto do atributo Resistência contribui para a resistência efetiva.")]
    [SerializeField] private float attributeContributionPerPoint = 0.1f;

    [Tooltip("Inclinação da mitigação. Mais alto = atributo+item reduzem efeitos mais agressivamente.")]
    [SerializeField] private float mitigationCurveK = 4f;

    [Header("Performance")]
    [Tooltip("Intervalo entre ticks de dano/stamina. 0.25 = 4 ticks por segundo.")]
    [SerializeField] private float tickInterval = 0.25f;

    // Getters públicos para o Receiver consumir
    public float BaseDamagePerSecond => baseDamagePerSecond;
    public float BaseStaminaDrainPerSecond => baseStaminaDrainPerSecond;
    public float MaxSlowMultiplier => maxSlowMultiplier;
    public float AttributeContributionPerPoint => attributeContributionPerPoint;
    public float MitigationCurveK => mitigationCurveK;
    public float TickInterval => tickInterval;

    private void Reset()
    {
        // Garante que o Collider2D fica como trigger ao adicionar o componente
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (requireFeetMarker && other.GetComponent<PlayerFeetMarker>() == null) return;

        var receiver = other.GetComponentInParent<PlayerToxicResinReceiver>();
        if (receiver != null) receiver.EnterHazard(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (requireFeetMarker && other.GetComponent<PlayerFeetMarker>() == null) return;

        var receiver = other.GetComponentInParent<PlayerToxicResinReceiver>();
        if (receiver != null) receiver.ExitHazard(this);
    }
}