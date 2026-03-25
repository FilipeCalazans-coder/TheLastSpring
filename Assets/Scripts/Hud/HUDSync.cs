using UnityEngine;
using UnityEngine.UI;

public class HUDSync : MonoBehaviour
{
    [SerializeField] private Slider healthBarSlider; // Arraste a Health_Bar aqui no Inspector do Canvas

    [SerializeField] private Slider staminaBarSlider; // Arraste a Stamina_Bar aqui no Inspector do Canvas

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            GameObject player = PlayerController.Instance.gameObject;

            // Sincroniza Vida
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            if (health != null && healthBarSlider != null)
            {
                health.SetHealthBar(healthBarSlider);
            }

            // Sincroniza Estamina
            PlayerStamina stamina = player.GetComponent<PlayerStamina>();
            if (stamina != null && staminaBarSlider != null)
            {
                stamina.SetStaminaBar(staminaBarSlider);
            }
        }
    }
}