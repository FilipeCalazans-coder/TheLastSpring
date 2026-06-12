using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyDamage : MonoBehaviour
{
    [SerializeField] private int damageAmount = 10;
    [Tooltip("Layer do corpo do Player. Outros colliders (Weapon, Feet) são ignorados.")]
    [SerializeField] private string playerLayerName = "Player";

    private int _playerLayerCached = -1;

    private void Awake()
    {
        _playerLayerCached = LayerMask.NameToLayer(playerLayerName);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Só aceita colliders que estão na layer "Player" (corpo principal)
        // Ignora WeaponCollider, FeetCollider e outros filhos com layers próprias
        if (other.gameObject.layer != _playerLayerCached) return;

        var health = other.transform.root.GetComponent<PlayerHealth>();
        if (health == null) return;

        health.TakeDamage(damageAmount);
    }
}