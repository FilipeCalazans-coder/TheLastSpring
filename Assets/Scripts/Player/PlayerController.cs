using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PlayerController : MonoBehaviour
{
    public bool FacingLeft { get { return facingLeft; } }
    public static PlayerController Instance;

    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float dashSpeed = 4f;
    [SerializeField] private TrailRenderer myTrailRenderer;


    private PlayerControls playerControls;
    private Vector2 movement;
    private Rigidbody2D rb;
    private Animator myAnimator;
    private SpriteRenderer mySpriteRenderer;
    private float startingMoveSpeed;
    private bool facingLeft = false;
    private bool isDashing = false;


    private void Start()
    {
        playerControls.Combat.Dash.performed += _ => Dash();
        startingMoveSpeed = moveSpeed;
    }

    private void Awake()
    {
        Instance = this;
        playerControls = new PlayerControls();
        rb = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        mySpriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void Update()
    {
        PlayerInput();
    }

    private void FixedUpdate()
    {
        AdjustPlayerFacingDirection();
        Move();
    }

    private void PlayerInput()
    {
        movement = playerControls.Movement.Move.ReadValue<Vector2>();

        myAnimator.SetFloat("moveX", movement.x);
        myAnimator.SetFloat("moveY", movement.y);
    }

    private void Move()
    {
        rb.MovePosition(rb.position + movement * (moveSpeed * Time.fixedDeltaTime));
    }

    private void AdjustPlayerFacingDirection()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPos = Camera.main.WorldToScreenPoint(transform.position);
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

    private void Dash()
    {
        // Se já estiver dando dash ou não tiver estamina suficiente, cancela
        if (isDashing) return;

        PlayerStamina stamina = GetComponent<PlayerStamina>();
        
        // Tentamos gastar 25 de estamina
        if (stamina != null && stamina.TrySpendStamina(25f))
        {
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        
        // BUSCAMOS o script de vida para ligar a invencibilidade
        PlayerHealth health = GetComponent<PlayerHealth>();
        yield return new WaitForSeconds(0.05f); // Pequeno atraso antes de ficar invencível
        if (health != null) health.isInvulnerable = true;
        // No início do Dash
        mySpriteRenderer.color = new Color(1f, 1f, 1f, 0.5f); // 50% de transparência

        moveSpeed *= dashSpeed;
        myTrailRenderer.emitting = true;

        // Duração do Dash (e dos I-Frames)
        yield return new WaitForSeconds(0.1f); // Janela real de invencibilidade (curta!)
        
        moveSpeed = startingMoveSpeed;
        myTrailRenderer.emitting = false;

        // DESLIGAMOS a invencibilidade assim que o dash acaba
        if (health != null) health.isInvulnerable = false;
        yield return new WaitForSeconds(0.05f); // Vulnerável no final do movimento
        // No final do Dash
        mySpriteRenderer.color = new Color(1f, 1f, 1f, 1f); // Volta ao normal

        // Tempo de espera para usar de novo
        yield return new WaitForSeconds(0.25f);
        isDashing = false;
    }
}