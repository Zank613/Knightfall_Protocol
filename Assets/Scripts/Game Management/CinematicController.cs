using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles ALL cinematics in the game:
/// - Vagabond intro (Prologue)
/// - Prologue ending (death → mage dialogue → Cyberpunk)
/// - Samurai boss intro (dramatic entrance)
/// </summary>
public class CinematicController : MonoBehaviour
{
    public static CinematicController Instance;

    #region Scene References
    [Header("Characters")]
    public GameObject player;
    public GameObject vagabond;
    public GameObject mage;
    public GameObject samurai;

    [Header("Camera")]
    public Camera mainCamera;
    private Vector3 originalCameraPosition;
    private float originalCameraSize;
    #endregion

    #region Cutscene Positions
    [Header("Prologue Cutscene Positions")]
    [Tooltip("Where player teleports for ending scene")]
    public Transform playerCutscenePos;
    
    [Tooltip("Where mage appears for ending scene")]
    public Transform druidCutscenePos;
    #endregion

    #region UI Elements
    [Header("UI")]
    public Image vignetteImage;
    #endregion

    #region Dialogue Keys
    [Header("Dialogue")]
    public string druidDialogueKey = "DruidPrologueEnd";
    #endregion

    #region Audio
    [Header("Audio")]
    public AudioClip bossFightMusic;
    private AudioSource audioSource;
    #endregion

