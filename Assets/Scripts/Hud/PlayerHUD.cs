using UnityEngine;
using UnityEngine.UI; 
using TMPro;           

public class PlayerHUD : MonoBehaviour
{
    [Header("Referências de Texto")]
    [SerializeField] private TextMeshProUGUI soulsText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI pointsText;

    private PlayerProgression _progression;

    private void Start()
    {
        // Busca a progressão no player usando a nossa Instance
        if (PlayerController.Instance != null)
        {
            _progression = PlayerController.Instance.GetComponent<PlayerProgression>();
        }
    }

    private void Update()
    {
        if (_progression == null) return;

        // Atualiza os textos
        soulsText.text = "Almas: " + _progression.GetCurrentSouls();
        levelText.text = "Nível: " + _progression.GetCurrentLevel();

        // Lógica para o texto de pontos
        int points = _progression.GetAvailablePoints();
        if (points > 0)
        {
            pointsText.gameObject.SetActive(true);
            pointsText.text = "Pontos Disponíveis: " + points;
            pointsText.color = Color.yellow; // Dá um destaque
        }
        else
        {
            pointsText.gameObject.SetActive(false); // Esconde se não tiver pontos
        }
    }
}