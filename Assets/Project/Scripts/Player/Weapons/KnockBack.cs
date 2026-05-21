using System.Collections;
using UnityEngine;

public class KnockBack : MonoBehaviour
{
    public bool GettingKnockedBack { get; private set; } // Variável pública apenas de leitura

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void GetKnockedBack(Transform damageSource, float knockBackThrust)
    {
        GettingKnockedBack = true;

        // Calcula a direção do empurrão
        Vector2 difference = (transform.position - damageSource.position).normalized;
        Vector2 force = difference * knockBackThrust;

        // Limpa a velocidade atual e aplica a força do impacto
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);

        // Inicia a rotina para recuperar o controlo após o empurrão
        StartCoroutine(KnockRoutine());
    }

    private IEnumerator KnockRoutine()
    {
        // O tempo que o inimigo fica "tonto" e escorregando
        yield return new WaitForSeconds(0.2f); 
        
        // Trava o corpo para ele não continuar a deslizar como no gelo
        rb.linearVelocity = Vector2.zero; 
        
        // Devolve o controlo ao EnemyPathFinding
        GettingKnockedBack = false;
    }
}