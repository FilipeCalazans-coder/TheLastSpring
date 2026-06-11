using UnityEngine;

namespace Project.Scripts.Player.Skills
{
    public class Landmine : MonoBehaviour
    {
        [Header("Configurações da Mina")]
        public int explosionDamage = 20; // Quanto dano a mina causa
        public float duration = 10f;     // Quanto tempo a mina fica no chão antes de sumir sozinha
        public GameObject explosionVFX;  // Efeito visual da explosão (opcional)

        private void Start()
        {
            // Se nenhum inimigo pisar, a mina destrói-se sozinha após 'duration' segundos
            Destroy(gameObject, duration);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Se quem pisou tem a tag "Enemy" (Inimigo)
            if (other.CompareTag("Enemy"))
            {
                // Procura o script de vida do inimigo
                EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
                
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(explosionDamage);
                    Debug.Log("<color=orange>Mina explodiu num inimigo!</color>");
                }

                // Cria a partícula de explosão
                if (explosionVFX != null)
                {
                    Instantiate(explosionVFX, transform.position, Quaternion.identity);
                }

                // Destrói a mina após explodir
                Destroy(gameObject);
            }
        }
    }
}