using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Inventory
{
    public class InventorySlot : MonoBehaviour
    {
        [Header("Referências Visuais")]
        public Image icon; // Ícone do item
        public Image lockIcon; // NOVO: Ícone do cadeado

        private ItemData _currentItem;
        private InventoryManager _inventory;
        private bool _isLocked = false; // Estado do slot

        public static bool IsAtMerchant = false; 

        private void Start()
        {
            _inventory = Object.FindFirstObjectByType<InventoryManager>();
        }

        // NOVO MÉTOD: Configura o estado visual completo do slot
        public void SetupSlot(bool locked, ItemData item)
        {
            _isLocked = locked;
            
            // Ativa o cadeado se estiver bloqueado, desativa se estiver livre
            if (lockIcon != null) lockIcon.gameObject.SetActive(locked);

            if (locked)
            {
                // Se está trancado, esconde qualquer item
                _currentItem = null;
                icon.enabled = false;
            }
            else
            {
                // Se está livre, verifica se tem item para mostrar
                if (item != null)
                {
                    _currentItem = item;
                    icon.sprite = item.icon;
                    icon.enabled = true;
                }
                else
                {
                    ClearSlot();
                }
            }
        }

        public void ClearSlot()
        {
            _currentItem = null;
            icon.sprite = null;
            icon.enabled = false;
        }

        public void OnSlotClicked()
        {
            // Se o slot estiver trancado ou vazio, ignora o clique
            if (_isLocked || _currentItem == null) return;

            // Encontra o Painel de Detalhes na cena e manda os dados do item
            InventoryDetailsPanel detailsPanel = Object.FindFirstObjectByType<InventoryDetailsPanel>();
            
            if (detailsPanel != null)
            {
                detailsPanel.UpdatePanel(_currentItem);
            }
            else
            {
                Debug.LogWarning("Painel de Detalhes não encontrado na cena!");
            }
        }
    }
}