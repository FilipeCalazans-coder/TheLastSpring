using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Project.Scripts.Player;

public class PlayerController : MonoBehaviour
{
    public bool FacingLeft { get { return facingLeft; } }
    public static PlayerController Instance;

    [Header("Configurações de Movimento")]
    [SerializeField] private float baseMoveSpeed = 1f;
    [SerializeField] private float dashSpeedMultiplier = 4f;
    [SerializeField] private TrailRenderer myTrailRenderer;

    [Header("Habilidades Especiais")]
    [SerializeField] private GameObject landminePrefab; // O objeto da mina que vamos atirar
    [SerializeField] private float skillCooldown = 2f;  // Tempo de espera entre cada mina
    private float _lastSkillTime;

    [Header("Skill 2: Dash com Dano")]
    [SerializeField] private float dashDamageRadius = 1.2f; // Tamanho da área de dano em volta dela
    [SerializeField] private int dashDamage = 30; // Dano do dash
    [SerializeField] private float skill2Cooldown = 3f; // Tempo de espera
    private float _lastSkill2Time;

    [Header("Skill 3: Afastamento (Explosão de Vento)")]
    [SerializeField] private float afastamentoRadius = 2.5f; // Tamanho da área da explosão
    [SerializeField] private float afastamentoForce = 20f;   // A força com que os inimigos são atirados para trás
    [SerializeField] private int afastamentoDamage = 10;     // Um bocadinho de dano opcional
    [SerializeField] private float skill3Cooldown = 4f;      // Tempo de espera
    private float _lastSkill3Time;

    [Header("Skill 4: Teleporte (Muda de Raiz)")]
    [SerializeField] private GameObject mudaPrefab; // O objeto da planta que servirá de âncora
    [SerializeField] private float skill4Cooldown = 5f; // Tempo de espera após o teleporte
    private float _lastSkill4Time;
    private GameObject _activeMuda; // O jogo vai guardar a Muda aqui para saber onde ela está

    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator myAnimator;
    private SpriteRenderer mySpriteRenderer;
    private Camera _mainCamera; // OTIMIZAÇÃO: Guarda a câmera na memória
    
    private float _currentSpeed;
    private float _itemSpeedMultiplier = 1f; 
    
