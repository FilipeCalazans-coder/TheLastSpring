using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        [Header("Configurações de Capacidade")]
        public int currentMaxSlots = 12; // Quantos ela pode usar agora
        public int absoluteMaxSlots = 36; // Quantos o grid vai mostrar (com cadeados)

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
                Debug.Log("<color=red>Mochila cheia! Você precisa de mais espaço.</color>");
            }
        }

        public void ExpandInventory(ItemData expansionItem)
        {
            // Aumenta o limite, mas não deixa passar do limite absoluto
            currentMaxSlots += expansionItem.slotsToGain;
            if (currentMaxSlots > absoluteMaxSlots) currentMaxSlots = absoluteMaxSlots;

            items.Remove(expansionItem); 
            Debug.Log($"<color=cyan>Mochila expandida! Novo limite: {currentMaxSlots}/{absoluteMaxSlots} slots.</color>");
            
            // Apenas atualiza a UI (tira os cadeados), não precisa recriar os slots!
            Object.FindFirstObjectByType<InventoryUI>()?.UpdateUI();
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

        // Método que consome um item genérico e atualiza a UI
        public void ConsumeGenericItem(ItemData itemToConsume)
        {
            if (items.Contains(itemToConsume))
            {
                items.Remove(itemToConsume);
                Object.FindFirstObjectByType<InventoryUI>()?.UpdateUI();
                
                Debug.Log($"<color=cyan>Fiorella usou: {itemToConsume.itemName}!</color>");
                // Dica: Aqui será o lugar perfeito para chamar player.Heal() quando criarmos a cura!
            }
        }
    }
}