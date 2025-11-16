using UnityEngine;

namespace Core.Audio
{
    public class MusicManager : MonoBehaviour
    {
        private static MusicManager Instance { get; set; }

        private AudioSource _audioSource;

        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private float defaultVolume = 1f;

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
        }

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                Debug.LogError($"{nameof(_audioSource)} component is missing on {nameof(MusicManager)}!");
                return;
            }

            if (backgroundMusic != null)
            {
                PlayBackgroundMusic(false, backgroundMusic);
            }
        }

        private void PlayBackgroundMusic(bool resetSong, AudioClip audioClip = null, float volume = 1f)
        {
            if (_audioSource == null)
            {
                Debug.LogError($"{nameof(_audioSource)} is not initialized!");
                return;
            }

            if (audioClip != null)
            {
                _audioSource.clip = audioClip;
                _audioSource.volume = volume;
                _audioSource.Play();
            }
            else if (_audioSource.clip != null)
            {
                if (resetSong)
                {
                    _audioSource.Stop();
                    _audioSource.time = 0f;
                }

                _audioSource.volume = volume;
                _audioSource.Play();
            }
        }
    }
}