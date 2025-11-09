using System.Collections;
using UnityEngine;

public class SickleTank : MonoBehaviour
{
    [Header("Stats (Tank)")]
    public int maxHealth = 25;
    public int damage = 1;
    public int currentHealth;

    [Header("Behavior (Simple)")]
    public float moveSpeed = 2f;
    public float patrolRadius = 5f;
    public float detectionRange = 10f; // Raycast range
    public float attackRange = 1.5f;
    public float attackCooldown = 2.0f;

    [Header("Annoying")]
    public float knockbackForce = 5f; // Pushes player

    [Header("References")]
    public LayerMask playerLayer;
    public LayerMask groundLayer; // For line-of-sight

    // --- References ---
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // --- State ---
    private bool isAttacking = false;
    private bool isDead = false;
    private Vector2 startPosition;
    private Vector2 patrolTargetPosition;
    private float nextAttackTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        startPosition = transform.position;
        SetNewPatrolTarget();
        
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void Update()
    {
        // If we are dead, or "busy" attacking, or have no player, do nothing.
        if (isDead || isAttacking || player == null) return;

        // --- Main Logic ---
        if (CanSeePlayer())
        {
            // Player is visible, run the "Chase & Attack" logic
            ChaseAndAttackLogic();
        }
        else
        {
            // Player is not visible, go back to patrolling
            PatrolLogic();
        }
    }

    bool CanSeePlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 1. Check if player is even in our detection range
        if (distanceToPlayer > detectionRange)
        {
            return false;
        }

        // 2. Fire the Raycast to check for line of sight (checks for Ground or Player)
        Vector2 direction = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distanceToPlayer, playerLayer | groundLayer);
        
        Debug.DrawRay(transform.position, direction * distanceToPlayer, Color.yellow);
        
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("Sickle_Tank: I see the player!", gameObject);
                return true;
            }
        }
        
        // We hit nothing, or we hit a wall first
        return false;
    }

    void PatrolLogic()
    {
        animator.SetBool("IsWalking", true);
        
        // 1. Move towards patrol target
        float direction = Mathf.Sign(patrolTargetPosition.x - transform.position.x);
        Flip(direction);
        rb.linearVelocity = new Vector2(direction * moveSpeed * 0.5f, rb.linearVelocity.y); // Slower patrol

        // 2. Check if we reached the target, and get a new one
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

    void ChaseAndAttackLogic()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Check if we are close enough to attack
        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            // Close enough, and cooldown is over.
            StartCoroutine(AttackSequence());
        }
        else if (distanceToPlayer > attackRange)
        {
            // Too far away. Run towards the player.
            animator.SetBool("IsWalking", true);
            float direction = Mathf.Sign(player.position.x - transform.position.x);
            Flip(direction);
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            // We are in range, but on cooldown. Just stand still and look.
            animator.SetBool("IsWalking", false);
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            FacePlayer();
        }
    }

    IEnumerator AttackSequence()
    {
        Debug.Log("<color=cyan>Sickle_Tank: Attacking!</color>", gameObject);
        
        // 1. LOCK the Sickle.
        isAttacking = true; 
        nextAttackTime = Time.time + attackCooldown;
        
        // 2. Stop moving and face the player
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        FacePlayer();
        
        // 3. Play the animation
        animator.SetTrigger("Attack");
        
        yield return null;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        Debug.Log($"Sickle_Tank: Took {damageAmount} damage.", gameObject);
        currentHealth -= damageAmount;
        StartCoroutine(FlashRedEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Sickle_Tank: Is Dying.", gameObject);
        isDead = true;
        StartCoroutine(FadeOutEffect());
        
        if (EchoManager.Instance != null)
        {
            EchoManager.Instance.OnEnemyKilled(gameObject);
        }
        
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        rb.gravityScale = 0;
    }

    IEnumerator FlashRedEffect()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = Color.white;
    }

    IEnumerator FadeOutEffect()
    {
        float fadeDuration = 1.5f;
        float timer = 0f;
        Color startColor = spriteRenderer.color;

        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
    
    void FacePlayer()
    {
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        Flip(direction);
    }

    void Flip(float direction)
    {
        if (direction == 0) return;
        Vector3 currentScale = transform.localScale;
        currentScale.x = Mathf.Abs(currentScale.x) * direction;
        transform.localScale = currentScale;
    }

    // --- ANIMATION EVENTS ---
    public void DealDamageAndKnockback()
    {
        // 1. Find the player (they might have moved)
        float direction = Mathf.Sign(transform.localScale.x);
        Vector2 attackCheckPos = (Vector2)transform.position + new Vector2(direction * 0.5f, 0.5f);
        
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackCheckPos, 0.5f, playerLayer);

        if (hitPlayer != null)
        {
            Debug.Log("<color=green>Sickle_Tank: Attack HIT Player!</color>", gameObject);
            
            // 2. Deal damage
            hitPlayer.GetComponent<Player>().TakeDamage(damage);
            hitPlayer.GetComponent<Player>().RegisterHitByEnemy(gameObject);
            
            // 3. Apply Knockback
            Rigidbody2D playerRb = hitPlayer.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 knockDir = (hitPlayer.transform.position - transform.position).normalized;
                knockDir.y = 0.5f; // Add a little "pop-up"
                playerRb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
    
    public void OnAttackAnimationComplete()
    {
        Debug.Log("Sickle_Tank: Attack animation finished.", gameObject);
        isAttacking = false; // UNLOCK the Sickle
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(startPosition, patrolRadius);
    }
}