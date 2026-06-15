using UnityEngine;
using System.Collections;

public class Sword : MonoBehaviour
{
    [SerializeField] private GameObject slashAnimPrefab;
    [SerializeField] private Transform slashAnimSpawnPoint;
    [SerializeField] private Transform weaponCollider;
    [SerializeField] private float swordAttackCD = 0.5f;

    private PlayerControls playerControls;
    private Animator myAnimator;
    private PlayerController playerController;
    private ActiveWeapon activeWeapon;
    private GameObject slashAnim;
    private bool attackButtonDown, isAttacking = false;

    private void Awake()
    {
        playerController = GetComponentInParent<PlayerController>();
        activeWeapon = GetComponentInParent<ActiveWeapon>();
        myAnimator = GetComponent<Animator>();
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    void Start()
    {
        playerControls.Combat.Attack.started += _ => StartAttacking();
        playerControls.Combat.Attack.canceled += _ => StopAttacking();
    }

    private void Update()
    {
        // NOVO: Agora a arma se sincroniza com a Fiorella em vez do Mouse!
        SyncWeaponDirection();
        Attack();
    }

    private void StartAttacking()
    {
        attackButtonDown = true;
    }

    private void StopAttacking()
    {
        attackButtonDown = false;
    }

    private void Attack()
    {
        // ==========================================
        // CORREÇÃO: Impede a espadada e o gasto de estamina no pause
        // ==========================================
        if (Time.timeScale == 0f) return;

        if (attackButtonDown && !isAttacking)
        {
            PlayerStamina stamina = GetComponentInParent<PlayerStamina>();

            if (stamina != null && stamina.TrySpendStamina(15f))
            {
                isAttacking = true;
                myAnimator.SetTrigger("Attack");
                weaponCollider.gameObject.SetActive(true);
                
                // Cria o efeito de corte
                slashAnim = Instantiate(slashAnimPrefab, slashAnimSpawnPoint.position, Quaternion.identity);
                // Coloca o efeito DENTRO do objeto ActiveWeapon para herdar a rotação dele
                slashAnim.transform.parent = this.transform.parent; 
                
                StartCoroutine(AttackCDRoutine());
            }
        }
    }

    private IEnumerator AttackCDRoutine()
    {
        yield return new WaitForSeconds(swordAttackCD);
        isAttacking = false;
    }

    public void DoneAttackingAnimEvent()
    {
        weaponCollider.gameObject.SetActive(false);
    }

    // ==========================================
    // ANIMAÇÕES DE CORTE LOCALIZADAS
    // ==========================================
    public void SwingUpFlipAnimEvent()
    {
        if (slashAnim != null)
        {
            // Gira o efeito no próprio eixo para dar a sensação de combo
            slashAnim.transform.localRotation = Quaternion.Euler(-180, 0, 0);
        }
    }

    public void SwingDownFlipAnimEvent()
    {
        if (slashAnim != null)
        {
            slashAnim.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    // ==========================================
    // NOVO: SINCRONIZAÇÃO COM O PLAYER (TECLADO)
    // ==========================================
    private void SyncWeaponDirection()
    {
        // Verifica a variável FacingLeft que já existe no seu PlayerController
        if (playerController.FacingLeft)
        {
            // Vira todo o sistema da arma para a esquerda
            activeWeapon.transform.localRotation = Quaternion.Euler(0, -180, 0);
            weaponCollider.transform.localRotation = Quaternion.Euler(0, -180, 0);
        }
        else
        {
            // Vira todo o sistema da arma para a direita
            activeWeapon.transform.localRotation = Quaternion.Euler(0, 0, 0);
            weaponCollider.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }
}