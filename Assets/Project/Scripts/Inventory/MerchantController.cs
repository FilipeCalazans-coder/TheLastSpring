using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Inventory
{
    public class MerchantController : MonoBehaviour
    {
        [Header("Configurações do Mercador")]
        public List<ItemData> itemsForSale = new List<ItemData>();
        
        [Header("Configurações de UI")]
        public GameObject merchantMainPanel; 
        public GameObject buyPanel;         
        public Transform buyListParent;      
        public GameObject buySlotPrefab;     

        private bool _isPlayerNear = false;
        private InventoryUI _playerUI;
        
        // NOVA VARIÁVEL DO INPUT SYSTEM
        private PlayerControls _playerControls;

        private void Awake()
        {
            // Inicializamos o sistema de controles
            _playerControls = new PlayerControls();
        }

        private void OnEnable()
        {
            _playerControls.Enable();
        }

        private void OnDisable()
        {
            _playerControls.Disable();
        }

        private void Start()
        {
            merchantMainPanel.SetActive(false);
            buyPanel.SetActive(false);
            _playerUI = Object.FindFirstObjectByType<InventoryUI>();
        }

        private void Update()
        {
            // VERIFICAÇÃO COM O NOVO INPUT SYSTEM
            if (_isPlayerNear && _playerControls.Menu.Interact.triggered)
            {
                if (!merchantMainPanel.activeSelf)
                {
                    OpenMerchantMenu();
                }
            }
        }

        // --- Lógica de Compra ---
        public void ProcessPurchase(ItemData item)
        {
            PlayerProgression player = Object.FindFirstObjectByType<PlayerProgression>();
            InventoryManager inv = Object.FindFirstObjectByType<InventoryManager>();

            if (player != null && player.SpendSouls(item.soulValue))
            {
                inv?.AddItem(item);
                Debug.Log($"<color=green>Compra realizada: {item.itemName}!</color>");
            }
            else
            {
                Debug.Log("<color=red>NPC: Você não tem almas suficientes, Fiorella...</color>");
            }
        }

        // --- Funções de Menu ---
        public void SelectBuyOption()
        {
            merchantMainPanel.SetActive(false);
            buyPanel.SetActive(true);
            PopulateBuyList();
        }

        private void PopulateBuyList()
        {
            foreach (Transform child in buyListParent) Destroy(child.gameObject);

            foreach (ItemData item in itemsForSale)
            {
                GameObject newSlot = Instantiate(buySlotPrefab, buyListParent);
                if (newSlot.TryGetComponent(out MerchantBuySlot slotScript))
                {
                    slotScript.Setup(item, this);
                }
            }
        }

        public void SelectSellOption()
        {
            merchantMainPanel.SetActive(false);
            if (_playerUI != null && !_playerUI.isOpen) _playerUI.ForceToggleInventory();
        }

        public void OpenMerchantMenu() => merchantMainPanel.SetActive(true);

        public void CloseAllMenus()
        {
            merchantMainPanel.SetActive(false);
            buyPanel.SetActive(false);
            if (_playerUI != null && _playerUI.isOpen) _playerUI.ForceToggleInventory();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlayerNear = true;
                InventorySlot.IsAtMerchant = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlayerNear = false;
                InventorySlot.IsAtMerchant = false;
                CloseAllMenus();
            }
        }
    }
}