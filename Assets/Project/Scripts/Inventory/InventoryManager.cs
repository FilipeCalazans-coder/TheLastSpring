using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        [Header("Mochila da Fiorella")]
        public List<ItemData> items = new List<ItemData>(); // Lista de itens carregados

        // Adiciona um item e avisa a UI para atualizar
        public void AddItem(ItemData newItem)
        {
            items.Add(newItem);
            Debug.Log($"<color=green>{newItem.itemName} adicionado ao inventário.</color>");
            
            // Procura a UI e manda atualizar os slots
            Object.FindFirstObjectByType<InventoryUI>()?.UpdateUI();
        }

        // Lógica de Venda/Consumo: Transforma o item em almas no sistema de progressão
        public void SellItem(ItemData itemToSell)
        {
            if (items.Contains(itemToSell))
            {
                // 1. Remove o item da lista
                items.Remove(itemToSell);

                // 2. Acessa o sistema de Level Up para adicionar as almas
                PlayerProgression progression = Object.FindFirstObjectByType<PlayerProgression>();
                if (progression != null)
                {
                    progression.AddSouls(itemToSell.soulValue);
                    progression.SaveProgression(); // Persistência: Garante que o GameData salvou a venda
                }

                Debug.Log($"<color=orange>Item {itemToSell.itemName} vendido/consumido por {itemToSell.soulValue} almas!</color>");

                // 3. Atualiza o visual do inventário imediatamente
                Object.FindFirstObjectByType<InventoryUI>()?.UpdateUI();
            }
        }
    }
}