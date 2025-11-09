using UnityEngine;

public class SceneAudioController : MonoBehaviour
{
    [Header("Scene Audio Settings")]
    [Tooltip("Music to play in this scene")]
    public SceneMusicType musicType = SceneMusicType.Ambiance;
    
    [Tooltip("Ambiance sound to play in this scene")]
    public SceneAmbianceType ambianceType = SceneAmbianceType.City;
    
    [Header("Combat Music Trigger")]
    [Tooltip("Automatically switch to combat music when enemies spawn?")]
    public bool enableCombatMusicTrigger = true;
    
    void Start()
    {
        // Play the music for this scene
        PlaySceneMusic();
        
        // Play the ambiance for this scene
        PlaySceneAmbiance();
        
        // Set up combat music detection if enabled
        if (enableCombatMusicTrigger)
        {
            SetupCombatMusicDetection();
        }
    }
    
    void PlaySceneMusic()
    {
        if (AudioManager.Instance == null) return;
        
        switch (musicType)
        {
            case SceneMusicType.Menu:
                AudioManager.Instance.PlayMenuMusic();
                break;
            case SceneMusicType.Ambiance:
                AudioManager.Instance.PlayAmbianceMusic();
                break;
            case SceneMusicType.Combat:
                AudioManager.Instance.PlayCombatMusic();
                break;
            case SceneMusicType.BossFight:
                AudioManager.Instance.PlayBossFightMusic();
                break;
            case SceneMusicType.None:
                AudioManager.Instance.StopMusic(false);
                break;
        }
        
        Debug.Log($"Scene Audio: Playing {musicType} music");
    }
    
    void PlaySceneAmbiance()
    {
        if (AudioManager.Instance == null) return;
        
        switch (ambianceType)
        {
            case SceneAmbianceType.City:
                AudioManager.Instance.PlayCityAmbiance();
                break;
            case SceneAmbianceType.Wind:
                AudioManager.Instance.PlayWindAmbiance();
                break;
            case SceneAmbianceType.None:
                AudioManager.Instance.StopAmbiance();
                break;
        }
        
        Debug.Log($"Scene Audio: Playing {ambianceType} ambiance");
    }
    
    void SetupCombatMusicDetection()
    {
        // Start checking for enemies every 2 seconds
        InvokeRepeating(nameof(CheckForEnemies), 2f, 2f);
    }
    
    void CheckForEnemies()
    {
        // Only switch to combat music if we're currently on ambiance music
        if (musicType != SceneMusicType.Ambiance) return;
        
        // Count active enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] bosses = GameObject.FindGameObjectsWithTag("Boss");
        
        int totalEnemies = enemies.Length + bosses.Length;
        
        if (totalEnemies > 0)
        {
            // Enemies present - switch to combat music
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCombatMusic();
            }
        }
        else
        {
            // No enemies - switch back to ambiance
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayAmbianceMusic();
            }
        }
    }
    
    void OnDestroy()
    {
        // Clean up when scene changes
        CancelInvoke();
    }
}

public enum SceneMusicType
{
    None,
    Menu,
    Ambiance,
    Combat,
    BossFight
}

public enum SceneAmbianceType
{
    None,
    City,
    Wind
}