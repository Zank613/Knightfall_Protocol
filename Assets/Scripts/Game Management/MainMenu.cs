using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    
    [Header("Audio")]
    public Toggle soundToggle;
    public Slider volumeSlider;
    
    private const string SOUND_PREF = "SoundEnabled";
    private const string VOLUME_PREF = "MasterVolume";

    void Start()
    {
        // Load saved preferences
        LoadAudioSettings();
        
        // Make sure settings panel is hidden at start
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Add listeners
        if (soundToggle != null)
        {
            soundToggle.onValueChanged.AddListener(ToggleSound);
        }
        
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(ChangeVolume);
        }
    }

    // === MAIN MENU BUTTONS ===
    
    public void PlayGame()
    {
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
        
        // This is for testing in the editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // === AUDIO FUNCTIONS ===
    
    public void ToggleSound(bool isEnabled)
    {
        AudioListener.volume = isEnabled ? 1f : 0f;
        
        if (isEnabled && volumeSlider != null)
        {
            AudioListener.volume = volumeSlider.value;
        }
        
        // Save preference
        PlayerPrefs.SetInt(SOUND_PREF, isEnabled ? 1 : 0);
        PlayerPrefs.Save();
        
        Debug.Log($"Sound {(isEnabled ? "Enabled" : "Disabled")}");
    }
    
    public void ChangeVolume(float volume)
    {
        // Only change volume if sound is enabled
        if (soundToggle != null && soundToggle.isOn)
        {
            AudioListener.volume = volume;
        }
        
        // Save preference
        PlayerPrefs.SetFloat(VOLUME_PREF, volume);
        PlayerPrefs.Save();
    }
    
    private void LoadAudioSettings()
    {
        // Load sound enabled state (default: true)
        bool soundEnabled = PlayerPrefs.GetInt(SOUND_PREF, 1) == 1;
        if (soundToggle != null)
        {
            soundToggle.isOn = soundEnabled;
        }
        
        // Load volume (default: 1.0)
        float volume = PlayerPrefs.GetFloat(VOLUME_PREF, 1f);
        if (volumeSlider != null)
        {
            volumeSlider.value = volume;
        }
        
        // Apply settings
        AudioListener.volume = soundEnabled ? volume : 0f;
    }
}