using System.Collections;
using UnityEngine;

public class Vagabond : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float attackCooldown = 3f;
    public float attackDuration = 1f;

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
    private float attackStartTime = 0f; 
    
    private bool isInCutscene = false;
    private bool isInvincible = true;

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
        
        if (isAttacking && Time.time - attackStartTime > attackDuration)
        {
            Debug.Log("<color=yellow>Vagabond: Attack timeout, force reset</color>");
            isAttacking = false;
        }

        // 3. Chase and Attack Logic
        if (isAttacking)
        {
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
        attackStartTime = Time.time;
        nextAttackTime = Time.time + attackCooldown;
        
        // Make sure we're facing the player
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        Vector3 currentScale = transform.localScale;
        currentScale.x = Mathf.Abs(currentScale.x) * direction;
        transform.localScale = currentScale;
        
        // Stop moving
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("IsWalking", false);
        
        // Trigger attack
        animator.SetTrigger("Attack");
        
        Debug.Log("<color=cyan>Vagabond: Attack triggered</color>");
        
        yield return new WaitForSeconds(attackDuration);
        
        if (isAttacking) // If still attacking after timeout
        {
            Debug.Log("<color=yellow>Vagabond: Force completing attack (timeout)</color>");
            isAttacking = false;
        }
    }
    
    public void DealDamageToPlayer()
    {
        if (player == null) return;
                    
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            Player playerScript = player.GetComponent<Player>();
            if (playerScript != null)
            {
                playerScript.RegisterHitByEnemy(gameObject);
                playerScript.TakeDamage(5);
            }
        }
    }
    
    public void OnAttackAnimationComplete()
    {
        isAttacking = false;
        Debug.Log("Vagabond: Attack complete");
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible)
        {
            // Just mock the player
            DialogueManager.SimplePopUp(transform, mockeryDialogue, null);
            return;
        }
        
        // Vagabond does not exist in other scenes but fun to keep anyway.
        Debug.Log($"Vagabond took {damage} damage");
    }
    
    public void PauseForCutscene()
    {
        isInCutscene = true;
        isAttacking = false;
        animator.SetBool("IsWalking", false);
        rb.linearVelocity = Vector2.zero;
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