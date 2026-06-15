using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private int damageAmount = 10;
    [Tooltip("Layer do corpo do Player. Outros colliders (Weapon, Feet) são ignorados.")]
    [SerializeField] private string playerLayerName = "Player";

    [Header("Efeito Visual de Dano Customizado")]
    [Tooltip("A cor com que a Fiorella vai piscar ao tomar dano deste inimigo específico.")]
    [SerializeField] private Color damageBlinkColor = Color.red; // Configure a cor da resina no Inspector!

    private int _playerLayerCached = -1;

    private void Awake()
    {
        _playerLayerCached = LayerMask.NameToLayer(playerLayerName);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Só aceita colliders que estão na layer "Player" (corpo principal)
        if (other.gameObject.layer != _playerLayerCached) return;

        var health = other.transform.root.GetComponent<PlayerHealth>();
        if (health == null) return;

        // NOVO: Passamos a quantidade de dano E a cor que queremos que a Fiorella pisque!
        health.TakeDamage(damageAmount, damageBlinkColor);
    }
}