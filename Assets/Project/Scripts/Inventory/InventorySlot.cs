using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Inventory
{
    public class InventorySlot : MonoBehaviour
    {
        public Image icon; // Referência à imagem do ícone no Prefab
        private ItemData _currentItem; // Item que está ocupando este slot no momento
        private InventoryManager _inventory;

        // Variável de controle para saber se Fiorella está perto de um mercador
        // No futuro, você pode mudar isso via trigger de NPC
        public static bool IsAtMerchant = false; 

        private void Start()
        {
            // Busca o gestor de inventário na cena
            _inventory = Object.FindFirstObjectByType<InventoryManager>();
        }

        // Preenche o slot com dados do item
        public void AddItem(ItemData newItem)
        {
            _currentItem = newItem;
            icon.sprite = newItem.icon;
            icon.enabled = true;
        }

        // Limpa o slot visualmente
        public void ClearSlot()
        {
            _currentItem = null;
            icon.sprite = null;
            icon.enabled = false;
        }

        // Função chamada pelo componente Button (On Click)
        public void OnSlotClicked()
        {
            if (_currentItem == null || _inventory == null) return;

            // REGRA DE OURO:
            // 1. Se for consumível, pode usar a qualquer momento
            if (_currentItem.isConsumable)
            {
                _inventory.SellItem(_currentItem);
            }
            // 2. Se não for consumível, mas for vendável, só funciona perto do mercador
            else if (_currentItem.canBeSold && IsAtMerchant)
            {
                _inventory.SellItem(_currentItem);
                Debug.Log("Item vendido para o mercador.");
            }
            else
            {
                Debug.Log($"O item {_currentItem.itemName} não pode ser usado aqui. Procure um mercador!");
            }
        }
    }
}