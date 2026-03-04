using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int startingHealth = 3;

    private int currentHealth;
    private KnockBack knockBack;
    private Flash flash;



    private void Awake()
    {
        flash = GetComponent<Flash>();
        knockBack = GetComponent<KnockBack>();
    }

    private void Start()
    {
        currentHealth = startingHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        knockBack.GetKnockedBack(PlayerController.Instance.transform, 15f);
        StartCoroutine(flash.FlashRoutine());
    }

    public void DetectDeath()
    {
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
