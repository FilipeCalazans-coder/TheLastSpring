using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Inventory
{
    public class ChestInventory : MonoBehaviour
    {
        public List<ItemData> storedItems = new List<ItemData>();
        public int maxCapacity = 50;

        // Verifica se há espaço no baú
        public bool CanStore() => storedItems.Count < maxCapacity;

        public void AddItem(ItemData item)
        {
            if (CanStore()) storedItems.Add(item);
        }

        public void RemoveItem(ItemData item)
        {
            if (storedItems.Contains(item)) storedItems.Remove(item);
        }
    }
}