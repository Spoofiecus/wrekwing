using UnityEngine;

namespace WreckWing.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Volume Settings")]
        [SerializeField] private float masterVolume = 1f;
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 0.8f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null)
                musicSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null)
                sfxSource = gameObject.AddComponent<AudioSource>();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
        }

        public void PlayMusic(AudioClip clip)
        {
            if (clip == null) return;
            if (musicSource.isPlaying) musicSource.Stop();
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }

        public void StopMusic()
        {
            if (musicSource.isPlaying) musicSource.Stop();
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        private void UpdateVolumes()
        {
            if (musicSource.isPlaying)
                musicSource.volume = musicVolume * masterVolume;
        }
    }
}
