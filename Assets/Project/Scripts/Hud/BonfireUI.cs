using UnityEngine;
using UnityEngine.UI;
using Project.Scripts.Inventory;

namespace Project.Scripts.Hud
{
    public class BonfireUI : MonoBehaviour
    {
        [Header("Painéis")]
        public GameObject mainMenuPanel;
        public GameObject levelUpPanel;
        public GameObject storagePanel;
        public GameObject skillTreePanel;
        public ChestUI chestUI;

        [Header("Botões do Menu Principal")]
        public Button btnLevelUp;
        public Button btnStorage;
        public Button btnSkillTree; 
        public Button btnLeave;

        [Header("Controle de Inventário")]
        public InventoryUI inventoryUI;

        private void Start()
        {
            gameObject.SetActive(false);
            if (btnLevelUp != null) btnLevelUp.onClick.AddListener(OpenLevelUp);
            if (btnStorage != null) btnStorage.onClick.AddListener(OpenStorage);
            if (btnSkillTree != null) btnSkillTree.onClick.AddListener(OpenSkillTree); 
            if (btnLeave != null) btnLeave.onClick.AddListener(CloseBonfire);
        }

        public void ShowMenu()
        {
            gameObject.SetActive(true);
            mainMenuPanel.SetActive(true);
            
            if(levelUpPanel != null) levelUpPanel.SetActive(false);
            if(storagePanel != null) storagePanel.SetActive(false);
            if(skillTreePanel != null) skillTreePanel.SetActive(false); 

            if (inventoryUI != null && inventoryUI.isOpen)
                inventoryUI.ForceToggleInventory();
        }

        public void OpenLevelUp() { mainMenuPanel.SetActive(false); if (levelUpPanel != null) levelUpPanel.SetActive(true); }
        public void OpenSkillTree() { mainMenuPanel.SetActive(false); if (skillTreePanel != null) skillTreePanel.SetActive(true); }

        public void OpenStorage()
        {
            mainMenuPanel.SetActive(false);
            if (storagePanel != null) storagePanel.SetActive(true);
            
            if (inventoryUI != null && !inventoryUI.isOpen) inventoryUI.ForceToggleInventory();
            if (chestUI != null) chestUI.UpdateChestUI();
        }

        public void BackToMainMenu() { ShowMenu(); }

        public void CloseBonfire()
        {
            if (inventoryUI != null && inventoryUI.isOpen) inventoryUI.ForceToggleInventory();
            gameObject.SetActive(false);
            BonfireManager.Instance.CloseMenuAndResumeGame();
        }
    }
}