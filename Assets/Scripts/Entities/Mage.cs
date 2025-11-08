using System.Collections;
using UnityEngine;

public class Mage : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    
    public Player player;
    public string dialogueKey = "Mage";

    private bool isInteracting = false;
    
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
        
        if (hit.collider != null && Input.GetKeyDown(KeyCode.E) && !isInteracting)
        {
            isInteracting = true;
            
            GiveMission();
        }
    }

    private void GiveMission()
    {
        TriggerDialogue();
    }
    
    public void TriggerDialogue()
    {
        DialogueManager.SimplePopUp(this.transform, dialogueKey, StartTeleport);
    }

    public void StartTeleport()
    {
        animator.SetTrigger("Teleport");
    }

    public void HideMage()
    {
        gameObject.SetActive(false);
    }
}