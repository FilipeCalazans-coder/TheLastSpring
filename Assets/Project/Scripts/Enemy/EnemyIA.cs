using UnityEngine;
using System.Collections;

public class EnemyIA : MonoBehaviour
{
    private enum State { Patrolling, Chasing, Attacking }
    private State _currentState = State.Patrolling;

    [Header("Configurações de Visão e Ataque")]
    public float chaseRange = 5f;
    public float attackRange = 1.2f;

    [Header("Configurações de Patrulha")]
    public float patrolRadius = 3f;         
    public float waitTimeBetweenPatrols = 2f; 

    private Transform _player;
    private EnemyPathFinding _pathFinding;
    private SpriteRenderer _sprite;

    private Vector2 _startPosition;
    private Vector2 _roamPosition;
    private float _patrolTimer;
    private bool _isAttacking = false;

    private void Awake()
    {
        _pathFinding = GetComponent<EnemyPathFinding>();
        _sprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (PlayerController.Instance != null) _player = PlayerController.Instance.transform;
        
        _startPosition = transform.position; 
        _roamPosition = GetRandomRoamPosition(); 
    }

    private void Update()
    {
        // Se ainda não encontrou o player, tenta procurar novamente
        if (_player == null) 
        {
            if (PlayerController.Instance != null) _player = PlayerController.Instance.transform;
            return;
        }

        if (_isAttacking) return; 

        // CRÍTICO: Cast para Vector2 ignora profundidades 3D e resolve a Poluição do Eixo Z
        float distanceToPlayer = Vector2.Distance((Vector2)transform.position, (Vector2)_player.position);

        switch (_currentState)
        {
            case State.Patrolling:
                PatrolLogic();
                if (distanceToPlayer < chaseRange) _currentState = State.Chasing;
                break;

            case State.Chasing:
                ChaseLogic();
                if (distanceToPlayer <= attackRange) 
                {
                    StartCoroutine(AttackRoutine());
                }
                else if (distanceToPlayer > chaseRange + 1f) 
                {
                    _currentState = State.Patrolling;
                    _roamPosition = GetRandomRoamPosition(); 
                }
                break;
        }
        
        AdjustFacingDirection();
    }

    private void PatrolLogic()
    {
        _pathFinding.MoveTo(_roamPosition);

        if (Vector2.Distance((Vector2)transform.position, _roamPosition) < 0.5f)
        {
            _pathFinding.StopMoving(); 
            _patrolTimer += Time.deltaTime; 

            if (_patrolTimer >= waitTimeBetweenPatrols)
            {
                _roamPosition = GetRandomRoamPosition();
                _patrolTimer = 0f;
            }
        }
    }

    private void ChaseLogic()
    {
        _pathFinding.MoveTo((Vector2)_player.position);
    }

    private IEnumerator AttackRoutine()
    {
        _currentState = State.Attacking;
        _isAttacking = true;
        
        _pathFinding.StopMoving();
        
        Debug.Log("<color=red>Inimigo Atacou!</color>");
        
        yield return new WaitForSeconds(1f); 
        
        _isAttacking = false;
        _currentState = State.Chasing; 
    }

    private Vector2 GetRandomRoamPosition()
    {
        // Garante que o raio de patrulha nunca seja zero
        float safeRadius = patrolRadius <= 0.1f ? 3f : patrolRadius;
        return _startPosition + Random.insideUnitCircle.normalized * safeRadius;
    }

    private void AdjustFacingDirection()
    {
        Vector2 targetPos = _currentState == State.Patrolling ? _roamPosition : (Vector2)_player.position;
        if (_sprite != null) _sprite.flipX = targetPos.x < transform.position.x;
    }

    // --- MAGIA DE ARQUITETURA: VISUALIZADOR ---
    // Isto vai desenhar as áreas na Unity para você saber exatamente o que o inimigo vê!
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; // Área que ele pode passear
        Gizmos.DrawWireSphere(Application.isPlaying ? _startPosition : transform.position, patrolRadius);

        Gizmos.color = Color.yellow; // Área que ativa a perseguição
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red; // Área onde ele ataca
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}