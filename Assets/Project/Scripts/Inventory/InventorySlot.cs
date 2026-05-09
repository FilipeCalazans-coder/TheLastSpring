using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Inventory
{
    public class InventorySlot : MonoBehaviour
    {
        public Image icon;
        private ItemData _currentItem;
        private InventoryManager _inventory;

        public static bool IsAtMerchant = false; 

        private void Start()
        {
            _inventory = Object.FindFirstObjectByType<InventoryManager>();
        }

        public void AddItem(ItemData newItem)
        {
            _currentItem = newItem;
            icon.sprite = newItem.icon;
            icon.enabled = true;
        }

        public void ClearSlot()
        {
            _currentItem = null;
            icon.sprite = null;
            icon.enabled = false;
        }

        public void OnSlotClicked()
        {
            if (_currentItem == null || _inventory == null) return;

            // 1. Lógica se o item for Consumível (Pode usar a qualquer momento)
            if (_currentItem.isConsumable)
            {
                if (_currentItem.isInventoryExpansion)
                {
                    _inventory.ExpandInventory(_currentItem);
                }
                else if (_currentItem.isSpeedBoost) // NOVA REGRA DE VELOCIDADE
                {
                    _inventory.UseSpeedBoost(_currentItem);
                }
                else
                {
                    _inventory.SellItem(_currentItem); // Ex: Grão de Pólen
                }
            }
            // 2. Lógica se o item for apenas Vendável (Requer Mercador)
            else if (_currentItem.canBeSold && IsAtMerchant)
            {
                _inventory.SellItem(_currentItem);
            }
        }
    }
}