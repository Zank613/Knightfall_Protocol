using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CinematicController : MonoBehaviour
{
    public static CinematicController Instance;

    [Header("Cutscene Objects")]
    public GameObject player;
    public GameObject vagabond;
    public GameObject mage;
    public Camera mainCamera;
    
    [Header("Samurai Boss Fight")]
    public GameObject samurai;
    public AudioClip bossFightMusic;
    private AudioSource audioSource;

    [Header("Cutscene Positions")]
    public Transform playerCutscenePos;
    public Transform druidCutscenePos;

    [Header("UI Elements")]
    public Image vignetteImage;
    
    [Header("Dialogue")]
    public string druidDialogueKey = "DruidPrologueEnd";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // This is called by Player when HP hits 0 and if it is Prologue
    public void StartPrologueEnding()
    {
        AudioManager.Instance?.PlayAmbianceMusic();
        
        StartCoroutine(PrologueSequence());
    }

    IEnumerator PrologueSequence()
    {
        // 1. Freeze player
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(0.1f);
        
        // 2. Despawn Vagabond, Spawn Druid
        vagabond.SetActive(false);
        mage.SetActive(true);

        // 3. Move Player and Druid to their new cutscene spots
        player.transform.position = playerCutscenePos.position;
        mage.transform.position = druidCutscenePos.position;
        
        // 4. Reposition Camera
        Vector3 centerPoint = (playerCutscenePos.position + druidCutscenePos.position) / 2;
        mainCamera.transform.position = new Vector3(centerPoint.x, centerPoint.y, mainCamera.transform.position.z);
        mainCamera.orthographicSize = 4; 
        
        // 5. Show Vignette
        vignetteImage.gameObject.SetActive(true);
        
        // 6. Start the final dialogue immediately
        DialogueManager.SimplePopUp(mage.transform, druidDialogueKey, OnPrologueDialogueComplete);
    }
    
    void OnPrologueDialogueComplete()
    {
        vignetteImage.gameObject.SetActive(false);
        SceneManager.LoadScene("Cyberpunk");
    }
    
    public void StartVagabondIntro(Vagabond vagabondScript)
    {
        // 1. Pause Player
        Player playerScript = player.GetComponent<Player>();
        playerScript.enabled = false; // Stops player input
        playerScript.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; // Stops sliding
        playerScript.GetComponent<Animator>().SetFloat("Move", 0); // Stops walk anim

        // 2. Pause Vagabond
        vagabondScript.PauseForCutscene();

        // 3. Show Vignette
        vignetteImage.gameObject.SetActive(true);

        // 4. Start Dialogue
        DialogueManager.SimplePopUp(vagabondScript.transform, vagabondScript.introDialogue, () => {
            
            // 5. Hide Vignette
            vignetteImage.gameObject.SetActive(false);
        
            // 6. Unpause Player
            playerScript.enabled = true;
        
            // 7. Unpause Vagabond
            vagabondScript.UnpauseFromCutscene();
        });
    }
   public void StartSamuraiBossCinematic()
    {
        StartCoroutine(SamuraiBossIntroSequence());
    }

    IEnumerator SamuraiBossIntroSequence()
    {
        Debug.Log("=== SAMURAI BOSS CINEMATIC STARTED ===");
        
        // 1. Get references
        Player playerScript = player.GetComponent<Player>();
        Samurai samuraiScript = samurai.GetComponent<Samurai>();

        // 2. Freeze Player
        playerScript.enabled = false;
        playerScript.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        playerScript.GetComponent<Animator>().SetFloat("Move", 0);

        Debug.Log("=== WAITING FOR SAMURAI TO FALL ===");
        
        // 3. Wait for Samurai to fall and land
        // Check if Samurai's vertical velocity is near zero (landed)
        Rigidbody2D samuraiRb = samuraiScript.GetComponent<Rigidbody2D>();
        yield return new WaitForSeconds(0.5f); // Give him time to start falling
        
        // Wait until he stops falling (velocity near zero)
        while (Mathf.Abs(samuraiRb.linearVelocity.y) > 0.1f)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        // Extra moment for impact
        yield return new WaitForSeconds(0.3f);

        Debug.Log("=== SAMURAI LANDED! FREEZING ===");
        
        // 4. Freeze Samurai after landing
        samuraiScript.FreezeForCinematic();

        // 5. Show Vignette
        vignetteImage.gameObject.SetActive(true);

        Debug.Log("=== STARTING DIALOGUE ===");
        
        // 6. Play Boss Intro Dialogue
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

        // 7. Hide Vignette
        vignetteImage.gameObject.SetActive(false);

        AudioManager.Instance?.PlayBossFightMusic();

        // Small dramatic pause
        yield return new WaitForSeconds(0.5f);

        // 9. Unfreeze Player
        playerScript.enabled = true;

        // 10. Unfreeze Samurai and Start Fight!
        samuraiScript.UnfreezeAndStartFight();

        Debug.Log("<color=cyan>=== BOSS FIGHT STARTED ===</color>");
    }

    // Optional: Call this when Samurai is defeated to stop music
    public void OnSamuraiDefeated()
    {
        
        AudioManager.Instance?.StopMusic(true); // Fade out boss music
        
        if (audioSource != null && audioSource.isPlaying)
        {
            StartCoroutine(FadeOutMusic(2f));
        }
    }

    IEnumerator FadeOutMusic(float duration)
    {
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

}