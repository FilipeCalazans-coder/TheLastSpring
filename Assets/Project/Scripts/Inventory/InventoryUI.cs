using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Scripts.Inventory
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("Referências da Interface")]
        public GameObject inventoryPanel; 
        public GameObject backgroundOverlay; 
        
        public Transform itemsGrid; 
        public GameObject slotPrefab;     
        
        [Tooltip("Crie um Prefab vazio (apenas um RectTransform com o tamanho do slot) e arraste para aqui.")]
        public GameObject dummySlotPrefab; // A SOLUÇÃO DEFINITIVA
        
        public InventoryManager inventory; 

        [Header("Truque de Layout do Artista")]
        public bool useDummySlot = true; 

        private List<InventorySlot> _slots = new List<InventorySlot>();
        public bool isOpen = false; 

        private PlayerControls _playerControls;

        private void Awake()
        {
            _playerControls = new PlayerControls();
        }

        private void OnEnable() => _playerControls.Enable();
        private void OnDisable() => _playerControls.Disable();

        void Start()
        {
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            if (backgroundOverlay != null) backgroundOverlay.SetActive(false);
            
            GenerateAllSlots(); 
        }

        private void Update()
        {
            if (_playerControls.Menu.ToggleInventory.triggered)
            {
                ForceToggleInventory();
            }
        }

        private void GenerateAllSlots()
        {
            if (itemsGrid == null || slotPrefab == null || inventory == null) return;

            // Limpa tudo o que estiver na grelha
            foreach (Transform child in itemsGrid) Destroy(child.gameObject);
            _slots.Clear();

            // 1. Instancia o DUMMY PREFAB (que tem o tamanho correto para o Grid Layout ler)
            if (useDummySlot && dummySlotPrefab != null)
            {
                Instantiate(dummySlotPrefab, itemsGrid, false);
            }
            else if (useDummySlot && dummySlotPrefab == null)
            {
                Debug.LogWarning("O useDummySlot está ativado, mas faltou colocar o Dummy Slot Prefab no Inspector!");
            }

            // 2. Gera os slots reais (ex: 36)
            int maxAbsolute = inventory.absoluteMaxSlots <= 0 ? 36 : inventory.absoluteMaxSlots;
            
            for (int i = 0; i < maxAbsolute; i++)
            {
                GameObject newSlot = Instantiate(slotPrefab, itemsGrid, false);
                InventorySlot slotScript = newSlot.GetComponent<InventorySlot>();
                if (slotScript != null)
                {
                    _slots.Add(slotScript);
                }
            }
            
            UpdateUI();
        }

        public void ForceToggleInventory()
        {
            if (inventoryPanel == null) return;

            isOpen = !isOpen;
            inventoryPanel.SetActive(isOpen);
            if (backgroundOverlay != null) backgroundOverlay.SetActive(isOpen);

            if (isOpen) UpdateUI();
        }

        public void UpdateUI()
        {
            if (inventory == null || _slots.Count == 0) return;

            for (int i = 0; i < _slots.Count; i++)
            {
                bool isLocked = i >= inventory.currentMaxSlots;
                ItemData item = (!isLocked && i < inventory.items.Count) ? inventory.items[i] : null;
                _slots[i].SetupSlot(isLocked, item);
            }
        }
    }
}