using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Scripts.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Referências da Interface")]
        public GameObject inventoryPanel; 
        public Transform itemsGrid;      // O objeto com o Grid Layout Group
        public GameObject slotPrefab;    // Arraste o Prefab do seu InventorySlot aqui
        public InventoryManager inventory; 

        private List<InventorySlot> _slots = new List<InventorySlot>();
        public bool isOpen = false; 

        void Start()
        {
            inventoryPanel.SetActive(false);
            RefreshSlotCount(); // Cria os 12 iniciais
        }

        // Cria ou remove slots visuais baseados no currentMaxSlots do Manager
        public void RefreshSlotCount()
        {
            // 1. Limpa os slots antigos da lista e da tela
            foreach (Transform child in itemsGrid) Destroy(child.gameObject);
            _slots.Clear();

            // 2. Cria a quantidade exata de slots que o player tem direito
            for (int i = 0; i < inventory.currentMaxSlots; i++)
            {
                GameObject newSlot = Instantiate(slotPrefab, itemsGrid);
                _slots.Add(newSlot.GetComponent<InventorySlot>());
            }
            
            UpdateUI(); // Preenche com os itens que ela já tinha
        }

        public void OnToggleInventory(InputValue value)
        {
            if (value.isPressed) ForceToggleInventory();
        }

        public void ForceToggleInventory()
        {
            isOpen = !isOpen;
            inventoryPanel.SetActive(isOpen);
            if (isOpen) UpdateUI();
        }

        public void UpdateUI()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (i < inventory.items.Count)
                {
                    _slots[i].AddItem(inventory.items[i]);
                }
                else
                {
                    _slots[i].ClearSlot();
                }
            }
        }
    }
}