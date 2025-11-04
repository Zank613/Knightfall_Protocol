using System.Collections;
using UnityEngine;

public class Mage : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    public int teleportTime = 5;
    public bool isTeleporting = false;
    public Player player;
    
    public string dialogueKey = "Mage";
    
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
            
            isTeleporting = true;
            
            StartCoroutine(WaitAndDestroy(teleportTime));

            if (isTeleporting)
            {
                animator.SetTrigger("Teleport");   
            }
        }
    }

    private void GiveMission()
    {
        TriggerDialogue();
    }
    
    IEnumerator WaitAndDestroy(int time) 
    {
        yield return new WaitForSecondsRealtime(time);
        
        Destroy(this.gameObject); 
    }
    
    public void TriggerDialogue()
    {
        DialogueManager.SimplePopUp(this.transform, dialogueKey);
    }
}