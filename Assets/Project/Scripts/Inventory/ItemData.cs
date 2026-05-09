using UnityEngine;

namespace Project.Scripts.Inventory
{
    [CreateAssetMenu(fileName = "NovoItem", menuName = "Fiore/Inventario/Item")]
    public class ItemData : ScriptableObject
    {
        [Header("Detalhes do Item")]
        public string itemName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon; 
        
        [Header("Economia e Comportamento")]
        public int soulValue = 50; 
        public bool isConsumable;  
        public bool canBeSold;     

        [Header("Efeito de Expansão")]
        public bool isInventoryExpansion;
        public int slotsToGain = 6; 

        [Header("Efeito de Velocidade (Novo)")]
        [Tooltip("Se marcado, este item aumentará a velocidade temporariamente.")]
        public bool isSpeedBoost;
        public float speedMultiplier = 1.5f; // Multiplica a velocidade (ex: 1.5 = 50% mais rápido)
        public float buffDuration = 5f;      // Tempo em segundos que o efeito dura
    }
}