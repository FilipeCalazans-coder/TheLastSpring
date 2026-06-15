using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System; // NOVO: Permite passar blocos de código (Action) como parâmetros

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance; 

    [Header("Fade Tradicional (Cor)")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Transição de Vídeo (Universal)")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private RawImage videoRawImage;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }

        if (videoRawImage != null) videoRawImage.gameObject.SetActive(false);
    }

    // ==========================================
    // SISTEMA DE TRANSIÇÃO COM VÍDEO E AÇÃO (EX: MORTE / TELEPORTE)
    // ==========================================
    public void PlayVideoTransition(VideoClip enterClip, VideoClip exitClip, Action onMidpointAction)
    {
        StartCoroutine(VideoTransitionRoutine(enterClip, exitClip, onMidpointAction));
    }

    private IEnumerator VideoTransitionRoutine(VideoClip enterClip, VideoClip exitClip, Action onMidpointAction)
    {
        // 1. Bloqueia os cliques na ecrã
        if (fadeCanvasGroup != null) fadeCanvasGroup.blocksRaycasts = true;
        if (videoRawImage != null) videoRawImage.gameObject.SetActive(true);

        // 2. Toca o vídeo de ENTRADA (Tela vai ficando preta/animada)
        if (enterClip != null && videoPlayer != null)
        {
            videoPlayer.clip = enterClip;
            videoPlayer.Play();
            yield return new WaitForSeconds((float)enterClip.length);
        }

        // 3. MOMENTO CEGO: A tela está coberta! Executamos o que você pediu.
        // Pode ser curar a vida, mover a personagem de lugar, abrir o baú, etc.
        onMidpointAction?.Invoke();

        // 4. Toca o vídeo de SAÍDA (Tela volta a revelar o jogo)
        if (exitClip != null && videoPlayer != null)
        {
            videoPlayer.clip = exitClip;
            videoPlayer.Play();
            yield return new WaitForSeconds((float)exitClip.length);
        }

        // 5. Limpa a tela
        if (videoRawImage != null) videoRawImage.gameObject.SetActive(false);
        if (fadeCanvasGroup != null) fadeCanvasGroup.blocksRaycasts = false;
    }

    // ==========================================
    // SISTEMA DE CARREGAR NOVA FASE (LEVEL) COM VÍDEO
    // ==========================================
    public void LoadSceneWithVideo(VideoClip enterClip, string sceneName)
    {
        StartCoroutine(LoadSceneVideoRoutine(enterClip, sceneName));
    }

    private IEnumerator LoadSceneVideoRoutine(VideoClip enterClip, string sceneName)
    {
        if (fadeCanvasGroup != null) fadeCanvasGroup.blocksRaycasts = true;
        if (videoRawImage != null) videoRawImage.gameObject.SetActive(true);

        // 1. Toca a transição
        if (enterClip != null && videoPlayer != null)
        {
            videoPlayer.clip = enterClip;
            videoPlayer.Play();
            yield return new WaitForSeconds((float)enterClip.length);
        }

        // 2. Carrega a cena assincronamente por trás do vídeo
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone) yield return null;

        // 3. Assim que carregar, esconde o vídeo e usa o fade suave normal
        if (videoRawImage != null) videoRawImage.gameObject.SetActive(false);
        yield return StartCoroutine(Fade(0f));
    }

    // ==========================================
    // MÉTODOS ORIGINAIS MANTIDOS
    // ==========================================
    public void FadeAndLoadScene(string sceneName) { StartCoroutine(FadeRoutine(sceneName)); }
    public void EnsureVisible() { fadeCanvasGroup.alpha = 0f; fadeCanvasGroup.blocksRaycasts = false; }
    public void SetToBlack() { if (fadeCanvasGroup != null) { fadeCanvasGroup.alpha = 1f; fadeCanvasGroup.blocksRaycasts = true; } }
    public void LoadSceneFromCutscene(string sceneName) { StartCoroutine(LoadFromCutsceneRoutine(sceneName)); }
    
    private IEnumerator LoadFromCutsceneRoutine(string sceneName)
    {
        SetToBlack();
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone) yield return null;
        yield return StartCoroutine(Fade(0f));
    }
    
    private IEnumerator FadeRoutine(string sceneName)
    {
        yield return StartCoroutine(Fade(1f));
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone) yield return null;
        yield return StartCoroutine(Fade(0f));
    }

    public IEnumerator Fade(float targetAlpha)
    {
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
        if (targetAlpha <= 0) fadeCanvasGroup.blocksRaycasts = false;
    }
}