using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Project.Scripts.Inventory;

namespace Project.Scripts.Inventory
{
    public class ChestUI : MonoBehaviour
    {
        [Header("Referências")]
        public GameObject chestPanel; // ARRASTE O PAINEL PRINCIPAL DO BAÚ AQUI
        public Transform itemsGrid; 
        public GameObject slotPrefab;
        
        // Esta variável será definida pelo BonfireTrigger quando abrirmos o baú
        public ChestInventory chestInventory;

        private List<InventorySlot> _slots = new List<InventorySlot>();

        // Método que o BonfireTrigger chamará
        public void Open(ChestInventory chest)
        {
            chestInventory = chest;
            if (chestPanel != null) chestPanel.SetActive(true);
            UpdateChestUI();
        }

        public void Close()
        {
            if (chestPanel != null) chestPanel.SetActive(false);
            chestInventory = null;
        }

        public void UpdateChestUI()
        {
            if (chestInventory == null || itemsGrid == null) return;

            foreach (Transform child in itemsGrid) Destroy(child.gameObject);
            _slots.Clear();

            for (int i = 0; i < chestInventory.maxCapacity; i++)
            {
                GameObject newSlot = Instantiate(slotPrefab, itemsGrid);
                InventorySlot slotScript = newSlot.GetComponent<InventorySlot>();
                
                if (slotScript != null)
                {
                    ItemData item = (i < chestInventory.storedItems.Count) ? chestInventory.storedItems[i] : null;
                    
                    slotScript.SetupSlot(false, item);

                    if (item != null)
                    {
                        Button btn = newSlot.GetComponent<Button>();
                        if (btn != null)
                        {
                            btn.onClick.AddListener(() => {
                                InventoryDetailsPanel panel = Object.FindFirstObjectByType<InventoryDetailsPanel>();
                                if (panel != null)
                                {
                                    panel.UpdatePanel(item, true); // true = é item do baú
                                }
                            });
                        }
                    }
                    _slots.Add(slotScript);
                }
            }
        }
    }
}