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

}