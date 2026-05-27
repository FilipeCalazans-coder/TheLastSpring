using UnityEngine;
using UnityEngine.InputSystem;
using Project.Scripts.Inventory; // Necessário para acessar ChestUI e InventoryDetailsPanel

public class BonfireTrigger : MonoBehaviour
{
    [Header("Configurações Visuais")]
    [SerializeField] private GameObject interactPrompt; // A UI "Pressione E para Descansar"
    
    [Header("Referências da UI (Arraste no Inspector)")]
    public ChestUI chestUI; 
    public InventoryDetailsPanel detailsPanel; 

    [Header("Dados do Baú")]
    public ChestInventory linkedChest; // Arraste o componente ChestInventory deste baú aqui

    private bool _isPlayerInRange = false;
    private PlayerControls _playerControls;

    private void Awake()
    {
        _playerControls = new PlayerControls();
        
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
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

            // Limpa o baú quando o jogador se afasta
            if (detailsPanel != null)
            {
                detailsPanel.SetActiveChest(null);
                detailsPanel.ClearPanel();
            }
        }
    }

    private void OnInteractPressed(InputAction.CallbackContext context)
    {
        // Se não foi um aperto de botão válido ou o player está longe, ignora
        if (!context.performed || !_isPlayerInRange) return;

        // 1. O CÓDIGO ORIGINAL QUE FAZIA FUNCIONAR!
        // Isso avisa o jogo que o player sentou na fogueira
        BonfireManager.Instance.RestAtBonfire(this.transform);

        // 2. A nossa nova lógica de conectar o baú
        if (linkedChest != null)
        {
            if (detailsPanel != null) 
            {
                detailsPanel.SetActiveChest(linkedChest); // Habilita o botão "Depositar"
            }
            
            if (chestUI != null)
            {
                chestUI.Open(linkedChest); // Abre a interface visual dos itens do baú
            }
        }
        else
        {
            Debug.LogWarning("Fogueira ativada, mas nenhum 'LinkedChest' foi atribuído no Inspector!");
        }
    }
}