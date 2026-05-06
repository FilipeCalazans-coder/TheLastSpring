using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Inventory
{
    public class MerchantController : MonoBehaviour
    {
        [Header("Configurações do Mercador")]
        public List<ItemData> itemsForSale = new List<ItemData>();
        
        [Header("Configurações de UI")]
        public GameObject merchantMainPanel; // Painel "Comprar / Vender"
        public GameObject buyPanel;         // Painel da Lista de Compra
        public Transform buyListParent;      // Onde as linhas serão criadas (Content)
        public GameObject buySlotPrefab;     // O Prefab da linha (Ícone + Preço + Desc)

        private bool _isPlayerNear = false;
        private InventoryUI _playerUI;

        private void Start()
        {
            merchantMainPanel.SetActive(false);
            buyPanel.SetActive(false);
            _playerUI = Object.FindFirstObjectByType<InventoryUI>();
        }

        private void Update()
        {
            // Abre o menu ao apertar E perto do NPC
            if (_isPlayerNear && Input.GetKeyDown(KeyCode.E))
            {
                OpenMerchantMenu();
            }
        }

        // --- Lógica de Compra ---

        public void ProcessPurchase(ItemData item)
        {
            PlayerProgression player = Object.FindFirstObjectByType<PlayerProgression>();
            InventoryManager inv = Object.FindFirstObjectByType<InventoryManager>();

            // 1. Verifica se tem dinheiro e gasta
            if (player != null && player.SpendSouls(item.soulValue))
            {
                // 2. Adiciona o item ao inventário
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

        // Cria a lista visual de itens baseada na lista itemsForSale
        private void PopulateBuyList()
        {
            // Limpa a lista antiga para não duplicar
            foreach (Transform child in buyListParent) Destroy(child.gameObject);

            // Cria uma nova linha para cada item que o mercador vende
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