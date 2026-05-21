using UnityEngine;
using UnityEngine.InputSystem;

public class BonfireTrigger : MonoBehaviour
{
    [Header("Configurações Visuais")]
    [SerializeField] private GameObject interactPrompt; // A UI "Pressione E para Descansar"
    
    private bool _isPlayerInRange = false;
    private PlayerControls _playerControls; // O seu Input System gerado

    private void Awake()
    {
        _playerControls = new PlayerControls();
        
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    private void OnEnable()
    {
        // Inscreve a função OnInteractPressed no evento de apertar o botão de Interagir (E)
        _playerControls.Menu.Interact.performed += OnInteractPressed;
        _playerControls.Enable();
    }

    private void OnDisable()
    {
        _playerControls.Menu.Interact.performed -= OnInteractPressed;
        _playerControls.Disable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
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
        // Se o player não estiver perto, ignora
        if (!_isPlayerInRange) return;

        // Se estiver perto, chama o Gerenciador Central da Fogueira
        BonfireManager.Instance.RestAtBonfire(this.transform);
    }
}