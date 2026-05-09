using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Project.Scripts.UI
{
    public class CutsceneManager : MonoBehaviour
    {
        [Header("Configurações da Cutscene")]
        [Tooltip("Arraste aqui as 4 imagens da cutscene em ordem.")]
        [SerializeField] private Sprite[] cutsceneImages;
        [Tooltip("Tempo em segundos que cada imagem fica na tela.")]
        [SerializeField] private float timePerImage = 3f;
        [Tooltip("Tempo em segundos para o efeito de transição (Fade).")]
        [SerializeField] private float fadeDuration = 1f;

        [Header("Referências")]
        [Tooltip("A imagem da UI que vai mostrar a cutscene.")]
        [SerializeField] private Image displayImage;
        [Tooltip("O nome da cena que deve carregar depois da cutscene.")]
        [SerializeField] private string nextSceneName = "GameScene";

        private void Start()
        {
            // CORREÇÃO: "Length" com 'L' maiúsculo
            if (cutsceneImages.Length > 0)
            {
                StartCoroutine(PlayCutscene());
            }
            else
            {
                Debug.LogWarning("Nenhuma imagem configurada na cutscene!");
                LoadNextScene();
            }
        }

        private IEnumerator PlayCutscene()
        {
            // Garante que a imagem está totalmente transparente no início
            SetImageAlpha(0f);

            for (int i = 0; i < cutsceneImages.Length; i++)
            {
                // Troca a imagem
                displayImage.sprite = cutsceneImages[i];

                // Efeito de Fade In (Aparecer)
                yield return StartCoroutine(FadeImage(0f, 1f));

                // Espera o tempo determinado com a imagem na tela
                yield return new WaitForSeconds(timePerImage);

                // Efeito de Fade Out (Desaparecer)
                // Pula o Fade Out se for a última imagem, para ir direto pro jogo escuro
                if (i < cutsceneImages.Length - 1)
                {
                    yield return StartCoroutine(FadeImage(1f, 0f));
                }
            }

            // Fade out final mais longo antes de carregar o jogo
            yield return StartCoroutine(FadeImage(1f, 0f));
            
            LoadNextScene();
        }

        private IEnumerator FadeImage(float startAlpha, float targetAlpha)
        {
            float elapsedTime = 0f;
            Color color = displayImage.color;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                // Interpola a transparência suavemente
                color.a = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / fadeDuration);
                displayImage.color = color;
                yield return null;
            }

            // Garante que o valor final exato seja aplicado
            color.a = targetAlpha;
            displayImage.color = color;
        }

        private void SetImageAlpha(float alpha)
        {
            Color c = displayImage.color;
            c.a = alpha;
            displayImage.color = c;
        }

        private void LoadNextScene()
        {

            SceneManager.LoadScene(nextSceneName);
        }

        // Função para colocar um botão "Pular Cutscene"
        public void SkipCutscene()
        {
            StopAllCoroutines();
            LoadNextScene();
        }
    }
}