using System.Collections;
using UnityEngine;

public class Cyberknight : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 10;
    public int damage = 3;
    private int currentHealth;

    [Header("Behavior")]
    public float moveSpeed = 3f;
    public float patrolRadius = 6f;
    public float detectionRange = 8f;
    public float attackRange = 1.2f;
    public float attackCooldown = 2f;

    [Header("Counter Dash")]
    public float dashSpeed = 15f;
    public float dashDistance = 3f;
    public float dashCooldown = 1f;
    private float lastDashTime = 0f;
    private bool isDashing = false;

    [Header("Dialogue")]
    public string introDialogueKey = "CyberknightIntro";
    public bool hasTriggeredDialogue = false;

    [Header("References")]
    public Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

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
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        // Stop all actions if dead, dashing, attacking, or no player
        if (isDead || isDashing || isAttacking || player == null)
        {
            if (isAttacking || isDashing)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                animator.SetBool("IsRunning", false);
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
        animator.SetBool("IsRunning", true);
        
        // Move towards patrol target (always facing right since sprites face right)
        float direction = Mathf.Sign(patrolTargetPosition.x - transform.position.x);
        FlipToDirection(direction);
        rb.linearVelocity = new Vector2(direction * moveSpeed * 0.5f, rb.linearVelocity.y);

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
        animator.SetBool("IsRunning", true);
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        FlipToDirection(direction);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    void StopMoving()
    {
        animator.SetBool("IsRunning", false);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        // Face the player
        FlipToDirection(Mathf.Sign(player.position.x - transform.position.x));
    }

    void FlipToDirection(float direction)
    {
        if (direction == 0) return;
        
        // Since sprites face right, flip sprite renderer when going left
        spriteRenderer.flipX = direction < 0;
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        StopMoving();
        animator.SetTrigger("Attack");
        
        yield return null;
    }

    // --- DAMAGE & DEATH ---

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // Check if player is attacking from the front
        bool isPlayerInFront = IsPlayerInFront();

        if (isPlayerInFront && Time.time >= lastDashTime + dashCooldown)
        {
            // COUNTER DASH! Dash behind player
            Debug.Log($"<color=cyan>Cyberknight: Counter Dash activated!</color>");
            StartCoroutine(PerformCounterDash());
        }
        else if (!isPlayerInFront)
        {
            // Attacked from behind! Take damage
            Debug.Log($"<color=green>Cyberknight: Hit from behind! Taking {damage} damage.</color>");
            currentHealth -= damage;
            StartCoroutine(FlashRedEffect());

            if (currentHealth <= 0)
            {
                Die();
            }
        }
        else
        {
            Debug.Log($"<color=yellow>Cyberknight: Dash on cooldown, taking hit!</color>");
            // Dash is on cooldown, so take the hit
            currentHealth -= damage;
            StartCoroutine(FlashRedEffect());

            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    bool IsPlayerInFront()
    {
        if (player == null) return false;

        // Check which direction Cyberknight is facing
        bool facingRight = !spriteRenderer.flipX;
        
        // Check if player is on the side the knight is facing
        if (facingRight)
        {
            return player.position.x > transform.position.x;
        }
        else
        {
            return player.position.x < transform.position.x;
        }
    }

    IEnumerator PerformCounterDash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        // Calculate dash position (behind the player)
        float playerDirection = Mathf.Sign(player.position.x - transform.position.x);
        Vector2 dashTarget = (Vector2)player.position + new Vector2(-playerDirection * dashDistance, 0);

        // Dash animation/movement
        float dashTime = 0.2f;
        float elapsed = 0f;
        Vector2 startPos = transform.position;

        while (elapsed < dashTime)
        {
            transform.position = Vector2.Lerp(startPos, dashTarget, elapsed / dashTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = dashTarget;

        // Face the player after dash
        FlipToDirection(Mathf.Sign(player.position.x - transform.position.x));

        isDashing = false;
    }

    void Die()
    {
        isDead = true;
        animator.SetTrigger("Death");
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        
        // Notify EchoManager that this enemy died
        /*
        if (EchoManager.Instance != null)
        {
            EchoManager.Instance.OnEnemyKilled(gameObject);
        }
        */
    }

    IEnumerator FlashRedEffect()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = Color.white;
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
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Patrol radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startPosition, patrolRadius);
    }
}