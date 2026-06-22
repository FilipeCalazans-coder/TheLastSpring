using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Project.Scripts.Player;
using Project.Scripts.Dialogue; // Adicionado para acessar os eventos de diálogo

public class PlayerController : MonoBehaviour
{
    public bool FacingLeft { get { return facingLeft; } }
    public static PlayerController Instance;

    [Header("Configurações de Movimento")]
    [SerializeField] private float baseMoveSpeed = 1f;
    [SerializeField] private float dashSpeedMultiplier = 4f;
    [SerializeField] private TrailRenderer myTrailRenderer;
    
    // ==========================================
    // Efeito Visual em Sprite para o Dash
    // ==========================================
    [SerializeField] private GameObject dashVFXPrefab;
    
    [Header("Habilidades Especiais")]
    [SerializeField] private GameObject landminePrefab; 
    [SerializeField] private float skillCooldown = 2f;  
    private float _lastSkillTime;

    [Header("Skill 2: Dash com Dano")]
    [SerializeField] private float dashDamageRadius = 1.2f; 
    [SerializeField] private int dashDamage = 30; 
    [SerializeField] private float skill2Cooldown = 3f; 
    private float _lastSkill2Time;

    [Header("Skill 3: Afastamento (Explosão de Vento)")]
    [SerializeField] private float afastamentoRadius = 2.5f; 
    [SerializeField] private float afastamentoForce = 20f;   
    [SerializeField] private int afastamentoDamage = 10;     
    [SerializeField] private float skill3Cooldown = 4f;      

    [SerializeField] private GameObject afastamentoVFXPrefab; 
    private float _lastSkill3Time;

    [Header("Skill 4: Teleporte (Muda de Raiz)")]
    [SerializeField] private GameObject mudaPrefab; 
    [SerializeField] private float skill4Cooldown = 5f; 
    private float _lastSkill4Time;
    private GameObject _activeMuda; 

    // ============================================================
    // [SISTEMA DE VFX] NOVAS REFERÊNCIAS PARA O BROTINHO MÁGICO
    // ============================================================
    [Header("VFX Visual do Brotinho Mágico")]
    [Tooltip("Efeito visual rápido que aparece no pé da Fiorella ao plantar.")]
    [SerializeField] private GameObject vfxPlantarPrefab;

    [Tooltip("Efeito visual que aparece tanto na origem quanto no destino do teleporte.")]
    [SerializeField] private GameObject vfxTeleportePrefab;

    private PlayerControls playerControls;
    private Vector2 movement;
    
    // [SISTEMA DE DIREÇÃO E CONTROLE]
    private Vector2 _lastMoveDirection = Vector2.right; 
    private Vector2 _dashDirection; 
    private bool _canMove = true; // NOVO: Variável para travar a Okyra durante diálogos

    private Rigidbody2D rb;
    private Animator myAnimator;
    private SpriteRenderer mySpriteRenderer;
    private Camera _mainCamera; 
    
    private float _currentSpeed;
    private float _itemSpeedMultiplier = 1f;      
    private bool facingLeft = false;
    private bool isKnockedBack = false;
    private bool isDashing = false;
    public bool IsDashing => isDashing;

    private PlayerToxicResinReceiver _resinReceiver;

    private void Awake()
    {
        Instance = this;
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
        _mainCamera = Camera.main; 
        _resinReceiver = GetComponent<PlayerToxicResinReceiver>();
    }

    private void Start()
    {
        _currentSpeed = baseMoveSpeed;
        isDashing = false;
    }

    private void OnEnable()
    {
        playerControls.Enable();
        // NOVO: Assina o evento global de diálogo
        DialogueManager.OnDialogueStateChanged += HandlePlayerFreeze;
    }

    private void OnDisable()
    {
        playerControls.Disable();
        // NOVO: Remove a assinatura para evitar vazamentos de memória
        DialogueManager.OnDialogueStateChanged -= HandlePlayerFreeze;
    }

    // NOVO: Método que processa a trava de movimentação
    private void HandlePlayerFreeze(bool freeze)
    {
        _canMove = !freeze;

        if (freeze)
        {
            // Zera o movimento para impedir deslizamentos residuais
            movement = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            
            // Força a animação a voltar para o estado "Idle"
            if (myAnimator != null)
            {
                myAnimator.SetFloat("moveX", 0);
                myAnimator.SetFloat("moveY", 0);
            }
        }
    }

    private void Update()
    {
        // NOVO: Trava de segurança. Se não puder mover, não lê os inputs do frame
        if (!_canMove) return;

        PlayerInput();
        AdjustPlayerFacingDirection(); 
    }

    private void FixedUpdate()
    {
        // NOVO: Trava de segurança para a física
        if (!_canMove) return;

        Move(); 
    }

