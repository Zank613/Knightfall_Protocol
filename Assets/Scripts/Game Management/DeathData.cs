using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Static class to store death data between scenes
/// </summary>
public static class DeathData
{
    // Enemy data
    public static string EnemyName { get; private set; }
    public static Sprite EnemySprite { get; private set; }
    public static RuntimeAnimatorController EnemyAnimator { get; private set; }
    public static bool EnemyFlipX { get; private set; }
    
    // Scene data
    public static string LastSceneName { get; private set; }

    /// <summary>
    /// Calling when the player dies to an enemy
    /// </summary>
    public static void RegisterDeath(GameObject enemy, string currentSceneName)
    {
        if (enemy == null)
        {
            Debug.LogWarning("DeathData: No enemy provided!");
            return;
        }

        // Store scene name
        LastSceneName = currentSceneName;

        // Store enemy name
        EnemyName = enemy.name.Replace("(Clone)", "").Trim();

        // Store sprite
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            EnemySprite = sr.sprite;
            EnemyFlipX = sr.flipX;
        }

        // Store animator
        Animator anim = enemy.GetComponent<Animator>();
        if (anim != null)
        {
            EnemyAnimator = anim.runtimeAnimatorController;
        }

        Debug.Log($"<color=red>DeathData: Registered death by {EnemyName} in {LastSceneName}</color>");
    }

    /// <summary>
    /// Load the death menu scene
    /// </summary>
    public static void LoadDeathScene()
    {
        SceneManager.LoadScene("DeathMenu");
    }

    /// <summary>
    /// Clear all data, optional
    /// </summary>
    public static void Clear()
    {
        EnemyName = null;
        EnemySprite = null;
        EnemyAnimator = null;
        EnemyFlipX = false;
        LastSceneName = null;
    }
}