using UnityEngine;

public class EnemyPathFinding : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 3f; // Agora é totalmente público e atualiza em tempo real!

    private Rigidbody2D rb;
    private Vector2 moveDir;
    private KnockBack knockBack;

    private void Awake()
    {
        knockBack = GetComponent<KnockBack>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // Se estiver a sofrer knockback, o motor desliga as pernas
        if (knockBack != null && knockBack.GettingKnockedBack)
        {
            Debug.Log($"[PathFinding] BLOQUEADO por knockback. linearVelocity atual: {rb.linearVelocity.magnitude:F3}");
            return;
        }

        // O motor aplica a velocidade atualizada
        rb.linearVelocity = moveDir * moveSpeed;
    }

    public void MoveTo(Vector2 targetPosition)
    {
        // O cast para (Vector2) garante que ignoramos o maldito Eixo Z!
        moveDir = (targetPosition - (Vector2)transform.position).normalized;
    }

    /// <summary>
    /// Define uma direção FIXA de movimento (não persegue um alvo).
    /// Usado pelo charge: o inimigo se compromete com a direção e não corrige mais.
    /// </summary>
    public void MoveInDirection(Vector2 direction)
    {
        moveDir = direction.normalized;
    }

    public void StopMoving()
    {
        moveDir = Vector2.zero;
        rb.linearVelocity = Vector2.zero;
    }
}