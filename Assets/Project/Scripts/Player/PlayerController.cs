using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public bool FacingLeft { get { return facingLeft; } }
    public static PlayerController Instance;

    [Header("Configurações de Movimento")]
    [SerializeField] private float baseMoveSpeed = 1f;
    [SerializeField] private float dashSpeedMultiplier = 4f;
    [SerializeField] private TrailRenderer myTrailRenderer;

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