using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Configuração de Cenas")]
    [SerializeField] private string gameSceneName = "Test"; // Nome da sua cena principal

    [Header("Referências de UI")]
    [SerializeField] private Button continueButton;
    
    // VARIÁVEIS MOVIDAS PARA DENTRO DA CLASSE
    public GameObject mainMenuPanel;
    public GameObject cutsceneManager; // Arraste aquele objeto vazio aqui
    public GameObject cutscenePanel;   // Arraste o painel preto aqui

    private void Start()
    {
        // Regra de Ouro: O botão Continuar só aparece/funciona se o GameData diz que tem save
        if (continueButton != null)
        {
            continueButton.interactable = GameData.HasSaveData;
            
            // Dica de polimento: Se não tiver save, deixamos o botão meio transparente
            Color btnColor = continueButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().color;
            btnColor.a = GameData.HasSaveData ? 1f : 0.3f;
            continueButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().color = btnColor;
        }
    }

    public void NewGame()
    {
        // 1. Resetamos o "Cofre" para os valores iniciais
        GameData.PlayerLevel = 1;
        GameData.CurrentSouls = 0;
        GameData.HasSaveData = false;
        GameData.SavedStats.Clear();

        // 2. Esconde o menu
        mainMenuPanel.SetActive(false);

        // 3. Liga a tela preta e o script da cutscene
        // OBS: Não precisamos de LoadScene aqui! O CutsceneManager cuidará disso no final.
        cutscenePanel.SetActive(true);
        cutsceneManager.SetActive(true); 
    }

    public void ContinueGame()
    {
        // Como o Start já validou o botão, aqui apenas carregamos a cena.
        Debug.Log("<color=cyan>Carregando Memória do Jardim...</color>");
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }
}