using UnityEngine;
using System.Collections;

public class EnemyIA : MonoBehaviour
{
    private enum State { Patrolling, Chasing, Attacking }
    private State _currentState = State.Patrolling;

    [Header("Configurações de Visão e Ataque")]
    public float chaseRange = 5f;
    public float attackRange = 1.2f;

    [Header("Origem da Detecção")]
    [Tooltip("Offset aplicado ao transform.position para definir o ponto efetivo de detecção e dano.")]
    public Vector2 detectionOffset = new Vector2(0f, 0.3f);

    [Header("Configurações de Patrulha")]
    public float patrolRadius = 3f;         
    public float waitTimeBetweenPatrols = 2f;

    [Header("Charge Attack")]
    [Tooltip("Multiplicador da animação durante pausas (preparação e recovery). 0.3 = 30% da velocidade normal.")]
    [Range(0f, 1f)]
    public float pauseAnimationSpeedMultiplier = 0.3f;

    [Tooltip("Tempo parado preparando o charge (continua atualizando direção do player).")]
    public float chargePrepareTime = 0.5f;
    [Tooltip("Multiplicador aplicado à velocidade base durante o charge (ex.: 3 = triplo da velocidade).")]
    public float chargeSpeedMultiplier = 3f;
    [Tooltip("Duração total do charge (acelerando + desacelerando).")]
    public float chargeDuration = 0.8f;
    [Range(0.1f, 0.95f)]
    [Tooltip("Em que porcentagem da duração começa a desaceleração. 0.75 = começa a desacelerar aos 75% do tempo.")]
    public float chargeDecayStartPercent = 0.75f;
    [Tooltip("Tempo parado após o charge (exaustão).")]
    public float recoveryDelay = 0.5f;
    [Tooltip("Quanto tempo leva pra voltar à velocidade default depois do recovery.")]
    public float recoveryAccelerationTime = 1.5f;
    [Tooltip("Distância mínima que o charge percorre antes de poder começar a desacelerar (mesmo que o tempo de decay já tenha chegado). Evita charges curtos quando o player está colado.")]
    public float chargeMinimumDistance = 2.0f;

    private Transform _player;
    private EnemyPathFinding _pathFinding;
    private SpriteRenderer[] _sprites;
    private Animator[] _animators;

    private Vector2 _startPosition;
    private Vector2 _roamPosition;
    private float _patrolTimer;
    private bool _isAttacking = false;
    private bool _isCharging = false;
    private float _baseMoveSpeed;       // Cache da velocidade default
    private float _baseAnimatorSpeed;   // Cache do speed default do Animator

    /// <summary>Exposto para o EnemyHealth saber se deve reduzir o knockback durante charge.</summary>
    public bool IsCharging => _isCharging;

    private void Awake()
    {
        _pathFinding = GetComponent<EnemyPathFinding>();
        // Pega TODOS os SpriteRenderers e Animators dos filhos (Sprite_Fill, Sprite_Stroke)
        _sprites = GetComponentsInChildren<SpriteRenderer>();
        _animators = GetComponentsInChildren<Animator>();
    }

