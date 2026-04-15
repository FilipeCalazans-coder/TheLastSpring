using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    private EnemyHealth _enemyHealth;

    private void Awake()
    {
        _enemyHealth = GetComponentInParent<EnemyHealth>();
    }

    private void Start()
    {
        // Oculta a barra no início (só aparece quando apanha)
        slider.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Garante que a barra não gire se o inimigo girar (flip)
        transform.rotation = Quaternion.identity;
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        slider.gameObject.SetActive(true);
        slider.maxValue = maxHealth;
        slider.value = currentHealth;
    }
}