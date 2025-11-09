using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    
    [Header("Audio Controls")]
    public Toggle soundToggle;
    public Slider volumeSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Character Display")]
    public Transform playerDisplayPosition;
    public Transform vagabondDisplayPosition;
    public GameObject playerPrefab;
    public GameObject vagabondPrefab;

    [Header("Title Animation")]
    public Text titleText;
    public float titlePulseSpeed = 2f;
    public float titlePulseAmount = 1.2f;
    
    private const string SOUND_PREF = "SoundEnabled";
    private const string VOLUME_PREF = "MasterVolume";
    private const string MUSIC_PREF = "MusicVolume";
    private const string SFX_PREF = "SFXVolume";

    private Vector3 titleOriginalScale;

    void Start()
    {
        // Store original title scale
        if (titleText != null)
        {
            titleOriginalScale = titleText.transform.localScale;
        }

        // Load saved preferences
        LoadAudioSettings();
        
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Add audio listeners
        SetupAudioListeners();

        // Spawn characters
        SpawnCharacterDisplays();

        // Play menu music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }
    }

    void Update()
    {
        // Animate title
        AnimateTitle();
    }

    void SetupAudioListeners()
    {
        if (soundToggle != null)
        {
            soundToggle.onValueChanged.AddListener(ToggleSound);
        }
        
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(ChangeMasterVolume);
        }

        if (musicSlider != null)
        {
            musicSlider.onValueChanged.AddListener(ChangeMusicVolume);
        }

        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.AddListener(ChangeSFXVolume);
        }
    }

    void SpawnCharacterDisplays()
    {
        // Spawn Player
        if (playerDisplayPosition != null && playerPrefab != null)
        {
            GameObject player = Instantiate(playerPrefab, playerDisplayPosition.position, Quaternion.identity);
            
            // Set to idle animation facing right
            Animator playerAnim = player.GetComponent<Animator>();
            if (playerAnim != null)
            {
                playerAnim.SetFloat("Move", 0);
                playerAnim.SetInteger("Direction", 1);
            }

            // Disable scripts
            DisableScripts(player);

            Debug.Log("Main Menu: Player spawned");
        }

        // Spawn Vagabond
        if (vagabondDisplayPosition != null && vagabondPrefab != null)
        {
            GameObject vagabond = Instantiate(vagabondPrefab, vagabondDisplayPosition.position, Quaternion.identity);
            
            // Set to idle animation facing left
            Animator vagabondAnim = vagabond.GetComponent<Animator>();
            if (vagabondAnim != null)
            {
                vagabondAnim.SetBool("IsWalking", false);
            }

            // Flip to face player
            SpriteRenderer sr = vagabond.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.flipX = true;
            }

            // Disable scripts
            DisableScripts(vagabond);

            Debug.Log("Main Menu: Vagabond spawned");
        }
    }

    void DisableScripts(GameObject obj)
    {
        // Disable all scripts except Animator
        MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (!(script is Animator)) script.enabled = false;
        }

        // Remove physics
        Collider2D col = obj.GetComponent<Collider2D>();
        if (col != null) Destroy(col);
        
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null) Destroy(rb);
    }

    void AnimateTitle()
    {
        if (titleText == null) return;

        // Pulsing scale effect
        float scale = 1f + Mathf.Sin(Time.time * titlePulseSpeed) * (titlePulseAmount - 1f) * 0.5f;
        titleText.transform.localScale = titleOriginalScale * scale;
    }

    // === BUTTON FUNCTIONS ===
    
    public void PlayGame()
    {
        // Fade out music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(true);
        }

        SceneManager.LoadScene("Prologue");
    }
    
    public void OpenSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
    
    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }
    
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // === AUDIO FUNCTIONS ===
    
    public void ToggleSound(bool isEnabled)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(isEnabled ? 1f : 0f);
        }
        else
        {
            AudioListener.volume = isEnabled ? 1f : 0f;
        }
        
        PlayerPrefs.SetInt(SOUND_PREF, isEnabled ? 1 : 0);
        PlayerPrefs.Save();
        
        Debug.Log($"Sound {(isEnabled ? "Enabled" : "Disabled")}");
    }
    
    public void ChangeMasterVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(volume);
        }
        else
        {
            AudioListener.volume = volume;
        }
        
        PlayerPrefs.SetFloat(VOLUME_PREF, volume);
        PlayerPrefs.Save();
    }

    public void ChangeMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
        }
        
        PlayerPrefs.SetFloat(MUSIC_PREF, volume);
        PlayerPrefs.Save();
    }

    public void ChangeSFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(volume);
        }
        
        PlayerPrefs.SetFloat(SFX_PREF, volume);
        PlayerPrefs.Save();
    }
    
    private void LoadAudioSettings()
    {
        // Load sound enabled state
        bool soundEnabled = PlayerPrefs.GetInt(SOUND_PREF, 1) == 1;
        if (soundToggle != null)
        {
            soundToggle.isOn = soundEnabled;
        }
        
        // Load master volume
        float masterVolume = PlayerPrefs.GetFloat(VOLUME_PREF, 1f);
        if (volumeSlider != null)
        {
            volumeSlider.value = masterVolume;
        }

        // Load music volume
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_PREF, 0.7f);
        if (musicSlider != null)
        {
            musicSlider.value = musicVolume;
        }

        // Load SFX volume
        float sfxVolume = PlayerPrefs.GetFloat(SFX_PREF, 1f);
        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVolume;
        }
        
        // Apply settings to AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(soundEnabled ? masterVolume : 0f);
            AudioManager.Instance.SetMusicVolume(musicVolume);
            AudioManager.Instance.SetSFXVolume(sfxVolume);
        }
    }
}