    private void PlayerInput()
    {
        if (Time.timeScale == 0f)
        {
            movement = Vector2.zero;
            myAnimator.SetFloat("moveX", 0);
            myAnimator.SetFloat("moveY", 0);
            return; 
        }

        movement = playerControls.Movement.Move.ReadValue<Vector2>();
        
        if (movement != Vector2.zero)
        {
            _lastMoveDirection = movement.normalized;
        }

        myAnimator.SetFloat("moveX", movement.x);
        myAnimator.SetFloat("moveY", movement.y);

        if (playerControls.Combat.Dash.triggered)
        {
            Dash();
        }

        if (playerControls.Combat.Skill1.triggered)
        {
            CastSkill1();
        }

        if (playerControls.Combat.Skill2.triggered)
        {
            CastSkill2();
        }

        if (playerControls.Combat.Skill3.triggered)
        {
            CastSkill3();
        }

        if (playerControls.Combat.Skill4.triggered)
        {
            CastSkill4();
        }
    }

    private void CastSkill1()
    {
        if (Time.time < _lastSkillTime + skillCooldown) return;

        PlayerSkills skills = GetComponent<PlayerSkills>();
        if (skills != null && skills.HasSkillByID("minas_terrestres"))
        {
            if (landminePrefab != null)
            {
                Instantiate(landminePrefab, transform.position, Quaternion.identity);
                _lastSkillTime = Time.time;
                Debug.Log("<color=green>Fiorella plantou uma Mina Terrestre com o NOVO Input!</color>");
            }
        }
    }

    private void CastSkill2()
    {
        if (Time.time < _lastSkill2Time + skill2Cooldown) return;
        if (isDashing) return;

        PlayerSkills skills = GetComponent<PlayerSkills>();
        if (skills != null && skills.HasSkillByID("dash_dano"))
        {
            _dashDirection = (movement != Vector2.zero) ? movement.normalized : _lastMoveDirection;

            StartCoroutine(DamageDashRoutine());
            _lastSkill2Time = Time.time;
            Debug.Log("<color=magenta>Fiorella usou o Dash com Dano!</color>");
        }
    }

    private IEnumerator DamageDashRoutine()
    {
        isDashing = true;
        PlayerHealth health = GetComponent<PlayerHealth>();
        
        if (health != null) health.isInvulnerable = true;
        mySpriteRenderer.color = new Color(1f, 0.5f, 0.5f, 0.8f); 
        
        _currentSpeed = baseMoveSpeed * dashSpeedMultiplier; 
        myTrailRenderer.emitting = true;

        float dashTime = 0.15f; 
        float timer = 0f;
        HashSet<EnemyHealth> enemiesHit = new HashSet<EnemyHealth>();

        while (timer < dashTime)
        {
            timer += Time.deltaTime;
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, dashDamageRadius);
            
            foreach (Collider2D hit in hitColliders)
            {
                if (hit.CompareTag("Enemy"))
                {
                    EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                    
                    if (enemy != null && !enemiesHit.Contains(enemy))
                    {
                        enemy.TakeDamage(dashDamage);
                        enemiesHit.Add(enemy); 
                        Debug.Log("<color=red>Slash! Dash de Dano acertou!</color>");
                    }
                }
            }
            yield return null; 
        }

        _currentSpeed = baseMoveSpeed;
        myTrailRenderer.emitting = false;
        if (health != null) health.isInvulnerable = false;
        mySpriteRenderer.color = new Color(1f, 1f, 1f, 1f); 

