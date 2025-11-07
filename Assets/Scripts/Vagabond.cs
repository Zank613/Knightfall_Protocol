using System.Collections;
using UnityEngine;

public class Vagabond : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float attackCooldown = 3f;

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
    
    private bool isInCutscene = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

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
            if (CinematicController.Instance != null)
            {
                CinematicController.Instance.StartVagabondIntro(this); 
            }
        }
        
        // 2. Stop all logic if in a cutscene
        if (isInCutscene)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("IsWalking", false);
            return;
        } 

        // 3. Chase and Attack Logic
        if (isAttacking)
        {
            // This forces him to stop moving.
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("IsWalking", false);
        }
        else
        {
            if (distanceToPlayer > attackRange)
            {
                ChasePlayer();
            }
            else if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
            {
                // This will set isAttacking = true
                StartCoroutine(PerformAttack());
            }
            else if (distanceToPlayer <= attackRange)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                animator.SetBool("IsWalking", false);
            }
        }
    }
    
    private void ChasePlayer()
    {
        animator.SetBool("IsWalking", true);

        float direction = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        
        Vector3 currentScale = transform.localScale;
        currentScale.x = Mathf.Abs(currentScale.x) * direction;
        transform.localScale = currentScale;
    }
    
    IEnumerator PerformAttack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        animator.SetTrigger("Attack");
        
        // Animation events will handle damage and set isAttacking = false
        yield return null; 
    }
    
    public void DealDamageToPlayer()
    {
        if (player == null) return;
                    
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(5); // Deal 5 damage per hit
            }
        }
    }
    
    public void OnAttackAnimationComplete()
    {
        isAttacking = false;
    }

    public void TakeDamage(int damage)
    {
        DialogueManager.SimplePopUp(transform, mockeryDialogue, null);
    }
    
    public void PauseForCutscene()
    {
        isInCutscene = true;
        animator.SetBool("IsWalking", false);
        rb.linearVelocity = Vector2.zero; // Stop him from sliding
    }
    
    public void UnpauseFromCutscene()
    {
        isInCutscene = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}