    private void Start()
    {
        if (PlayerController.Instance != null) _player = PlayerController.Instance.transform;

        _startPosition = transform.position;
        _roamPosition = GetRandomRoamPosition();

        // Cacheia velocidades default para o charge poder restaurar
        _baseMoveSpeed = _pathFinding.moveSpeed;
        _baseAnimatorSpeed = (_animators != null && _animators.Length > 0) ? _animators[0].speed : 1f;
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

        Vector2 detectionOrigin = (Vector2)transform.position + detectionOffset;
        float distanceToPlayer = Vector2.Distance(detectionOrigin, (Vector2)_player.position);

        switch (_currentState)
        {
            case State.Patrolling:
                PatrolLogic();
                if (distanceToPlayer < chaseRange) 
                {
                    _currentState = State.Chasing;
                    
                    // NOVO: Quando começar a perseguir, toca a música de combate!
                    if (Project.Scripts.Audio.AudioManager.Instance != null)
                        Project.Scripts.Audio.AudioManager.Instance.PlayCombatMusic();
                }
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
                    
                    // NOVO: Quando o inimigo desistir e voltar a patrulhar, volta a música calma!
                    if (Project.Scripts.Audio.AudioManager.Instance != null)
                        Project.Scripts.Audio.AudioManager.Instance.PlayAmbientMusic();
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

        // FASE 1 — PREPARING: para, encara o player, atualiza direção até o último frame

        _pathFinding.StopMoving();
        // Desacelera a animação durante a pausa de preparação
        if (_animators != null)
        {
            foreach (var anim in _animators)
            {
                if (anim != null) anim.speed = _baseAnimatorSpeed * pauseAnimationSpeedMultiplier;
            }
        }

        float prepareElapsed = 0f;
        Vector2 lockedDirection = Vector2.right; // fallback
        const float MIN_TRACKING_DISTANCE = 0.5f; // abaixo disso, congela a direção

        // Faz uma primeira leitura imediata pra garantir uma direção inicial decente
        if (_player != null)
        {
            Vector2 origin = (Vector2)transform.position + detectionOffset;
            Vector2 toPlayer = (Vector2)_player.position - origin;
            if (toPlayer.sqrMagnitude > 0.0001f)
                lockedDirection = toPlayer.normalized;
        }

        while (prepareElapsed < chargePrepareTime)
        {
            if (_player != null)
            {
                Vector2 origin = (Vector2)transform.position + detectionOffset;
                Vector2 toPlayer = (Vector2)_player.position - origin;

                // Só atualiza a direção se o player NÃO está em cima do inimigo —
                // evita rotação instável quando há sobreposição.
                if (toPlayer.magnitude > MIN_TRACKING_DISTANCE)
                {
                    lockedDirection = toPlayer.normalized;
                }
            }

            prepareElapsed += Time.deltaTime;
            yield return null;
        }

        // FASE 2 — CHARGING: avança em linha reta na direção travada
        // Acelera no início, desacelera após chargeDecayStartPercent

        Vector2 chargeStartPos = (Vector2)transform.position;

        _isCharging = true;

        float chargeElapsed = 0f;
        float peakSpeed = _baseMoveSpeed * chargeSpeedMultiplier;
        float decayStartTime = chargeDuration * chargeDecayStartPercent;

        _pathFinding.MoveInDirection(lockedDirection);

        int frameCount = 0;
        Vector2 lastPos = chargeStartPos;

        while (chargeElapsed < chargeDuration)
        {
            float currentSpeed;
            float distanceCovered = Vector2.Distance((Vector2)transform.position, chargeStartPos);

            bool canDecay = chargeElapsed >= decayStartTime
                            && distanceCovered >= chargeMinimumDistance;

            if (!canDecay)
            {
                currentSpeed = peakSpeed;
            }
            else
            {
                float decayProgress = (chargeElapsed - decayStartTime) / (chargeDuration - decayStartTime);
                currentSpeed = Mathf.Lerp(peakSpeed, 0f, Mathf.Clamp01(decayProgress));
            }

            _pathFinding.moveSpeed = currentSpeed;

            if (_animators != null)
            {
                float speedRatio = (_baseMoveSpeed > 0.0001f) ? currentSpeed / _baseMoveSpeed : 1f;
                foreach (var anim in _animators)
                {
                    if (anim != null) anim.speed = _baseAnimatorSpeed * speedRatio;
                }
            }

            // DIAGNÓSTICO POR FRAME (loga a cada 10 frames pra não poluir)
            frameCount++;
            if (frameCount % 10 == 0)
            {
                Vector2 currentPos = (Vector2)transform.position;
                float deltaMoved = Vector2.Distance(currentPos, lastPos);
                float expectedDelta = currentSpeed * (Time.deltaTime * 10f);
                Debug.Log($"[Charge Frame {frameCount}] currentSpeed: {currentSpeed:F2} | deltaMovido: {deltaMoved:F3} | esperado: {expectedDelta:F3} | totalCovered: {distanceCovered:F2}");
                lastPos = currentPos;
            }

            chargeElapsed += Time.deltaTime;
            yield return null;
        }

        _isCharging = false;
        _pathFinding.moveSpeed = 0f;
        _pathFinding.StopMoving();
        if (_animators != null)
        {
            foreach (var anim in _animators)
            {
                if (anim != null) anim.speed = _baseAnimatorSpeed;
            }
        }

        // FASE 3 — RECOVERING: parado por um delay (exaustão)
        // Animação desacelerada para dar sensação de exaustão
        // ============================================================
        if (_animators != null)
        {
            foreach (var anim in _animators)
            {
                if (anim != null) anim.speed = _baseAnimatorSpeed * pauseAnimationSpeedMultiplier;
            }
        }
        yield return new WaitForSeconds(recoveryDelay);

        // Libera só o estado de perseguição (pra voltar a se mover),
        // MAS mantém _isAttacking = true pra impedir novo charge durante aceleração.
        _currentState = State.Chasing;
        _pathFinding.moveSpeed = 0f;

        // FASE 4 — ACCELERATING: velocidade cresce gradualmente até o default

        float accelElapsed = 0f;
        while (accelElapsed < recoveryAccelerationTime)
        {
            float t = accelElapsed / recoveryAccelerationTime;
            _pathFinding.moveSpeed = Mathf.Lerp(0f, _baseMoveSpeed, t);

            accelElapsed += Time.deltaTime;
            yield return null;
        }

        _pathFinding.moveSpeed = _baseMoveSpeed;

        // Restaura a velocidade default da animação
        if (_animators != null)
        {
            foreach (var anim in _animators)
            {
                if (anim != null) anim.speed = _baseAnimatorSpeed;
            }
        }

        // Agora sim, libera para um novo charge poder começar
        _isAttacking = false;
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
        bool flip = targetPos.x > transform.position.x;
        if (_sprites != null)
        {
            foreach (var sr in _sprites)
            {
                if (sr != null) sr.flipX = flip;
            }
        }
    }

    // Desenha as áreas na Unity para saber o que o inimigo detecta
    private void OnDrawGizmosSelected()
    {
        Vector3 gizmoOrigin = transform.position + new Vector3(detectionOffset.x, detectionOffset.y, 0f);
        Vector3 patrolOrigin = Application.isPlaying
            ? new Vector3(_startPosition.x, _startPosition.y, 0f) + new Vector3(detectionOffset.x, detectionOffset.y, 0f)
            : gizmoOrigin;

        Gizmos.color = Color.green;  // Área de patrulha
        Gizmos.DrawWireSphere(patrolOrigin, patrolRadius);

        Gizmos.color = Color.yellow; // Área de perseguição
        Gizmos.DrawWireSphere(gizmoOrigin, chaseRange);

        Gizmos.color = Color.red;    // Área de ataque
        Gizmos.DrawWireSphere(gizmoOrigin, attackRange);
    }

    // Método utilitário chamado pelo EnemyHealth quando o Brotinho de Resplendor é ativado
    public void ResetSpawnAnchor(Vector2 newSpawnPos)
    {
        _startPosition = newSpawnPos;
        _currentState = State.Patrolling; // Força o monstro a voltar a patrulhar em paz
        _isAttacking = false;
        _patrolTimer = 0f;
        _roamPosition = GetRandomRoamPosition(); // Sorteia um novo ponto baseado no spawn resetado
    }
}