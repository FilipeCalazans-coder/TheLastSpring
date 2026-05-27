using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Project.Scripts.Inventory
{
    public class InventoryDetailsPanel : MonoBehaviour
    {
        [Header("Referências Visuais")]
        public GameObject panelRoot;     
        public TextMeshProUGUI itemNameText; 
        public TextMeshProUGUI itemLoreText; 
        public TextMeshProUGUI itemValueText; 
        
        [Header("Botão de Ação")]
        public Button actionButton;      
        public TextMeshProUGUI actionButtonText; 

        private ItemData _currentItem;
        private InventoryManager _inventoryManager;
        private ChestInventory _activeChest;

        private void Start()
        {
            _inventoryManager = Object.FindFirstObjectByType<InventoryManager>();
            ClearPanel(); 
        }

        public void UpdatePanel(ItemData item, bool isFromChest = false)
        {
            if (item == null)
            {
                ClearPanel();
                return;
            }

            _currentItem = item;
            panelRoot.SetActive(true);

            // Preenche os textos (Sem a imagem, conforme o novo GDD!)
            itemNameText.text = item.itemName;
            itemLoreText.text = item.description;

            // Mostra o custo em "Pólen" para qualquer item que tenha valor maior que zero
            if (item.soulValue > 0)
            {
                itemValueText.text = $"Valor: {item.soulValue} Pólen";
                itemValueText.gameObject.SetActive(true);
            }
            else
            {
                itemValueText.gameObject.SetActive(false);
            }

            ConfigureActionButton(isFromChest);
        }

        private void ConfigureActionButton(bool isFromChest)
        {
            actionButton.onClick.RemoveAllListeners();
            actionButton.interactable = true; // Garante que esteja ativo

            if (isFromChest)
            {
                // Botão para o Baú
                actionButtonText.text = "RETIRAR";
                actionButton.onClick.AddListener(OnWithdrawClicked);
            }
            else if (_activeChest != null) // NOVO: Se tem um baú aberto e não é item dele
            {
                actionButtonText.text = "DEPOSITAR";
                actionButton.onClick.AddListener(OnDepositClicked);
            }
            else if (InventorySlot.IsAtMerchant && _currentItem.canBeSold)
            {
                // Botão para o Mercador
                actionButtonText.text = "VENDER";
                actionButton.onClick.AddListener(OnSellClicked);
            }
            else if (_currentItem.isConsumable)
            {
                // Botão para a Mochila
                actionButtonText.text = "USAR";
                actionButton.onClick.AddListener(OnUseClicked);
            }
            else
            {
                actionButtonText.text = "EXAMINAR";
                actionButton.interactable = false;
            }
        }

        // Novo método para depositar
        private void OnDepositClicked()
        {
            if (_currentItem == null || _activeChest == null) return;

            // Em vez de chamar o baú direto, use o método do seu Manager
            _inventoryManager.MoveToChest(_currentItem, _activeChest);
            
            // Atualiza a UI do baú
            ChestUI chestUI = Object.FindFirstObjectByType<ChestUI>();
            if (chestUI != null) chestUI.UpdateChestUI();
            
            ClearPanel();
        }

        private void OnWithdrawClicked()
        {
            ChestInventory chest = Object.FindFirstObjectByType<ChestInventory>();
            _inventoryManager.MoveFromChest(_currentItem, chest);
            ClearPanel();
            
            // Importante: Se o baú estiver aberto, precisamos atualizar a UI dele
            ChestUI chestUI = Object.FindFirstObjectByType<ChestUI>();
            if (chestUI != null) chestUI.UpdateChestUI();
        }

        private void OnUseClicked()
        {
            if (_currentItem == null || _inventoryManager == null) return;

            // Roteamento inteligente de consumíveis
            if (_currentItem.isInventoryExpansion)
            {
                _inventoryManager.ExpandInventory(_currentItem);
            }
            else if (_currentItem.isSpeedBoost)
            {
                _inventoryManager.UseSpeedBoost(_currentItem);
            }
            else
            {
                // Lógica de fallback para itens que ainda não têm script próprio (ex: Poção de Cura)
                _inventoryManager.ConsumeGenericItem(_currentItem);
            }

            ClearPanel();
        }

        private void OnSellClicked()
        {
            if (_currentItem == null || _inventoryManager == null) return;

            _inventoryManager.SellItem(_currentItem);
            ClearPanel();
        }

        public void ClearPanel()
        {
            _currentItem = null;
            panelRoot.SetActive(false);
        }

        public void SetActiveChest(ChestInventory chest)
        {
            _activeChest = chest;
        }
    }
}