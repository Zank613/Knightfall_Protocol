using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    #region Audio Sources
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource ambianceSource;
    public AudioSource sfxSource; // For one-shot sounds
    #endregion

    #region Player Sounds
    [Header("Player SFX")]
    public AudioClip playerFootstep;
    public AudioClip playerJump;
    public AudioClip playerLand;
    public AudioClip playerAttack;
    public AudioClip playerHurt;
    public AudioClip playerDeath;
    #endregion

    #region Enemy Sounds
    [Header("Enemy SFX")]
    public AudioClip enemyFootstep;
    public AudioClip enemyAttack;
    public AudioClip enemyHurt;
    public AudioClip enemyDeath;
    #endregion

    #region Boss Sounds
    [Header("Boss SFX")]
    public AudioClip bossFootstep;
    public AudioClip bossAttack;
    public AudioClip bossHurt;
    public AudioClip bossDefeat;
    #endregion

    #region Music
    [Header("Music")]
    public AudioClip menuMusic;
    public AudioClip ambianceMusic;
    public AudioClip combatMusic;
    public AudioClip bossFightMusic;
    #endregion

    #region Ambiance
    [Header("Ambiance")]
    public AudioClip cityAmbiance;
    public AudioClip windAmbiance;
    #endregion

    #region Settings
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float ambianceVolume = 0.5f;
    #endregion

    private AudioClip currentMusic;

    #region Initialization
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAudioSources()
    {
        // Create audio sources if they don't exist
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (ambianceSource == null)
        {
            ambianceSource = gameObject.AddComponent<AudioSource>();
            ambianceSource.loop = true;
            ambianceSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        UpdateVolumes();
    }

    void UpdateVolumes()
    {
        musicSource.volume = musicVolume * masterVolume;
        sfxSource.volume = sfxVolume * masterVolume;
        ambianceSource.volume = ambianceVolume * masterVolume;
    }
    #endregion

    #region Player Audio
    public void PlayPlayerFootstep()
    {
        PlaySFX(playerFootstep, 0.3f); // Quieter footsteps
    }

    public void PlayPlayerJump()
    {
        PlaySFX(playerJump);
    }

    public void PlayPlayerLand()
    {
        PlaySFX(playerLand, 0.6f);
    }

    public void PlayPlayerAttack()
    {
        PlaySFX(playerAttack, 0.8f);
    }

    public void PlayPlayerHurt()
    {
        PlaySFX(playerHurt);
    }

    public void PlayPlayerDeath()
    {
        PlaySFX(playerDeath);
    }
    #endregion

    #region Enemy Audio
    public void PlayEnemyFootstep()
    {
        PlaySFX(enemyFootstep, 0.3f);
    }

    public void PlayEnemyAttack()
    {
        PlaySFX(enemyAttack, 0.7f);
    }

    public void PlayEnemyHurt()
    {
        PlaySFX(enemyHurt, 0.8f);
    }

    public void PlayEnemyDeath()
    {
        PlaySFX(enemyDeath);
    }
    #endregion

    #region Boss Audio
    public void PlayBossFootstep()
    {
        PlaySFX(bossFootstep, 0.4f);
    }

    public void PlayBossAttack()
    {
        PlaySFX(bossAttack, 0.9f);
    }

    public void PlayBossHurt()
    {
        PlaySFX(bossHurt);
    }

    public void PlayBossDefeat()
    {
        PlaySFX(bossDefeat);
    }
    #endregion

    #region Music Control
    public void PlayMusic(AudioClip music, bool fadeIn = true)
    {
        if (music == null || music == currentMusic) return;

        if (fadeIn)
        {
            StartCoroutine(FadeMusic(currentMusic, music, 1f));
        }
        else
        {
            musicSource.Stop();
            musicSource.clip = music;
            musicSource.Play();
        }

        currentMusic = music;
    }

    public void PlayMenuMusic()
    {
        PlayMusic(menuMusic);
    }

    public void PlayAmbianceMusic()
    {
        PlayMusic(ambianceMusic);
    }

    public void PlayCombatMusic()
    {
        PlayMusic(combatMusic, false); // Instant transition for combat
    }

    public void PlayBossFightMusic()
    {
        PlayMusic(bossFightMusic, false); // Instant transition for boss
    }

    public void StopMusic(bool fadeOut = true)
    {
        if (fadeOut)
        {
            StartCoroutine(FadeOutMusic(1f));
        }
        else
        {
            musicSource.Stop();
            currentMusic = null;
        }
    }

    System.Collections.IEnumerator FadeMusic(AudioClip from, AudioClip to, float duration)
    {
        float elapsed = 0f;
        float startVolume = musicSource.volume;

        // Fade out current
        while (elapsed < duration / 2)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Switch clip
        musicSource.Stop();
        musicSource.clip = to;
        musicSource.Play();

        // Fade in new
        elapsed = 0f;
        while (elapsed < duration / 2)
        {
            musicSource.volume = Mathf.Lerp(0f, musicVolume * masterVolume, elapsed / (duration / 2));
            elapsed += Time.deltaTime;
            yield return null;
        }

        musicSource.volume = musicVolume * masterVolume;
    }

    System.Collections.IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = musicVolume * masterVolume;
        currentMusic = null;
    }
    #endregion

    #region Ambiance Control
    public void PlayAmbiance(AudioClip ambiance)
    {
        if (ambiance == null) return;

        ambianceSource.clip = ambiance;
        ambianceSource.Play();
    }

    public void PlayCityAmbiance()
    {
        PlayAmbiance(cityAmbiance);
    }

    public void PlayWindAmbiance()
    {
        PlayAmbiance(windAmbiance);
    }

    public void StopAmbiance()
    {
        ambianceSource.Stop();
    }
    #endregion

    #region SFX Helper

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, volumeScale * sfxVolume * masterVolume);
    }

    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, volumeScale * sfxVolume * masterVolume);
    }
    #endregion

    #region Volume Control
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
        UpdateVolumes();
    }

    public void SetAmbianceVolume(float volume)
    {
        ambianceVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }
    #endregion
}