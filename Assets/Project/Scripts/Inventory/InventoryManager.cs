using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        [Header("Configurações de Capacidade")]
        public int currentMaxSlots = 12;
        public int absoluteMaxSlots = 36;

        [Header("Mochila da Fiorella")]
        public List<ItemData> items = new List<ItemData>();

        public void AddItem(ItemData newItem)
        {
            if (items.Count < currentMaxSlots)
            {
                items.Add(newItem);
                Object.FindFirstObjectByType<InventoryUI>()?.UpdateUI();
            }
            else
            {
                Debug.Log("<color=red>Mochila cheia!</color>");
            }
        }

        public void ExpandInventory(ItemData expansionItem)
        {
            currentMaxSlots += expansionItem.slotsToGain;
            if (currentMaxSlots > absoluteMaxSlots) currentMaxSlots = absoluteMaxSlots;

            items.Remove(expansionItem); 
            Object.FindFirstObjectByType<InventoryUI>()?.UpdateUI();
        }

        public void SellItem(ItemData itemToSell)
        {
            if (items.Contains(itemToSell))
            {
                items.Remove(itemToSell);
                PlayerProgression progression = Object.FindFirstObjectByType<PlayerProgression>();
                if (progression != null)
                {
                    progression.AddSouls(itemToSell.soulValue);
                    progression.SaveProgression();
                }
                Object.FindFirstObjectByType<InventoryUI>()?.UpdateUI();
            }
        }

        public void UseItem(ItemData itemToUse)
        {
            if (!items.Contains(itemToUse)) return;

            if (itemToUse.isPollenItem)
            {
                PlayerProgression progression = Object.FindFirstObjectByType<PlayerProgression>();
                if (progression != null)
                {
                    progression.AddSouls(itemToUse.soulValue);
                    progression.SaveProgression();
                }
            }
            else if (itemToUse.isSpeedBoost)
            {
                PlayerController player = Object.FindFirstObjectByType<PlayerController>();
                player?.ApplySpeedBuff(itemToUse.speedMultiplier, itemToUse.buffDuration);
            }

            items.Remove(itemToUse);
            Object.FindFirstObjectByType<InventoryUI>()?.UpdateUI();
        }

        // --- MÉTODOS DE TRANSFERÊNCIA PARA O BAÚ ---
        public void MoveToChest(ItemData item, ChestInventory chest)
        {
            if (items.Contains(item) && chest.CanStore())
            {
                chest.AddItem(item);
                items.Remove(item);
                
                // Correção: chamando o método de atualização na UI
                Object.FindFirstObjectByType<InventoryUI>()?.UpdateUI();
            }
        }

        public void MoveFromChest(ItemData item, ChestInventory chest)
        {
            if (items.Count < currentMaxSlots)
            {
                items.Add(item);
                chest.RemoveItem(item);
                
                // Correção: chamando o método de atualização na UI
                Object.FindFirstObjectByType<InventoryUI>()?.UpdateUI();
            }
        }

        // --- ATALHOS PARA COMPATIBILIDADE ---
        public void UseSpeedBoost(ItemData item) => UseItem(item);
        public void ConsumeGenericItem(ItemData item) => UseItem(item);
    }
}