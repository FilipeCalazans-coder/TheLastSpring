using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        [Header("Configurações de Capacidade")]
        public int currentMaxSlots = 12; 

        [Header("Mochila da Fiorella")]
        public List<ItemData> items = new List<ItemData>();

        // Adiciona um item se houver espaço
        public void AddItem(ItemData newItem)
        {
            if (items.Count < currentMaxSlots)
            {
                items.Add(newItem);
                Object.FindFirstObjectByType<InventoryUI>()?.UpdateUI();
            }
            else
            {
                Debug.Log("<color=red>Mochila cheia! Não há espaço.</color>");
            }
        }

        // Expande os slots do inventário
        public void ExpandInventory(ItemData expansionItem)
        {
            currentMaxSlots += expansionItem.slotsToGain;
            items.Remove(expansionItem); 
            Object.FindFirstObjectByType<InventoryUI>()?.RefreshSlotCount();
        }

        // Vende ou consome almas
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

        // --- NOVA CHAMADA DE VELOCIDADE ---
        public void UseSpeedBoost(ItemData boostItem)
        {
            if (items.Contains(boostItem))
            {
                items.Remove(boostItem); // Remove a poção da mochila
                Object.FindFirstObjectByType<InventoryUI>()?.UpdateUI(); // Atualiza a tela

                // Encontra a Fiorella e manda ela aplicar o Buff nela mesma!
                PlayerController player = Object.FindFirstObjectByType<PlayerController>(); 
                if (player != null)
                {
                    player.ApplySpeedBuff(boostItem.speedMultiplier, boostItem.buffDuration);
                    Debug.Log($"<color=cyan>Usou {boostItem.itemName}! Velocidade aumentada.</color>");
                }
            }
        }
    }
}