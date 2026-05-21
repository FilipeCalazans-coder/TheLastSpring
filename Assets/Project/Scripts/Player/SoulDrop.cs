using UnityEngine;
using UnityEngine.InputSystem;

public class SoulDrop : MonoBehaviour
{
    [SerializeField] private GameObject interactPrompt; // A UI "Pressione E para recuperar"
    private int _storedPolen;
    private bool _isPlayerInRange = false;
    private PlayerControls _playerControls;

    private void Awake()
    {
        _playerControls = new PlayerControls();
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void OnEnable()
    {
        _playerControls.Menu.Interact.performed += OnInteractPressed;
        _playerControls.Enable();
    }

    private void OnDisable()
    {
        _playerControls.Menu.Interact.performed -= OnInteractPressed;
        _playerControls.Disable();
    }

    // Este método é chamado pelo PlayerProgression.cs ao morrer
    public void Initialize(int amount)
    {
        _storedPolen = amount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Colisão detectada com: {other.name} (Tag: {other.tag})"); // ADICIONE ISSO
        if (other.CompareTag("Player"))
        {
            Debug.Log("Fiorella entrou na área da Alma."); // ADICIONE ISSO
            _isPlayerInRange = true;
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerInRange = false;
            if (interactPrompt != null) interactPrompt.SetActive(false);
        }
    }

    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        if (!_isPlayerInRange) return;

        // Recupera o pólen e destrói o objeto
        if (PlayerController.Instance != null)
        {
            PlayerProgression prog = PlayerController.Instance.GetComponent<PlayerProgression>();
            if (prog != null)
            {
                prog.AddSouls(_storedPolen);
                Debug.Log($"<color=green>Você recuperou {_storedPolen} de Pólen!</color>");
                Destroy(gameObject); // Remove a alma do chão
            }
        }
    }
}