using UnityEngine;

public class DamageSource : MonoBehaviour
{
    [Header("Configurações de Status")]
    // Arraste aqui o ScriptableObject 'AtaqueFisico' que criamos
    [SerializeField] private StatType attackStatType; 
    
    private BaseStats _playerStats;

    private void Awake()
    {
        // Como o DamageSource costuma estar em um filho do Player (na arma),
        // buscamos o componente BaseStats que está no objeto principal.
        _playerStats = GetComponentInParent<BaseStats>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificamos se o que acertamos tem o SEU script EnemyHealth
        EnemyHealth enemy = other.gameObject.GetComponent<EnemyHealth>();

        if (enemy != null && _playerStats != null)
        {
            // BUSCAMOS o valor dinâmico: se no Inspector estiver 10, o dano será 10.
            int damageToApply = _playerStats.GetStatValue(attackStatType);
            
            // CHAMAMOS o seu método TakeDamage que já cuida do flash e knockback
            enemy.TakeDamage(damageToApply);
        }
    }
}