    private bool facingLeft = false;
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
        _mainCamera = Camera.main; // OTIMIZAÇÃO APLICADA AQUI
        _resinReceiver = GetComponent<PlayerToxicResinReceiver>();
    }

    private void Start()
    {
        _currentSpeed = baseMoveSpeed;
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Update()
    {
        PlayerInput();
        AdjustPlayerFacingDirection(); // MOVIDO: Input do mouse deve ficar no Update!
    }

    private void FixedUpdate()
    {
        Move(); // BLINDADO: FixedUpdate agora cuida EXCLUSIVAMENTE da física!
    }

    private void PlayerInput()
    {
        movement = playerControls.Movement.Move.ReadValue<Vector2>();

        myAnimator.SetFloat("moveX", movement.x);
        myAnimator.SetFloat("moveY", movement.y);

        if (playerControls.Combat.Dash.triggered)
        {
            Dash();
        }

        // Lê a tecla Z
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

    // --- SISTEMA DE HABILIDADES ---
    private void CastSkill1()
    {
        // 1. Verifica o Cooldown (Espera)
        if (Time.time < _lastSkillTime + skillCooldown) return;

        // 2. Pergunta ao PlayerSkills se o jogador JÁ COMPROU esta habilidade
        PlayerSkills skills = GetComponent<PlayerSkills>();
        if (skills != null && skills.HasSkillByID("minas_terrestres"))
        {
            if (landminePrefab != null)
            {
                Instantiate(landminePrefab, transform.position, Quaternion.identity);
                _lastSkillTime = Time.time;
                Debug.Log("<color=green>Fiorella plantou uma Mina Terrestre com o NOVO Input!</color>");
            }
            else
            {
                Debug.LogWarning("Prefab da Mina não foi colocado no Inspector!");
            }
        }
    }

    private void CastSkill2()
    {
        // Verifica o cooldown e se ela já não está a meio de um dash
        if (Time.time < _lastSkill2Time + skill2Cooldown) return;
        if (isDashing) return;

        PlayerSkills skills = GetComponent<PlayerSkills>();
        if (skills != null && skills.HasSkillByID("dash_dano"))
        {
            StartCoroutine(DamageDashRoutine());
            _lastSkill2Time = Time.time;
            Debug.Log("<color=magenta>Fiorella usou o Dash com Dano!</color>");
        }
    }

    private IEnumerator DamageDashRoutine()
    {
        isDashing = true;
        PlayerHealth health = GetComponent<PlayerHealth>();
        
        // 1. Prepara o Dash (Invulnerabilidade e Cor Vermelha para dar feedback)
        if (health != null) health.isInvulnerable = true;
        mySpriteRenderer.color = new Color(1f, 0.5f, 0.5f, 0.8f); 
        
        _currentSpeed = baseMoveSpeed * dashSpeedMultiplier; // Acelera
        myTrailRenderer.emitting = true;

        float dashTime = 0.15f; // Quanto tempo o dash dura
        float timer = 0f;

        // O nosso "caderno de notas" para evitar dar dano duplo ao mesmo inimigo
        HashSet<EnemyHealth> enemiesHit = new HashSet<EnemyHealth>();

        // 2. Durante o tempo do Dash, verifica a cada frame se tocou num inimigo
        while (timer < dashTime)
        {
            timer += Time.deltaTime;

            // Cria um círculo invisível à volta da Fiorella e apanha tudo o que tocar nele
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, dashDamageRadius);
            
            foreach (Collider2D hit in hitColliders)
            {
                if (hit.CompareTag("Enemy"))
                {
                    EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                    
                    // Se tocou num inimigo e ainda não o anotamos no caderno
                    if (enemy != null && !enemiesHit.Contains(enemy))
                    {
                        enemy.TakeDamage(dashDamage);
                        enemiesHit.Add(enemy); // Anota no caderno
                        Debug.Log("<color=red>Slash! Dash de Dano acertou!</color>");
                    }
                }
            }
            yield return null; // Espera pelO próxima frame
        }

        // 3. O Dash acabou, volta tudo ao normal
        _currentSpeed = baseMoveSpeed;
        myTrailRenderer.emitting = false;

        if (health != null) health.isInvulnerable = false;
        mySpriteRenderer.color = new Color(1f, 1f, 1f, 1f); 

        // Tempo extra de espera antes de poder usar o Dash normal de novo (para evitar spam)
        yield return new WaitForSeconds(0.25f);
        isDashing = false;
    }

    private void CastSkill3()
    {
        // 1. Verifica o tempo de espera (Cooldown)
        if (Time.time < _lastSkill3Time + skill3Cooldown) return;

        // 2. Pergunta ao PlayerSkills se o jogador JÁ COMPROU esta habilidade
        PlayerSkills skills = GetComponent<PlayerSkills>();
        if (skills != null && skills.HasSkillByID("afastamento"))
        {
            // 3. Cria o círculo de explosão em volta da Fiorella
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, afastamentoRadius);
            
            // Vamos adicionar um pequeno feedback visual à Fiorella (ficar branca rápido)
            StartCoroutine(FlashAfastamentoRoutine());

            foreach (Collider2D hit in hitColliders)
            {
                // Se a explosão apanhou um inimigo...
                if (hit.CompareTag("Enemy"))
                {
                    // Empurra-o para trás usando o teu script de Knockback!
                    KnockBack kb = hit.GetComponent<KnockBack>();
                    if (kb != null)
                    {
                        // O 'transform' é a Fiorella, para o inimigo saber de onde vem o empurrão e voar para o lado oposto
                        kb.GetKnockedBack(this.transform, afastamentoForce); 
                    }

                    // Aplica um pequeno dano
                    EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
                    if (enemy != null)
                    {
                        enemy.TakeDamage(afastamentoDamage);
                    }
                }
            }

            _lastSkill3Time = Time.time;
            Debug.Log("<color=cyan>Habilidade 3: Afastamento ativado!</color>");
        }
    }

    // Um pequeno truque visual para parecer que ela libertou energia
    private IEnumerator FlashAfastamentoRoutine()
    {
        mySpriteRenderer.color = new Color(0.5f, 1f, 0.5f, 1f); // Fica num tom esverdeado brilhante (natureza)
        yield return new WaitForSeconds(0.15f);
        mySpriteRenderer.color = Color.white; // Volta ao normal
    }

    private void CastSkill4()
    {
        PlayerSkills skills = GetComponent<PlayerSkills>();
        
        // Verifica se a Fiorella já comprou a habilidade
        if (skills != null && skills.HasSkillByID("teleporte"))
        {
            // ----------------------------------------------------
            // FASE 1: Se a muda AINDA NÃO existe, vamos plantá-la
            // ----------------------------------------------------
            if (_activeMuda == null)
            {
                // Só podemos plantar se o Cooldown já tiver acabado
                if (Time.time < _lastSkill4Time + skill4Cooldown) return;

                if (mudaPrefab != null)
                {
                    // Cria a muda e GUARDA A REFERÊNCIA na variável _activeMuda
                    _activeMuda = Instantiate(mudaPrefab, transform.position, Quaternion.identity);
                    Debug.Log("<color=green>Muda plantada! Prime 'V' de novo para teleportar.</color>");
                }
                else
                {
                    Debug.LogWarning("O Prefab da Muda não está configurado no Inspector!");
                }
            }
            // ----------------------------------------------------
            // FASE 2: Se a muda JÁ EXISTE, vamos teleportar
            // ----------------------------------------------------
            else
            {
                // Move a Fiorella magicamente para a posição exata da muda
                transform.position = _activeMuda.transform.position;
                
                // Dá um pequeno efeito visual para não ser um corte tão seco
                StartCoroutine(TeleportFlashRoutine());

                // Destrói a muda, porque já foi usada
                Destroy(_activeMuda);
                _activeMuda = null; // Limpa a memória para permitir plantar uma nova no futuro

                // Começa a contar o tempo de espera APÓS o teleporte ser concluído
                _lastSkill4Time = Time.time;
                Debug.Log("<color=cyan>Fiorella teleportou-se com sucesso!</color>");
            }
        }
    }

    // Um pequeno efeito de "piscar" verde para dar a sensação de magia da natureza
    private IEnumerator TeleportFlashRoutine()
    {
        PlayerHealth health = GetComponent<PlayerHealth>();
        
        // Dá um pingo de invulnerabilidade no frame do teleporte para ela não morrer assim que chegar
        if (health != null) health.isInvulnerable = true;
        
        mySpriteRenderer.color = new Color(0f, 1f, 0.5f, 0.5f); // Verde transparente
        yield return new WaitForSeconds(0.2f);
        mySpriteRenderer.color = Color.white; // Volta ao normal
        
        if (health != null) health.isInvulnerable = false;
    }

    private void Move()
    {
        float resinMultiplier = (_resinReceiver != null) ? _resinReceiver.CurrentSpeedMultiplier : 1f;
        float finalSpeed = _currentSpeed * _itemSpeedMultiplier * resinMultiplier;
        rb.MovePosition(rb.position + movement * (finalSpeed * Time.fixedDeltaTime));
    }

    private void AdjustPlayerFacingDirection()
    {
        if (_mainCamera == null) return; // Evita erros se a câmera sumir

        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPos = _mainCamera.WorldToScreenPoint(transform.position);
        
        if (mousePos.x < playerScreenPos.x)
        {
            mySpriteRenderer.flipX = true;
            facingLeft = true;
        }
        else
        {
            mySpriteRenderer.flipX = false;
            facingLeft = false;
        }
    }

    // --- SISTEMA DE DASH ---
    private void Dash()
    {
        if (isDashing) return;

        PlayerStamina stamina = GetComponent<PlayerStamina>();
        
        if (stamina != null && stamina.TrySpendStamina(25f))
        {
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

        _currentSpeed = baseMoveSpeed * dashSpeedMultiplier;
        myTrailRenderer.emitting = true;

        yield return new WaitForSeconds(0.1f); 
        
        _currentSpeed = baseMoveSpeed;
        myTrailRenderer.emitting = false;

        if (health != null) health.isInvulnerable = false;
        yield return new WaitForSeconds(0.05f); 
        
        mySpriteRenderer.color = new Color(1f, 1f, 1f, 1f); 

        yield return new WaitForSeconds(0.25f);
        isDashing = false;
    }

    // --- SISTEMA DE BUFFS (Itens) ---
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