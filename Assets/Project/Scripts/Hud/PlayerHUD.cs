using UnityEngine;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("Referências de Texto")]
    [SerializeField] private TextMeshProUGUI soulsText;
    // Removido: levelText daqui

    private PlayerProgression _progression;

    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            _progression = PlayerController.Instance.GetComponent<PlayerProgression>();
        }
    }

    private void Update()
    {
        if (_progression == null) return;
        soulsText.text = "Almas: " + _progression.GetCurrentSouls();
    }
}