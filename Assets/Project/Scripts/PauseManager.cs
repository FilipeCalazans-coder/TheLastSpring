using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI; // Necessário para interagir com componentes de UI (Slider, Toggle)
using TMPro; // Necessário para interagir com o Dropdown do TextMeshPro

namespace Project.Scripts
{
    public class PauseManager : MonoBehaviour
    {
        [Header("Configuração de UI")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private GameObject settingsPanel; 
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        
        [Header("Componentes das Configurações (UI)")]
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private TMP_Dropdown graphicsDropdown;
        [SerializeField] private Toggle fullscreenToggle;

        private bool _isPaused = false;

        private void Start()
        {
            // Assim que o jogo inicia, carregamos as opções guardadas pelo utilizador
            LoadSettings();
        }

        public void OnPause(InputValue value)
        {
            if (value.isPressed)
            {
                if (_isPaused) 
                    Resume();
                else 
                    Pause();
            }
        }

        public void Pause()
        {
            pauseMenuPanel.SetActive(true);
            if (settingsPanel != null) settingsPanel.SetActive(false); 
            Time.timeScale = 0f; 
            _isPaused = true;
            Debug.Log("Sinal de Pausa recebido via Action Map!");
        }

        public void Resume()
        {
            pauseMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false); 
            Time.timeScale = 1f; 
            _isPaused = false;
        }

        public void OpenSettings()
        {
            pauseMenuPanel.SetActive(false); 
            if (settingsPanel != null) settingsPanel.SetActive(true); 

            // CRUCIAL: Antes de mostrar o menu, atualizamos os Sliders/Dropdowns 
            // com os valores reais atuais do jogo!
            UpdateSettingsUI();
        }

        public void CloseSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false); 
            pauseMenuPanel.SetActive(true); 
        }

        // ==========================================================
        // --- NOVAS FUNÇÕES DE CONFIGURAÇÃO (VOLUME & GRÁFICOS) ---
        // ==========================================================

        /// <summary>
        /// Altera o volume geral do jogo. Deve ser ligada ao OnValueChanged do Slider.
        /// </summary>
        public void SetVolume(float volume)
        {
            AudioListener.volume = volume; 
            PlayerPrefs.SetFloat("MasterVolume", volume); 
            PlayerPrefs.Save(); // CORREÇÃO: Força a gravação imediata na memória!
        }

        public void SetGraphicsQuality(int qualityIndex)
        {
            QualitySettings.SetQualityLevel(qualityIndex); 
            PlayerPrefs.SetInt("GraphicsQuality", qualityIndex); 
            PlayerPrefs.Save(); // CORREÇÃO
        }

        public void SetFullscreen(bool isFullscreen)
        {
            Screen.fullScreen = isFullscreen; 
            PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0); 
            PlayerPrefs.Save(); // CORREÇÃO
        }

        private void LoadSettings()
        {
            float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            AudioListener.volume = savedVolume;

            int savedQuality = PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
            QualitySettings.SetQualityLevel(savedQuality);

            bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
            Screen.fullScreen = savedFullscreen;
        }

        private void UpdateSettingsUI()
        {
            // CORREÇÃO CRUCIAL: Agora puxamos diretamente do "PlayerPrefs" (a memória guardada) 
            // para garantir 100% de precisão visual quando o painel é reaberto.
            if (volumeSlider != null) 
                volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
                
            if (graphicsDropdown != null) 
                graphicsDropdown.value = PlayerPrefs.GetInt("GraphicsQuality", QualitySettings.GetQualityLevel());
                
            if (fullscreenToggle != null) 
                fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;
        }

        // ==========================================================

        public void BackToMenu()
        {
            Time.timeScale = 1f;
            
            var progression = Object.FindFirstObjectByType<PlayerProgression>();
            if (progression != null)
            {
                progression.SaveProgression(); 
            }

            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}