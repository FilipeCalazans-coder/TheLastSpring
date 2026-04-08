using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance; // Singleton para fácil acesso
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Importante: o fader não pode morrer no LoadScene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void FadeAndLoadScene(string sceneName)
    {
        StartCoroutine(FadeRoutine(sceneName));
    }

    private IEnumerator FadeRoutine(string sceneName)
    {
        // 1. Fade In (Tela fica preta)
        yield return StartCoroutine(Fade(1f));

        // 2. Carrega a cena enquanto a tela está preta
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone) yield return null;

        // 3. Fade Out (Tela volta ao normal)
        yield return StartCoroutine(Fade(0f));
    }

   private IEnumerator Fade(float targetAlpha)
    {
        // Se vamos ficar pretos (Fade In), começamos a bloquear cliques IMEDIATAMENTE
        if (targetAlpha > 0) fadeCanvasGroup.blocksRaycasts = true;

        float startAlpha = fadeCanvasGroup.alpha;
        float timer = 0;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;

        // Se terminamos o Fade Out (Alpha = 0), paramos de bloquear cliques
        if (targetAlpha <= 0) fadeCanvasGroup.blocksRaycasts = false;
    }
}