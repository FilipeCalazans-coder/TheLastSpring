using UnityEngine;
using UnityEngine.UI;
using Project.Scripts.Inventory;

namespace Project.Scripts.Hud
{
    public class BonfireUI : MonoBehaviour
    {
        [Header("Painéis")]
        public GameObject mainMenuPanel; // O painel com os botões iniciais
        public GameObject levelUpPanel;  // O painel de gastar Pólen (Level Up)
        public GameObject storagePanel;  // O painel do Baú (Armazenamento)
        public GameObject skillTreePanel; // NOVO: O painel da Árvore de Habilidades
        public ChestUI chestUI;

        [Header("Botões do Menu Principal")]
        public Button btnLevelUp;
        public Button btnStorage;
        public Button btnSkillTree; // NOVO: Botão para abrir a Árvore
        public Button btnLeave;

        [Header("Controle de Inventário")]
        public InventoryUI inventoryUI;

        private void Start()
        {
            // Oculta tudo no início
            gameObject.SetActive(false);

            // Adiciona os eventos de clique via código
            if (btnLevelUp != null) btnLevelUp.onClick.AddListener(OpenLevelUp);
            if (btnStorage != null) btnStorage.onClick.AddListener(OpenStorage);
            if (btnSkillTree != null) btnSkillTree.onClick.AddListener(OpenSkillTree); // Conecta o novo botão
            if (btnLeave != null) btnLeave.onClick.AddListener(CloseBonfire);
        }

        // Chamado pelo BonfireManager
        public void ShowMenu()
        {
            gameObject.SetActive(true);
            mainMenuPanel.SetActive(true);
            
            if(levelUpPanel != null) levelUpPanel.SetActive(false);
            if(storagePanel != null) storagePanel.SetActive(false);
            if(skillTreePanel != null) skillTreePanel.SetActive(false); // Garante que a árvore fecha
        }

        public void OpenLevelUp()
        {
            mainMenuPanel.SetActive(false);
            if (levelUpPanel != null) levelUpPanel.SetActive(true);
        }

        public void OpenStorage()
        {
            mainMenuPanel.SetActive(false);
            if (storagePanel != null) storagePanel.SetActive(true);
            
            if (inventoryUI != null && !inventoryUI.isOpen)
            {
                inventoryUI.ForceToggleInventory();
            }

            // Atualiza a visualização do baú sempre que abrir
            if (chestUI != null)
            {
                chestUI.UpdateChestUI();
            }
            
            Debug.Log("<color=yellow>Abrindo o Baú de Raízes...</color>");
        }

        // NOVO: Método para abrir a Árvore de Habilidades
        public void OpenSkillTree()
        {
            mainMenuPanel.SetActive(false);
            if (skillTreePanel != null) skillTreePanel.SetActive(true);
            
            Debug.Log("<color=magenta>Acessando a Árvore de Habilidades...</color>");
        }

        // Volta do LevelUp/Baú/Árvore para o menu principal do Brotinho
        public void BackToMainMenu()
        {
            ShowMenu();
        }

        public void CloseBonfire()
        {
            // Fecha o inventário se ele estiver aberto quando fecharmos o menu da fogueira
            if (inventoryUI != null && inventoryUI.isOpen)
            {
                inventoryUI.ForceToggleInventory();
            }
            
            gameObject.SetActive(false);
            BonfireManager.Instance.CloseMenuAndResumeGame();
        }
    }
}