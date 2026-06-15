using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // NOVO: Necessário para ler a tecla E

namespace Project.Scripts.UI
{
    public class CutsceneManager : MonoBehaviour
    {
        [Header("Configurações da Cutscene")]
        [Tooltip("Arraste aqui as imagens da cutscene em ordem.")]
        [SerializeField] private Sprite[] cutsceneImages;
        [Tooltip("Tempo em segundos que cada imagem fica na tela.")]
        [SerializeField] private float timePerImage = 3f;
        [Tooltip("Tempo em segundos para o efeito de transição (Fade).")]
        [SerializeField] private float fadeDuration = 1f;

        [Header("Referências")]
        [Tooltip("A imagem da UI que vai mostrar a cutscene.")]
        [SerializeField] private Image displayImage;
        [Tooltip("O painel preto de fundo da cutscene.")]
        [SerializeField] private GameObject cutscenePanel;
        [Tooltip("O nome da cena que deve carregar depois da cutscene.")]
        [SerializeField] private string nextSceneName = "Ruins_Ancestrals";

        private Coroutine _cutsceneRoutine;
        private PlayerControls _playerControls; // Variável para ler os controlos
        private bool _isSkipping = false; // Impede que o jogador clique várias vezes seguidas

        private void Awake()
        {
            // Cria o sistema de controlos quando a cutscene nasce
            _playerControls = new PlayerControls();
        }

        private void OnEnable()
        {
            // Liga os ouvidos para a tecla "E" (Interact)
            _playerControls.Menu.Interact.performed += OnInteractPressed;
            _playerControls.Enable();

            _cutsceneRoutine = StartCoroutine(BeginCutscene());
        }

        private void OnDisable()
        {
            // Desliga os ouvidos quando a cutscene acabar para não dar erros
            _playerControls.Menu.Interact.performed -= OnInteractPressed;
            _playerControls.Disable();

            if (_cutsceneRoutine != null)
            {
                StopCoroutine(_cutsceneRoutine);
                _cutsceneRoutine = null;
            }
        }

        // NOVO: Função que é chamada exatemente quando a tecla "E" é pressionada
        private void OnInteractPressed(InputAction.CallbackContext context)
        {
            // Se o botão foi apertado e ainda não estamos a pular a cutscene
            if (context.performed && !_isSkipping)
            {
                Debug.Log("<color=yellow>Jogador pulou a cutscene!</color>");
                SkipCutscene();
            }
        }

        private IEnumerator BeginCutscene()
        {
            if (displayImage == null)
            {
                Debug.LogError("[CutsceneManager] displayImage não está configurado no Inspector.");
                yield return LoadNextScene();
                yield break;
            }

            if (cutsceneImages == null || cutsceneImages.Length == 0)
            {
                Debug.LogWarning("[CutsceneManager] Nenhuma imagem configurada. Carregando cena diretamente.");
                yield return LoadNextScene();
                yield break;
            }

            yield return PlayCutscene();
        }

        private IEnumerator PlayCutscene()
        {
            SetImageAlpha(0f);

            for (int i = 0; i < cutsceneImages.Length; i++)
            {
                displayImage.sprite = cutsceneImages[i];
                yield return StartCoroutine(FadeImage(0f, 1f));
                
                // Espera o tempo, a menos que o jogador aperte para pular
                yield return new WaitForSeconds(timePerImage);

                if (i < cutsceneImages.Length - 1)
                    yield return StartCoroutine(FadeImage(1f, 0f));
            }

            yield return StartCoroutine(FadeImage(1f, 0f));
            yield return LoadNextScene();
        }

        private IEnumerator FadeImage(float startAlpha, float targetAlpha)
        {
            float elapsedTime = 0f;
            Color color = displayImage.color;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
                displayImage.color = color;
                yield return null;
            }

            color.a = targetAlpha;
            displayImage.color = color;
        }

        private void SetImageAlpha(float alpha)
        {
            Color c = displayImage.color;
            c.a = alpha;
            displayImage.color = c;
        }

        private IEnumerator LoadNextScene()
        {
            if (string.IsNullOrWhiteSpace(nextSceneName)) yield break;

            if (cutscenePanel != null) cutscenePanel.SetActive(false);

            if (SceneFader.Instance != null)
            {
                SceneFader.Instance.LoadSceneFromCutscene(nextSceneName);
            }
            else
            {
                SceneManager.LoadScene(nextSceneName);
            }

            yield break;
        }

        public void SkipCutscene()
        {
            // Trava de segurança para não rodar duas vezes
            if (_isSkipping) return;
            _isSkipping = true;

            // Para todas as animações e imagens que estão a rodar no momento
            StopAllCoroutines();

            // Pula direto para o carregamento da próxima fase
            StartCoroutine(LoadNextScene());
        }
    }
}