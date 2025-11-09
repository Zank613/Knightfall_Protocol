using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeathMenuScene : MonoBehaviour
{
    #region UI Elements
    [Header("UI References")]
    public Text gameOverText;
    public Text enemyDialogueText;
    public Button restartButton;
    public Button mainMenuButton;
    #endregion

    #region Character Display
    [Header("Character Display Positions")]
    public Transform playerDisplayPosition;
    public Transform enemyDisplayPosition;
    
    [Header("Prefabs")]
    public GameObject playerPrefab;
    #endregion

    #region Settings
    [Header("Animation Settings")]
    public float dialogueTypingSpeed = 0.05f;
    public float initialDelay = 1f;
    #endregion

    #region Enemy Mockery Lines
    private string[] crowMockery = new string[]
    {
        "CAW CAW! Too slow, human!",
        "Your bones will feed my flock!",
        "The skies belong to the crows!",
        "Pathetic... utterly pathetic."
    };

    private string[] sickleTankMockery = new string[]
    {
        "WEAK! Is this all you have?",
        "You dare challenge a tank?!",
        "HAHAHA! Crushed like an insect!",
        "Get stronger... if you can."
    };

    private string[] cyberknightMockery = new string[]
    {
        "You telegraphed every move...",
        "Predictable. Disappointing.",
        "My blade was faster. Always.",
        "Return when you've learned REAL skill."
    };

    private string[] samuraiMockery = new string[]
    {
        "You fought with honor... but died nonetheless.",
        "The path of the warrior is harsh.",
        "Your spirit burns bright... but your body failed.",
        "Rise again, warrior. I await our next duel."
    };

    private string[] genericMockery = new string[]
    {
        "HAHAHA! Pathetic!",
        "Too weak to continue?",
        "You never stood a chance!",
        "Maybe next time... if there IS a next time."
    };
    #endregion

    #region Initialization
    void Start()
    {
        // Setup button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartLevel);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        // Start the death scene sequence
        StartCoroutine(DeathSceneSequence());
    }
    #endregion

    #region Death Scene Sequence
    IEnumerator DeathSceneSequence()
    {
        // Initial delay for dramatic effect
        yield return new WaitForSeconds(initialDelay);

        // Spawn player (defeated)
        SpawnPlayerDisplay();

        // Spawn enemy (victorious)
        SpawnEnemyDisplay();

        // Wait a moment
        yield return new WaitForSeconds(0.5f);

        // Type out enemy mockery
        string mockery = GetEnemyMockery();
        yield return StartCoroutine(TypeDialogue(mockery));

        // Optional: Play death music
        if (AudioManager.Instance != null)
        {
            // TODO:
            // AudioManager.Instance.PlayMusic(gameOverMusic);
        }
    }
    #endregion

    #region Character Spawning
    void SpawnPlayerDisplay()
    {
        if (playerDisplayPosition == null || playerPrefab == null)
        {
            Debug.LogWarning("Death Scene: Missing player display setup!");
            return;
        }

        GameObject playerDisplay = Instantiate(playerPrefab, playerDisplayPosition.position, Quaternion.identity);
        
        // Get animator and set to idle
        Animator playerAnim = playerDisplay.GetComponent<Animator>();
        if (playerAnim != null)
        {
            playerAnim.SetFloat("Move", 0);
            playerAnim.SetInteger("Direction", 1); // Face right (toward enemy)
        }

        // Darken the player sprite to show defeat
        SpriteRenderer playerSprite = playerDisplay.GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            playerSprite.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        }

        // Disable ALL scripts except Animator
        MonoBehaviour[] scripts = playerDisplay.GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            if (!(script is Animator)) script.enabled = false;
        }

        // Remove physics components
        Collider2D col = playerDisplay.GetComponent<Collider2D>();
        if (col != null) Destroy(col);
        
        Rigidbody2D rb = playerDisplay.GetComponent<Rigidbody2D>();
        if (rb != null) Destroy(rb);

        Debug.Log("Death Scene: Player spawned");
    }

    void SpawnEnemyDisplay()
    {
        if (enemyDisplayPosition == null)
        {
            Debug.LogWarning("Death Scene: Missing enemy display position!");
            return;
        }

        // Get enemy data from DeathData
        string enemyName = DeathData.EnemyName;
        Sprite enemySprite = DeathData.EnemySprite;
        RuntimeAnimatorController enemyAnimator = DeathData.EnemyAnimator;
        bool enemyFlipX = DeathData.EnemyFlipX;

        if (enemySprite == null)
        {
            Debug.LogWarning("Death Scene: No enemy data found!");
            return;
        }

        // Create enemy display GameObject
        GameObject enemyDisplay = new GameObject($"{enemyName}_Display");
        enemyDisplay.transform.position = enemyDisplayPosition.position;

        // Add sprite renderer
        SpriteRenderer sr = enemyDisplay.AddComponent<SpriteRenderer>();
        sr.sprite = enemySprite;
        sr.sortingOrder = 10;
        sr.flipX = !enemyFlipX; // Flip to face player

        // Add animator if available
        if (enemyAnimator != null)
        {
            Animator anim = enemyDisplay.AddComponent<Animator>();
            anim.runtimeAnimatorController = enemyAnimator;
            
            // Set to idle
            anim.SetBool("IsWalking", false);
            anim.SetBool("IsRunning", false);
            anim.SetFloat("Move", 0);
        }

        Debug.Log($"Death Scene: Enemy '{enemyName}' spawned");
    }
    #endregion

    #region Enemy Mockery
    string GetEnemyMockery()
    {
        string enemyName = DeathData.EnemyName;

        if (string.IsNullOrEmpty(enemyName))
        {
            return GetRandomLine(genericMockery);
        }

        // Match enemy name to mockery lines
        if (enemyName.Contains("Crow"))
        {
            return GetRandomLine(crowMockery);
        }
        else if (enemyName.Contains("Sickle") || enemyName.Contains("Tank"))
        {
            return GetRandomLine(sickleTankMockery);
        }
        else if (enemyName.Contains("Cyber") || enemyName.Contains("Knight"))
        {
            return GetRandomLine(cyberknightMockery);
        }
        else if (enemyName.Contains("Samurai"))
        {
            return GetRandomLine(samuraiMockery);
        }
        else
        {
            return GetRandomLine(genericMockery);
        }
    }

    string GetRandomLine(string[] lines)
    {
        return lines[Random.Range(0, lines.Length)];
    }

    IEnumerator TypeDialogue(string text)
    {
        if (enemyDialogueText == null) yield break;

        enemyDialogueText.text = "";

        foreach (char c in text)
        {
            enemyDialogueText.text += c;
            yield return new WaitForSeconds(dialogueTypingSpeed);
        }
    }
    #endregion

    #region Button Actions
    void RestartLevel()
    {
        // Load the last gameplay scene
        string lastScene = DeathData.LastSceneName;
        if (!string.IsNullOrEmpty(lastScene))
        {
            SceneManager.LoadScene(lastScene);
        }
        else
        {
            Debug.LogWarning("No last scene stored! Loading scene index 1");
            SceneManager.LoadScene("Cyberpunk");
        }
    }

    void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    #endregion
}