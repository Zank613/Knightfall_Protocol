using System.Collections;
using UnityEngine;

public class EchoGhost : MonoBehaviour
{
    #region Configuration
    private Transform player;
    private EchoManager.EchoPowerLevel powerLevel;
    
    [Header("Behavior Settings")]
    public float followDistance = 3f; // Stay this far from player
    public float enemyDetectionRange = 8f;
    public float attackRange = 0.5f;
    
    private enum GhostState { FollowingPlayer, ChasingEnemy, Attacking, Fading }
    private GhostState currentState = GhostState.FollowingPlayer;
    #endregion

    #region Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D ghostCollider;
    #endregion

    #region State
    private Transform targetEnemy;
    private float lifetimeRemaining;
    private bool hasStunnedEnemy = false;
    #endregion

    #region Initialization
    public void Initialize(Transform playerTransform, EchoManager.EchoPowerLevel power)
    {
        player = playerTransform;
        powerLevel = power;
        lifetimeRemaining = power.duration;
        
        SetupComponents();
        StartCoroutine(LifetimeCountdown());
        
        Debug.Log($"<color=cyan>Ghost initialized: Speed={power.ghostSpeed}, Stun={power.stunDuration}s, Damage={power.damagePerHit}</color>");
    }

    void SetupComponents()
    {
        // Setup Rigidbody
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Ghosts float!
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Setup sprite
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color ghostColor = spriteRenderer.color;
            ghostColor.a = 0.5f; // Semi-transparent
            ghostColor = new Color(0.5f, 0.8f, 1f, 0.5f); // Cyan tint
            spriteRenderer.color = ghostColor;
        }

        // Setup collider as trigger
        ghostCollider = GetComponent<Collider2D>();
        if (ghostCollider != null)
        {
            ghostCollider.isTrigger = true;
        }
    }
    #endregion

    #region Update Loop
    void Update()
    {
        if (player == null || currentState == GhostState.Fading) return;

        switch (currentState)
        {
            case GhostState.FollowingPlayer:
                FollowPlayerBehavior();
                break;

            case GhostState.ChasingEnemy:
                ChaseEnemyBehavior();
                break;

            case GhostState.Attacking:
                // Wait for attack to complete
                break;
        }
    }
    #endregion

    #region Follow Player Behavior
    void FollowPlayerBehavior()
    {
        // Check for nearby enemies
        Collider2D nearestEnemy = FindNearestEnemy();
        
        if (nearestEnemy != null)
        {
            // Found enemy! Switch to chase mode
            targetEnemy = nearestEnemy.transform;
            currentState = GhostState.ChasingEnemy;
            Debug.Log($"<color=yellow>Ghost: Found enemy {targetEnemy.name}! Engaging!</color>");
            return;
        }

        // Follow player at distance
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer > followDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * powerLevel.ghostSpeed;
            
            // Face movement direction
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }
        else
        {
            // Close enough, slow down
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, Time.deltaTime * 5f);
        }
    }

    Collider2D FindNearestEnemy()
    {
        // Find all enemies in range
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, enemyDetectionRange);
        
        Collider2D nearest = null;
        float nearestDistance = Mathf.Infinity;

        foreach (Collider2D enemy in enemies)
        {
            if (enemy.CompareTag("Enemy") || enemy.CompareTag("Boss"))
            {
                float distance = Vector2.Distance(transform.position, enemy.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = enemy;
                }
            }
        }

        return nearest;
    }
    #endregion

    #region Chase Enemy Behavior
    void ChaseEnemyBehavior()
    {
        // Check if enemy still exists
        if (targetEnemy == null)
        {
            currentState = GhostState.FollowingPlayer;
            return;
        }

        float distanceToEnemy = Vector2.Distance(transform.position, targetEnemy.position);

        if (distanceToEnemy <= attackRange)
        {
            // In range! Attack!
            StartCoroutine(AttackEnemy());
        }
        else
        {
            // Chase the enemy
            Vector2 direction = (targetEnemy.position - transform.position).normalized;
            rb.linearVelocity = direction * powerLevel.ghostSpeed * 1.5f; // Chase faster!
            
            // Face enemy
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }
    }
    #endregion

    #region Attack Enemy
    IEnumerator AttackEnemy()
    {
        currentState = GhostState.Attacking;
        rb.linearVelocity = Vector2.zero;

        Debug.Log($"<color=green>Ghost: Attacking {targetEnemy.name}!</color>");

        // Stun the enemy
        StunEnemy(targetEnemy);

        // Deal damage if power level allows
        if (powerLevel.damagePerHit > 0)
        {
            targetEnemy.SendMessage("TakeDamage", powerLevel.damagePerHit, SendMessageOptions.DontRequireReceiver);
        }

        // Visual effect - flash
        if (spriteRenderer != null)
        {
            Color original = spriteRenderer.color;
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = original;
        }

        // Wait a moment
        yield return new WaitForSeconds(0.5f);

        // Return to following player
        targetEnemy = null;
        currentState = GhostState.FollowingPlayer;
    }

    void StunEnemy(Transform enemy)
    {
        // Try to get enemy script and disable it temporarily
        MonoBehaviour[] scripts = enemy.GetComponents<MonoBehaviour>();
        
        foreach (MonoBehaviour script in scripts)
        {
            // Check if it's an enemy AI script
            if (script is Crow || script is SickleTank || script is Cyberknight || script is Samurai)
            {
                StartCoroutine(StunScript(script, powerLevel.stunDuration));
                Debug.Log($"<color=orange>Ghost: Stunned {enemy.name} for {powerLevel.stunDuration}s!</color>");
            }
        }
    }

    IEnumerator StunScript(MonoBehaviour script, float duration)
    {
        script.enabled = false;
        
        // Freeze enemy movement
        Rigidbody2D enemyRb = script.GetComponent<Rigidbody2D>();
        if (enemyRb != null)
        {
            enemyRb.linearVelocity = Vector2.zero;
        }

        // Visual stun indicator (optional - change color)
        SpriteRenderer enemySprite = script.GetComponent<SpriteRenderer>();
        Color originalColor = Color.white;
        if (enemySprite != null)
        {
            originalColor = enemySprite.color;
            enemySprite.color = new Color(0.7f, 0.7f, 1f, 1f); // Blue tint
        }

        yield return new WaitForSeconds(duration);

        // Restore
        script.enabled = true;
        if (enemySprite != null)
        {
            enemySprite.color = originalColor;
        }
    }
    #endregion

    #region Lifetime Management
    IEnumerator LifetimeCountdown()
    {
        yield return new WaitForSeconds(powerLevel.duration);
        
        Debug.Log("<color=cyan>Ghost: Duration expired, fading out...</color>");
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        currentState = GhostState.Fading;
        rb.linearVelocity = Vector2.zero;

        float fadeDuration = 1f;
        float elapsed = 0f;
        Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        while (elapsed < fadeDuration)
        {
            if (spriteRenderer != null)
            {
                float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / fadeDuration);
                spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
    #endregion

    #region Debug
    void OnDrawGizmosSelected()
    {
        // Enemy detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyDetectionRange);
        
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Follow distance
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, followDistance);
        }
    }
    #endregion
}