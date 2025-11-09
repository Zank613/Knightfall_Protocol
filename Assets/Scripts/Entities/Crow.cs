using System.Collections;
using UnityEngine;

public class Crow : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 6;
    public int damage = 2;
    private int currentHealth;

    [Header("Behavior")]
    public float moveSpeed = 1.5f;
    public float patrolRadius = 5f;
    public float detectionRange = 7f;
    public float attackRange = 1f;
    public float attackCooldown = 2.5f;

    [Header("Dialogue")]
    public string introDialogueKey = "CrowIntro";
    public bool hasTriggeredDialogue = false;

    [Header("References")]
    public Transform player;
    private Animator animator;
    private Rigidbody2D rb;

    private Vector2 startPosition;
    private Vector2 patrolTargetPosition;
    private bool isPatrolling = true;
    private bool isAttacking = false;
    private bool isDead = false;
    private float nextAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        startPosition = transform.position;
        SetNewPatrolTarget();

        // Find the player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        // Stop all actions if dead, attacking, or no player
        if (isDead || isAttacking || player == null)
        {
            if (isAttacking)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                animator.SetBool("IsWalking", false);
            }
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            // --- Player is in range ---
            isPatrolling = false;

            // Trigger dialogue once
            if (!hasTriggeredDialogue)
            {
                hasTriggeredDialogue = true;
                DialogueManager.SimplePopUp(transform, introDialogueKey, null);
            }

            // Attack State
            if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
            {
                StartCoroutine(PerformAttack());
            }
            // Chase State
            else if (distanceToPlayer > attackRange)
            {
                ChasePlayer();
            }
            // In range, but on cooldown
            else
            {
                StopMoving();
            }
        }
        else
        {
            // --- Player is out of range, resume patrolling ---
            isPatrolling = true;
            Patrol();
        }
    }

    void Patrol()
    {
        animator.SetBool("IsWalking", true);
        
        // Move towards patrol target
        float direction = Mathf.Sign(patrolTargetPosition.x - transform.position.x);
        Flip(direction);
        rb.linearVelocity = new Vector2(direction * moveSpeed * 0.5f, rb.linearVelocity.y); // Patrol slower

        // Check if we reached the target, and get a new one
        if (Vector2.Distance(transform.position, patrolTargetPosition) < 0.5f)
        {
            SetNewPatrolTarget();
        }
    }

    void SetNewPatrolTarget()
    {
        float randomX = Random.Range(startPosition.x - patrolRadius, startPosition.x + patrolRadius);
        patrolTargetPosition = new Vector2(randomX, startPosition.y);
    }

    void ChasePlayer()
    {
        animator.SetBool("IsWalking", true);
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        Flip(direction);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    void StopMoving()
    {
        animator.SetBool("IsWalking", false);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        // Flip to face the player
        Flip(Mathf.Sign(player.position.x - transform.position.x));
    }

    void Flip(float direction)
    {
        if (direction == 0) return;

        Vector3 currentScale = transform.localScale;
        currentScale.x = Mathf.Abs(currentScale.x) * direction;
        transform.localScale = currentScale;
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        StopMoving(); // Stop moving to perform the attack
        
        AudioManager.Instance?.PlayEnemyAttack();
        
        animator.SetTrigger("Attack");
        
        yield return null;
    }

    // --- DAMAGE & DEATH ---

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        animator.SetTrigger("TakeDamage");
        Debug.Log($"Crow taken damage: {damage}");
        
        AudioManager.Instance?.PlayEnemyHurt();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        
        AudioManager.Instance?.PlayEnemyDeath();
        
        animator.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false; // Disable collider so player can walk through
    }
    
    // --- ANIMATION EVENTS ---
    public void DealDamageToPlayer()
    {
        if (player == null) return;
                    
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            player.GetComponent<Player>().TakeDamage(damage);
        }
    }
    
    public void OnAttackAnimationComplete()
    {
        isAttacking = false;
    }

    public void OnDeathAnimationComplete()
    {
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
    
    public void PlayEnemyFootstepSound()
    {
        AudioManager.Instance?.PlayEnemyFootstep();
    }

}