        yield return new WaitForSeconds(0.25f);
        isDashing = false;
    }

    private void CastSkill3()
    {
        if (Time.time < _lastSkill3Time + skill3Cooldown) return;

        PlayerSkills skills = GetComponent<PlayerSkills>();
        if (skills != null && skills.HasSkillByID("afastamento"))
        {
            if (afastamentoVFXPrefab != null)
            {
                Instantiate(afastamentoVFXPrefab, transform.position, Quaternion.identity);
            }

            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, afastamentoRadius);
            StartCoroutine(FlashAfastamentoRoutine());

            foreach (Collider2D hit in hitColliders)
            {
                if (hit.CompareTag("Enemy"))
                {
                    KnockBack kb = hit.GetComponent<KnockBack>();
                    if (kb != null)
                    {
                        kb.GetKnockedBack(this.transform, afastamentoForce); 
                    }

                    EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(afastamentoDamage);
                    }
                }
            }
            _lastSkill3Time = Time.time;
            Debug.Log("<color=cyan>Habilidade 3: Afastamento ativado com VFX!</color>");
        }
    }

    private IEnumerator FlashAfastamentoRoutine()
    {
        mySpriteRenderer.color = new Color(0.5f, 1f, 0.5f, 1f); 
        yield return new WaitForSeconds(0.15f);
        mySpriteRenderer.color = Color.white; 
    }

    private void CastSkill4()
    {
        PlayerSkills skills = GetComponent<PlayerSkills>();
        
        if (skills != null && skills.HasSkillByID("teleporte"))
        {
            if (_activeMuda == null)
            {
                if (Time.time < _lastSkill4Time + skill4Cooldown) return;
                
                if (mudaPrefab != null)
                {
                    _activeMuda = Instantiate(mudaPrefab, transform.position, Quaternion.identity);
                    
                    if (vfxPlantarPrefab != null)
                    {
                        Instantiate(vfxPlantarPrefab, transform.position, Quaternion.identity);
                    }
                    
                    Debug.Log("<color=green>Muda plantada! Prime 'V' de novo para teleportar.</color>");
                }
            }
            else
            {
                if (vfxTeleportePrefab != null)
                {
                    Instantiate(vfxTeleportePrefab, transform.position, Quaternion.identity);
                }

                transform.position = _activeMuda.transform.position;

                if (vfxTeleportePrefab != null)
                {
                    Instantiate(vfxTeleportePrefab, transform.position, Quaternion.identity);
                }

                StartCoroutine(TeleportFlashRoutine());
                
                Destroy(_activeMuda);
                _activeMuda = null; 
                _lastSkill4Time = Time.time;
                Debug.Log("<color=cyan>Fiorella teleportou-se com sucesso!</color>");
            }
        }
    }

    private IEnumerator TeleportFlashRoutine()
    {
        PlayerHealth health = GetComponent<PlayerHealth>();
        
        if (health != null) health.isInvulnerable = true;
        
        mySpriteRenderer.color = new Color(0f, 1f, 0.5f, 0.5f); 
        yield return new WaitForSeconds(0.2f);
        mySpriteRenderer.color = Color.white; 
        
        if (health != null) health.isInvulnerable = false;
    }

    private void Move()
    {
        if (isKnockedBack) return; 

        float resinMultiplier = (_resinReceiver != null) ? _resinReceiver.CurrentSpeedMultiplier : 1f;
        float finalSpeed = _currentSpeed * _itemSpeedMultiplier * resinMultiplier;

        Vector2 activeDirection = isDashing ? _dashDirection : movement;

        rb.MovePosition(rb.position + activeDirection * (finalSpeed * Time.fixedDeltaTime));
    }

    private void AdjustPlayerFacingDirection()
    {
        if (movement.x < 0)
        {
            mySpriteRenderer.flipX = true;
            facingLeft = true;
        }
        else if (movement.x > 0)
        {
            mySpriteRenderer.flipX = false;
            facingLeft = false;
        }
    }

    private void Dash()
    {
        if (isDashing) return;
        PlayerStamina stamina = GetComponent<PlayerStamina>();
        
        if (stamina != null && stamina.TrySpendStamina(25f))
        {
            _dashDirection = (movement != Vector2.zero) ? movement.normalized : _lastMoveDirection;
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        
        PlayerHealth health = GetComponent<PlayerHealth>();
        yield return new WaitForSeconds(0.05f); 
        
        if (health != null) health.isInvulnerable = true;
        mySpriteRenderer.color = new Color(1f, 1f, 1f, 0.5f); 

        if (dashVFXPrefab != null)
        {
            GameObject vfx = Instantiate(dashVFXPrefab, transform.position, Quaternion.identity);
            SpriteRenderer vfxSprite = vfx.GetComponent<SpriteRenderer>();
            
            if (vfxSprite != null)
            {
                vfxSprite.flipX = mySpriteRenderer.flipX; 
            }
        }

        _currentSpeed = baseMoveSpeed * dashSpeedMultiplier;
        
        if (myTrailRenderer != null) myTrailRenderer.emitting = true;
        
        yield return new WaitForSeconds(0.1f); 
        
        _currentSpeed = baseMoveSpeed;
        if (myTrailRenderer != null) myTrailRenderer.emitting = false;
        if (health != null) health.isInvulnerable = false;
        
        yield return new WaitForSeconds(0.05f); 
        
        mySpriteRenderer.color = new Color(1f, 1f, 1f, 1f); 
        yield return new WaitForSeconds(0.25f);
        
        isDashing = false;
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration = 0.2f)
    {
        StartCoroutine(KnockbackRoutine(direction, force, duration));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float force, float duration)
    {
        isKnockedBack = true;
        isDashing = false; 

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            rb.MovePosition(rb.position + direction * (force * Time.deltaTime));
            yield return null;
        }

        isKnockedBack = false;
    }

    public void ApplySpeedBuff(float multiplier, float duration)
    {
        StartCoroutine(SpeedBuffRoutine(multiplier, duration));
    }

    private IEnumerator SpeedBuffRoutine(float multiplier, float duration)
    {
        _itemSpeedMultiplier = multiplier; 
        yield return new WaitForSeconds(duration); 
        _itemSpeedMultiplier = 1f; 
    }
}