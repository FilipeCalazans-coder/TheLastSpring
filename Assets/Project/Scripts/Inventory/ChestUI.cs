using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Project.Scripts.Inventory;

namespace Project.Scripts.Inventory
{
    public class ChestUI : MonoBehaviour
    {
        public Transform itemsGrid; 
        public GameObject slotPrefab;
        public ChestInventory chestInventory;

        private List<InventorySlot> _slots = new List<InventorySlot>();

        public void UpdateChestUI()
        {
            if (chestInventory == null || itemsGrid == null) return;

            // Limpa tudo e prepara o grid
            foreach (Transform child in itemsGrid) Destroy(child.gameObject);
            _slots.Clear();

            // Loop fixo baseado na capacidade máxima
            for (int i = 0; i < chestInventory.maxCapacity; i++)
            {
                GameObject newSlot = Instantiate(slotPrefab, itemsGrid);
                InventorySlot slotScript = newSlot.GetComponent<InventorySlot>();
                
                if (slotScript != null)
                {
                    // Verifica se existe um item nessa posição da lista
                    ItemData item = (i < chestInventory.storedItems.Count) ? chestInventory.storedItems[i] : null;
                    
                    if (item != null)
                    {
                        slotScript.SetupSlot(false, item); 

                        // Adiciona UMA única vez o listener para abrir os detalhes
                        Button btn = newSlot.GetComponent<Button>();
                        if (btn != null)
                        {
                            btn.onClick.AddListener(() => {
                                InventoryDetailsPanel panel = Object.FindFirstObjectByType<InventoryDetailsPanel>();
                                if (panel != null)
                                {
                                    panel.UpdatePanel(item, true); // true = é do baú
                                }
                            });
                        }
                    }
                    else
                    {
                        // Slot vazio
                        slotScript.SetupSlot(false, null);
                    }
                    
                    _slots.Add(slotScript);
                }
            }
        }
    }
}