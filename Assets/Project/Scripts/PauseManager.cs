using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Necessário para o novo sistema

namespace Project.Scripts
{
    public class PauseManager : MonoBehaviour
    {
        [Header("Configuração de UI")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        
        private bool _isPaused = false;

        // O Novo Input System chamará esta função via mensagem
        // Certifique-se que o nome da Action no editor seja "Pause"
        public void OnPause(InputValue value)
        {
            // value.isPressed confirma que o botão foi clicado
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
            Time.timeScale = 0f; // Congela o mundo
            _isPaused = true;
            Debug.Log("Sinal de Pausa recebido via Action Map!");
        }

        public void Resume()
        {
            pauseMenuPanel.SetActive(false);
            Time.timeScale = 1f; // O tempo volta a correr
            _isPaused = false;
        }

        public void BackToMenu()
        {
            Time.timeScale = 1f;
            
            // Procura o script de progressão e manda ele salvar os valores atuais no GameData
            var progression = Object.FindFirstObjectByType<PlayerProgression>();
            if (progression != null)
            {
                progression.SaveProgression(); 
            }

            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}