    #region Initialization
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Store original camera state
        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.position;
            originalCameraSize = mainCamera.orthographicSize;
        }

        // Ensure vignette is hidden at start
        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(false);
        }
    }
    #endregion

    #region PROLOGUE CINEMATICS

    /// <summary>
    /// Called when player gets close to Vagabond for the first time
    /// Shows intro dialogue with vignette effect
    /// </summary>
    public void StartVagabondIntro(Vagabond vagabondScript)
    {
        StartCoroutine(VagabondIntroSequence(vagabondScript));
    }

    IEnumerator VagabondIntroSequence(Vagabond vagabondScript)
    {
        Debug.Log("=== VAGABOND INTRO CINEMATIC ===");

        // 1. Freeze Player
        Player playerScript = player.GetComponent<Player>();
        playerScript.enabled = false;
        playerScript.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        playerScript.GetComponent<Animator>().SetFloat("Move", 0);

        // 2. Freeze Vagabond
        vagabondScript.PauseForCutscene();

        // 3. Show Vignette for dramatic effect
        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(0.3f);

        // 4. Play Vagabond's intro dialogue
        bool dialogueComplete = false;
        DialogueManager.SimplePopUp(
            vagabondScript.transform, 
            vagabondScript.introDialogue, 
            () => { dialogueComplete = true; }
        );

        // Wait for dialogue to finish
        while (!dialogueComplete)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        // 5. Hide Vignette
        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(false);
        }

        // 6. Unfreeze everyone - FIGHT BEGINS
        playerScript.enabled = true;
        vagabondScript.UnpauseFromCutscene();

        Debug.Log("Vagabond intro complete - impossible boss fight begins!");
    }

    /// <summary>
    /// Called by Player.cs when HP reaches 0 in Prologue
    /// Triggers the ending cutscene with the Mage
    /// </summary>
    public void StartPrologueEnding()
    {
        StartCoroutine(PrologueEndingSequence());
    }

    IEnumerator PrologueEndingSequence()
    {
        Debug.Log("=== PROLOGUE ENDING CINEMATIC ===");

        // 1. Freeze Player COMPLETELY
        Player playerScript = player.GetComponent<Player>();
        Animator playerAnimator = player.GetComponent<Animator>();
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        
        if (playerScript != null)
        {
            playerScript.enabled = false;
        }
        
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }
        
        if (playerAnimator != null)
        {
            // Stop ALL movement animations
            playerAnimator.SetFloat("Move", 0);
            playerAnimator.SetBool("IsWalking", false);
        }

        // 2. Freeze Vagabond if still active
        if (vagabond != null && vagabond.activeInHierarchy)
        {
            Vagabond vagabondScript = vagabond.GetComponent<Vagabond>();
            if (vagabondScript != null)
            {
                vagabondScript.PauseForCutscene();
            }
        }

        // 3. Fade to black effect
        yield return new WaitForSeconds(1f);

        // 4. Play peaceful/mysterious music
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAmbianceMusic();
        }

        // 5. Despawn Vagabond
        if (vagabond != null)
        {
            vagabond.SetActive(false);
        }

        // 6. Spawn Mage
        if (mage != null)
        {
            mage.SetActive(true);
        }

        // 7. Teleport characters to cutscene positions
        if (player != null && playerCutscenePos != null)
        {
            player.transform.position = playerCutscenePos.position;
        }

        if (mage != null && druidCutscenePos != null)
        {
            mage.transform.position = druidCutscenePos.position;
        }

        // 8. Force player to face RIGHT
        if (playerAnimator != null)
        {
            playerAnimator.SetInteger("Direction", 1); // 1 = Right
        }

        // 9. Reposition camera to frame both characters
        if (mainCamera != null && playerCutscenePos != null && druidCutscenePos != null)
        {
            Vector3 centerPoint = (playerCutscenePos.position + druidCutscenePos.position) / 2;
            mainCamera.transform.position = new Vector3(
                centerPoint.x, 
                centerPoint.y, 
                mainCamera.transform.position.z
            );
            mainCamera.orthographicSize = 4f;
        }

        // 10. Show Vignette
        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(0.5f);

        // 11. Mage delivers final dialogue
        bool dialogueComplete = false;
        if (mage != null)
        {
            DialogueManager.SimplePopUp(
                mage.transform, 
                druidDialogueKey, 
                () => { dialogueComplete = true; }
            );
        }

        // Wait for dialogue to complete
        while (!dialogueComplete)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // 12. Hide Vignette
        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(0.3f);

        // 13. Load next scene
        Debug.Log("Loading Cyberpunk scene...");
        SceneManager.LoadScene("Cyberpunk");
    }

    #endregion

    #region SAMURAI BOSS CINEMATICS

    /// <summary>
    /// Called when player enters boss arena
    /// Dramatic entrance: Samurai falls from sky
    /// </summary>
    public void StartSamuraiBossCinematic()
    {
        StartCoroutine(SamuraiBossIntroSequence());
    }

    IEnumerator SamuraiBossIntroSequence()
    {
        Debug.Log("=== SAMURAI BOSS CINEMATIC STARTED ===");

        // 1. STOP COMBAT MUSIC IMMEDIATELY
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(false); // Instant stop
        }

        // 2. Get references
        Player playerScript = player.GetComponent<Player>();
        Samurai samuraiScript = samurai.GetComponent<Samurai>();

        if (playerScript == null || samuraiScript == null)
        {
            Debug.LogError("Missing player or samurai reference!");
            yield break;
        }

        // 3. Freeze Player
        playerScript.enabled = false;
        playerScript.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        playerScript.GetComponent<Animator>().SetFloat("Move", 0);

        Debug.Log("=== WAITING FOR SAMURAI TO FALL ===");

        // 4. Wait for Samurai to fall and land
        Rigidbody2D samuraiRb = samuraiScript.GetComponent<Rigidbody2D>();
        
        // Give him a moment to start falling
        yield return new WaitForSeconds(0.5f);

        // Wait until he lands
        float timeout = 5f;
        float elapsed = 0f;
        while (Mathf.Abs(samuraiRb.linearVelocity.y) > 0.1f && elapsed < timeout)
        {
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        // Extra moment for dramatic impact
        yield return new WaitForSeconds(0.3f);

        Debug.Log("=== SAMURAI LANDED! FREEZING ===");

        // 5. Freeze Samurai after landing
        samuraiScript.FreezeForCinematic();

        // 6. Show Vignette
        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(0.3f);

        Debug.Log("=== STARTING BOSS DIALOGUE ===");

        // 7. Play Boss intro dialogue
        bool dialogueComplete = false;
        DialogueManager.SimplePopUp(
            samuraiScript.transform,
            samuraiScript.introDialogue,
            () => {
                dialogueComplete = true;
                Debug.Log("=== DIALOGUE COMPLETE ===");
            }
        );

        // Wait for dialogue to finish
        while (!dialogueComplete)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        // 8. Hide Vignette
        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(false);
        }

        // 9. START BOSS MUSIC
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBossFightMusic();
            Debug.Log("=== BOSS MUSIC STARTED ===");
        }

        // Dramatic pause before fight
        yield return new WaitForSeconds(0.5f);

        // 10. UNFREEZE EVERYTHING - FIGHT BEGINS!
        playerScript.enabled = true;
        samuraiScript.UnfreezeAndStartFight();

        Debug.Log("<color=cyan>=== BOSS FIGHT STARTED ===</color>");
    }

    /// <summary>
    /// Called when Samurai is defeated
    /// Fades out boss music
    /// </summary>
    public void OnSamuraiDefeated()
    {
        Debug.Log("Samurai defeated - fading music");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(true); // Fade out
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutMusic(2f));
        }
    }

    IEnumerator FadeOutMusic(float duration)
    {
        if (audioSource == null) yield break;

        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Resets camera to original position and size
    /// </summary>
    public void ResetCamera()
    {
        if (mainCamera != null)
        {
            mainCamera.transform.position = originalCameraPosition;
            mainCamera.orthographicSize = originalCameraSize;
        }
    }

    /// <summary>
    /// Shows vignette effect
    /// </summary>
    public void ShowVignette()
    {
        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Hides vignette effect
    /// </summary>
    public void HideVignette()
    {
        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(false);
        }
    }

    #endregion
}