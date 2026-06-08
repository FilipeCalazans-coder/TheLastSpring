using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        [Tooltip("O painel preto de fundo da cutscene (será escondido antes de carregar a cena).")]
        [SerializeField] private GameObject cutscenePanel;
        [Tooltip("O nome da cena que deve carregar depois da cutscene.")]
        [SerializeField] private string nextSceneName = "Ruins_Ancestrals";

        private Coroutine _cutsceneRoutine;

        private void OnEnable()
        {
            _cutsceneRoutine = StartCoroutine(BeginCutscene());
        }

        private void OnDisable()
        {
            if (_cutsceneRoutine != null)
            {
                StopCoroutine(_cutsceneRoutine);
                _cutsceneRoutine = null;
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
            if (string.IsNullOrWhiteSpace(nextSceneName))
            {
                Debug.LogError("[CutsceneManager] nextSceneName está vazio. Configure no Inspector.");
                yield break;
            }

            if (!IsSceneInBuildSettings(nextSceneName))
            {
                Debug.LogError(
                    $"[CutsceneManager] A cena '{nextSceneName}' não está em File → Build Settings. " +
                    "Adicione a cena e faça um novo build.");
                yield break;
            }

            if (cutscenePanel != null)
                cutscenePanel.SetActive(false);

            Debug.Log($"[CutsceneManager] Carregando cena: {nextSceneName}");

            AsyncOperation operation = SceneManager.LoadSceneAsync(nextSceneName);
            if (operation == null)
            {
                Debug.LogError($"[CutsceneManager] Falha ao iniciar carregamento da cena '{nextSceneName}'.");
                yield break;
            }

            operation.allowSceneActivation = true;

            while (!operation.isDone)
                yield return null;

            if (SceneFader.Instance != null)
                SceneFader.Instance.EnsureVisible();

            Debug.Log($"[CutsceneManager] Cena '{nextSceneName}' carregada com sucesso.");
        }

        private static bool IsSceneInBuildSettings(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneNameFromPath == sceneName)
                    return true;
            }

            return false;
        }

        public void SkipCutscene()
        {
            StopAllCoroutines();
            StartCoroutine(LoadNextScene());
        }
    }
}
