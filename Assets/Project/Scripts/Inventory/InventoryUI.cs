using UnityEngine;
using UnityEngine.InputSystem; // Necessário para o Novo Input System

namespace Project.Scripts.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Referências da Interface")]
        public GameObject inventoryPanel; // O painel principal que contém a grelha
        public Transform itemsParent;     // O objeto "ItemsGrid"
        public InventoryManager inventory; // Referência ao script do inventário

        private InventorySlot[] slots;
        
        // Transformamos em pública para que o Mercador saiba se a mochila está aberta
        public bool isOpen = false; 

        void Start()
        {
            // Coleta todas as 12 ranhuras que estão dentro do Grid
            slots = itemsParent.GetComponentsInChildren<InventorySlot>();
            
            // Garante que o jogo comece com o inventário fechado
            inventoryPanel.SetActive(false);
        }

        // Função original chamada automaticamente pelo Player Input (Tecla 'I')
        public void OnToggleInventory(InputValue value)
        {
            if (value.isPressed)
            {
                ForceToggleInventory();
            }
        }

        // Nova função separada para que outros scripts (como o Mercador) possam abrir a mochila
        public void ForceToggleInventory()
        {
            isOpen = !isOpen; // Inverte o estado (se estava aberto, fecha. Se estava fechado, abre)
            inventoryPanel.SetActive(isOpen);

            if (isOpen)
            {
                UpdateUI();
                Debug.Log("<color=yellow>Inventário Aberto!</color>");
            }
            else
            {
                Debug.Log("<color=yellow>Inventário Fechado!</color>");
            }
        }

        // Atualiza os ícones na tela
        public void UpdateUI()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                // Se existe um item para este índice na lista, desenha-o
                if (i < inventory.items.Count)
                {
                    slots[i].AddItem(inventory.items[i]);
                }
                // Caso contrário, esvazia a ranhura visual
                else
                {
                    slots[i].ClearSlot();
                }
            }
        }
    }
}