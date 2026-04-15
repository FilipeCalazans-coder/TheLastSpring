using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Configuração de Cenas")]
    [SerializeField] private string gameSceneName = "Test"; // Nome da sua cena principal

    [Header("Referências de UI")]
    [SerializeField] private Button continueButton;

    private void Start()
    {
        // Regra de Ouro: O botão Continuar só aparece/funciona se o GameData diz que tem save
        if (continueButton != null)
        {
            continueButton.interactable = GameData.HasSaveData;
            
            // Dica de polimento: Se não tiver save, podemos deixar o botão meio transparente
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

        // 2. Carregamos o jogo
        Debug.Log("<color=green>Iniciando Nova Jornada de Fiorella...</color>");
        SceneManager.LoadScene(gameSceneName);
    }

    public void ContinueGame()
    {
        // Como o Start já validou o botão, aqui apenas carregamos a cena.
        // O Start() do Player na outra cena vai ler o GameData automaticamente!
        Debug.Log("<color=cyan>Carregando Memória do Jardim...</color>");
        SceneManager.LoadScene(gameSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }
}