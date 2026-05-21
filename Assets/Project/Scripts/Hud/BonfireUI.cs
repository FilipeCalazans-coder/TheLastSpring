using UnityEngine;
using UnityEngine.UI;

namespace Project.Scripts.Hud
{
    public class BonfireUI : MonoBehaviour
    {
        [Header("Painéis")]
        public GameObject mainMenuPanel; // O painel com os 3 botões iniciais
        public GameObject levelUpPanel;  // O painel de gastar Pólen (Level Up)
        public GameObject storagePanel;  // O painel do Baú (Armazenamento)

        [Header("Botões do Menu Principal")]
        public Button btnLevelUp;
        public Button btnStorage;
        public Button btnLeave;

        private void Start()
        {
            // Oculta tudo no início
            gameObject.SetActive(false);

            // Adiciona os eventos de clique via código (mais seguro que arrastar no Inspector)
            btnLevelUp.onClick.AddListener(OpenLevelUp);
            btnStorage.onClick.AddListener(OpenStorage);
            btnLeave.onClick.AddListener(CloseBonfire);
        }

        // Chamado pelo BonfireManager
        public void ShowMenu()
        {
            gameObject.SetActive(true);
            mainMenuPanel.SetActive(true);
            levelUpPanel.SetActive(false);
            
            if(storagePanel != null) storagePanel.SetActive(false);
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
            Debug.Log("<color=yellow>Abrindo o Baú de Raízes...</color>");
        }

        // Volta do LevelUp/Baú para o menu principal do Brotinho
        public void BackToMainMenu()
        {
            ShowMenu();
        }

        public void CloseBonfire()
        {
            gameObject.SetActive(false);
            BonfireManager.Instance.CloseMenuAndResumeGame();
        }
    }
}