using UnityEngine;

namespace Project.Scripts.Inventory
{
    public class ItemPickup : MonoBehaviour
    {
        [Header("Qual item é este?")]
        public ItemData itemToGive; // Arraste o seu ScriptableObject (ex: PolenCurativo) aqui

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Verifica se quem encostou foi a Fiorella
            if (other.CompareTag("Player"))
            {
                // Procura o gestor de inventário na cena (nova API da Unity)
                InventoryManager inventory = Object.FindFirstObjectByType<InventoryManager>();
                
                if (inventory != null && itemToGive != null)
                {
                    inventory.AddItem(itemToGive);
                    
                    // Força a UI a se desenhar novamente com o novo item
                    InventoryUI ui = Object.FindFirstObjectByType<InventoryUI>();
                    if (ui != null) ui.UpdateUI();
                    
                    // Destrói o objeto do chão
                    Destroy(gameObject);
                }
            }
        }
    }
}