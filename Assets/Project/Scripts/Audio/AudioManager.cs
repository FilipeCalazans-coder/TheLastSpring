using UnityEngine;
using System.Collections;

namespace Project.Scripts.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Coluna de Som")]
        [SerializeField] private AudioSource musicSource;
        
        [Header("Faixas de Música")]
        public AudioClip menuMusic;
        
        [Tooltip("Lista de músicas calmas. O jogo vai escolher uma à sorte!")]
        public AudioClip[] ambientTracks; // AGORA É UMA LISTA (ARRAY)
        
        public AudioClip combatMusic;

        [Header("Configurações de Transição")]
        public float fadeDuration = 1.5f;

        private Coroutine _fadeRoutine;

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
                return;
            }
        }

        private void Start()
        {
            PlayMenuMusic();
        }

        public void PlayMusic(AudioClip newTrack)
        {
            if (musicSource == null || newTrack == null) return;
            if (musicSource.clip == newTrack) return;

            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }

            _fadeRoutine = StartCoroutine(FadeMusicRoutine(newTrack));
        }

        private IEnumerator FadeMusicRoutine(AudioClip newTrack)
        {
            float targetVolume = 1f; 

            if (musicSource.clip != null)
            {
                while (musicSource.volume > 0)
                {
                    musicSource.volume -= targetVolume * (Time.unscaledDeltaTime / fadeDuration);
                    yield return null; 
                }
            }

            musicSource.clip = newTrack;
            musicSource.Play();

            musicSource.volume = 0f;
            while (musicSource.volume < targetVolume)
            {
                musicSource.volume += targetVolume * (Time.unscaledDeltaTime / fadeDuration);
                yield return null;
            }

            musicSource.volume = targetVolume;
        }

        // --- ATALHOS INTELIGENTES ---
        public void PlayMenuMusic() => PlayMusic(menuMusic);
        
        // NOVO: Escolhe uma música ambiente aleatória da tua lista!
        public void PlayAmbientMusic() 
        {
            if (ambientTracks != null && ambientTracks.Length > 0)
            {
                int randomTrackIndex = Random.Range(0, ambientTracks.Length);
                PlayMusic(ambientTracks[randomTrackIndex]);
            }
        }

        public void PlayCombatMusic() => PlayMusic(combatMusic);
    }
}