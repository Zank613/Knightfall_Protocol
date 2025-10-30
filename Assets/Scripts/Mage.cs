using System.Collections;
using UnityEngine;

public class Mage : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    public int teleportTime = 5;

    public Player player;
    
    public string dialogueKey = "Mage";
    public GameObject dialogueBoxPrefab;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        float directionToPlayer = player.transform.position.x - transform.position.x;

        // Control Facing Direction
        if (directionToPlayer > 0.1f) // Player is to the right
        {
            // Face Right
            animator.SetInteger("Direction", 1);
        }
        else if (directionToPlayer < -0.1f) // Player is to the left
        {
            // Face Left
            animator.SetInteger("Direction", -1);
        }

        // Raycast Logic
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position, 
            new Vector2(Mathf.Sign(directionToPlayer), 0),
            7f,
            LayerMask.GetMask("Player")
        );
        
        if (hit.collider != null && Input.GetKeyDown(KeyCode.E))
        {
            GiveMission();
            StartCoroutine(WaitCoroutine(teleportTime));
            animator.SetTrigger("Teleport");
            Destroy(this.gameObject);
        }
    }

    private void GiveMission()
    {
        TriggerDialogue();
    }

    IEnumerator WaitCoroutine(int time)
    {
        yield return new WaitForSecondsRealtime(time);
    }
    
    public void TriggerDialogue()
    {
        // 1. Get the random line using the key
        string line = DialogueManager.Instance.GetRandomLine(dialogueKey);

        // 2. Instantiate and Initialize the Dialogue Box
        GameObject dialogueBoxInstance = Instantiate(dialogueBoxPrefab, transform.position, Quaternion.identity);
        DialogueBoxController controller = dialogueBoxInstance.GetComponent<DialogueBoxController>();
    
        if (controller != null)
        { 
            controller.Initialize(this.transform, line);
        }
    }
}
