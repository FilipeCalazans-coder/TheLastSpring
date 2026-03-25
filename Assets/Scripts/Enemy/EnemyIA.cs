using UnityEngine;
using System.Collections;

public class EnemyIA : MonoBehaviour
{
    private enum State { Idle, Chasing, Attacking }
    private State _currentState = State.Idle;

    [Header("Configurações de Distância")]
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float moveSpeed = 2f;

    private Transform _player;
    private Rigidbody2D _rb;
    private SpriteRenderer _sprite;

    private bool _canMove = true; // Controla se o inimigo pode se mover (usado para Knockback)

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _player = PlayerController.Instance.transform;
    }

    private void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        switch (_currentState)
        {
            case State.Idle:
                if (distanceToPlayer < chaseRange) _currentState = State.Chasing;
                break;

            case State.Chasing:
                if (distanceToPlayer <= attackRange) _currentState = State.Attacking;
                else if (distanceToPlayer > chaseRange) _currentState = State.Idle;
                break;

            case State.Attacking:
                if (distanceToPlayer > attackRange) _currentState = State.Chasing;
                break;
        }
    }

    public void StopMoving(float duration)
    {
        StartCoroutine(StopMovingRoutine(duration));
    }

    private IEnumerator StopMovingRoutine(float duration)
    {
        _canMove = false;
        yield return new WaitForSeconds(duration);
        _canMove = true;
    }

    private void FixedUpdate()
    {
        if (!_canMove) return; // Se não puder mover (Knockback), não mexe na velocidade

        if (_currentState == State.Chasing)
        {
            MoveTowardsPlayer();
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }

    private void MoveTowardsPlayer()
    {
        if (_player == null) return;

        Vector2 direction = (_player.position - transform.position).normalized;
        _rb.linearVelocity = direction * moveSpeed;

        if (_sprite != null)
        {
            _sprite.flipX = direction.x < 0;
        }
    }
}