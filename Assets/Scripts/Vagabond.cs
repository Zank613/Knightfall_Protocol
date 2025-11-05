using System.Collections;
using UnityEngine;

public class Vagabond : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    
    [Header("Dialogue Keys")]
    public string introDialogue = "VagabondIntro";
    public string mockeryDialogue = "VagabondMockery";
    public string finisherDialogue = "VagabondFinisher";

    [Header("References")]
    public Transform player;
    
    
    private Animator animator;
    private Rigidbody2D rb;
    
    private bool isAttacking = false;
    private bool hasTriggeredIntro = false;
    private float nextAttackTime = 0f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 1. Intro Dialogue Trigger
        if (!hasTriggeredIntro && distanceToPlayer < 8f)
        {
            hasTriggeredIntro = true;
            DialogueManager.SimplePopUp(transform, introDialogue, null);
        }

        // 2. Chase and Attack Logic
        if (distanceToPlayer > attackRange && !isAttacking)
        {
            ChasePlayer();
        }
        else if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            StartCoroutine(PerformAttack());
        }
        else
        {
            // Idle state between attacks
            animator.SetBool("IsWalking", false);
        }
    }

    private void ChasePlayer()
    {
        animator.SetBool("IsWalking", true);
        
        // Determine direction
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        
        // Move towards player
        transform.position += new Vector3(direction * moveSpeed * Time.deltaTime, 0, 0);
        
        // Face the player
        transform.localScale = new Vector3(direction, 1, 1);
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        animator.SetBool("IsWalking", false);
        animator.SetTrigger("Attack");
        
        // Wait a moment for the animation to reach the "hit" frame
        yield return new WaitForSeconds(0.5f); 

        // Check if player is still in range to be hit
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            TriggerPrologueEnding();
        }

        // Cooldown
        nextAttackTime = Time.time + attackCooldown;
        isAttacking = false;
    }
    
    public void TakeDamage(int damage)
    {
        // He takes NO damage. Instead, he mocks the player.
        DialogueManager.SimplePopUp(transform, mockeryDialogue, null);
    }

    private void TriggerPrologueEnding()
    {
        // 1. Disable further Vagabond action
        this.enabled = false; 
        animator.SetBool("IsWalking", false);
        
        DialogueManager.SimplePopUp(transform, finisherDialogue, () => {
            
            Debug.Log("PROLOGUE ENDED! Transition to Cyberpunk scene here.");
            
            // TODO: SceneManager.LoadScene("CyberpunkLevel1");
        });
        
    }
    
    /*
    
    // Visualizing the attack range in the Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    */
}