using UnityEngine;

namespace Project.Scripts.Inventory
{
    // Permite criar novos itens clicando com o botão direito no projeto
    [CreateAssetMenu(fileName = "NovoItem", menuName = "Fiore/Inventario/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Detalhes do Item")]
        public string itemName; // Nome visível do item
        
        [TextArea(3, 5)]
        public string description; // Descrição que aparecerá na Tooltip
        
        public Sprite icon; // Ícone para os slots da UI
        
        [Header("Economia e Comportamento")]
        public int soulValue = 50; // Valor em almas/pólen
        
        [Tooltip("Se marcado, o player pode usar o item direto do inventário para ganhar almas.")]
        public bool isConsumable; 

        [Tooltip("Se marcado, o item pode ser vendido para um NPC mercador.")]
        public bool canBeSold; 
    }
}