/*
 *
 * DEPRECATED DO NOT USE.
 *
 * This is old Sickle behavior script.
 * 
 */



using System.Collections;
using UnityEngine;

public class Sickle : MonoBehaviour
{
    private enum State
    {
        Idle_OnGround,
        Jumping_ToPlatform,
        Tracking_OnPlatform,
        Dropping,
        Attacking_OnImpact,
        Cooldown_OnGround
    }

    [Header("Stats")]
    public int maxHealth = 10;
    public int damage = 10;
    private int currentHealth;

    [Header("Behavior")]
    public float detectionRange = 10f;
    public float trackSpeed = 3f;
    public float attackCooldown = 3f;
    
    [Header("Jump To Platform")]
    public float jumpArcHeight = 3f;
    public float jumpDuration = 1.5f;
    
    [Header("Attack Arc")]
    [Tooltip("Gravity multiplier for the 'drop' part of the attack")]
    public float fastDropGravityMultiplier = 3f;
    [Tooltip("How far in front to check for an edge")]
    public float edgeRaycastDistance = 0.5f;

    [Header("Dialogue")]
    public string attackDialogueKey = "SickleAttack";

    [Header("References")]
    public Transform[] highPoints; 
    public LayerMask playerLayer;
    public LayerMask groundLayer;

    // --- References ---
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private float originalGravityScale;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    
    // --- State ---
    private State currentState;
    private Transform targetHighPoint;
    private float stateTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        originalGravityScale = rb.gravityScale;
        
        SetState(State.Idle_OnGround);

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        
        if (highPoints.Length == 0)
        {
            Debug.LogError("SICKLE ERROR: No 'highPoints' assigned!", gameObject);
        }
    }

    void Update()
    {
        if (isDead || player == null || highPoints.Length == 0) return;

        switch (currentState)
        {
            case State.Idle_OnGround:
                IdleLogic();
                break;
            case State.Tracking_OnPlatform:
                TrackingLogic();
                break;
            case State.Cooldown_OnGround:
                CooldownLogic();
                break;
        }
    }

    private void SetState(State newState)
    {
        Debug.Log($"<color=cyan>Sickle state changed to: {newState}</color>", gameObject);
        
        currentState = newState;
        stateTimer = 0f; 

        switch (currentState)
        {
            case State.Idle_OnGround:
                animator.SetBool("IsWalking", false);
                rb.isKinematic = false;
                col.enabled = true;
                rb.gravityScale = originalGravityScale;
                break;

            case State.Jumping_ToPlatform:
                animator.SetTrigger("Jump");
                // Turn off physics and collision completely for the arc
                rb.isKinematic = true; 
                col.enabled = false;
                
                StartCoroutine(JumpToHighPointCoroutine());
                break;
                
            case State.Tracking_OnPlatform:
                animator.SetBool("IsWalking", true);
                
                rb.isKinematic = false; 
                col.enabled = true;
                rb.gravityScale = originalGravityScale; 
                break;

            case State.Dropping:
                Debug.Log("<color=red>Sickle: Dropping to attack!</color>", gameObject);
                rb.isKinematic = false;
                col.enabled = true;
                rb.gravityScale = originalGravityScale * fastDropGravityMultiplier;
                break;
                
            case State.Attacking_OnImpact:
                Debug.Log("<color=green>Sickle: HIT PLAYER! Playing attack...</color>", gameObject);
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = originalGravityScale; // Normal gravity
                animator.SetTrigger("Attack");
                player.GetComponent<Player>().TakeDamage(damage);
                // An animation event will call OnAttackAnimationComplete()
                break;
                
            case State.Cooldown_OnGround:
                animator.SetBool("IsWalking", false);
                rb.isKinematic = false;
                col.enabled = true;
                rb.gravityScale = originalGravityScale;
                break;
        }
    }

    // --- STATE LOGIC ---

    void IdleLogic()
    {
        if (Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            Debug.Log("Sickle: Player in range, finding high point...", gameObject);
            FindClosestHighPoint();
            if (targetHighPoint != null)
            {
                SetState(State.Jumping_ToPlatform);
            }
        }
    }

    IEnumerator JumpToHighPointCoroutine()
    {
        Debug.Log($"Sickle: Jumping to {targetHighPoint.name}.", gameObject);
        
        Vector2 startPos = transform.position;
        Vector2 targetPos = targetHighPoint.position;
        float timer = 0f;

        while (timer < jumpDuration)
        {
            float t = timer / jumpDuration;
            float arc = Mathf.Sin(t * Mathf.PI) * jumpArcHeight;
            transform.position = Vector2.Lerp(startPos, targetPos, t) + new Vector2(0, arc);

            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Sickle: Jump finished, landing on platform.", gameObject);
        transform.position = targetPos; // Snap to final position
        
        SetState(State.Tracking_OnPlatform); // We've landed, now we track.
    }
    
    void TrackingLogic()
    {
        // 1. Follow Player
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * trackSpeed, rb.linearVelocity.y);
        Flip(direction);
        
        // Check if we are still on the ground
        RaycastHit2D groundCheckBelow = Physics2D.Raycast(transform.position, Vector2.down, 1.5f, groundLayer);
        if (groundCheckBelow.collider == null)
        {
            Debug.Log("<color=orange>Sickle: I fell off! Going to cooldown.</color>", gameObject);
            SetState(State.Cooldown_OnGround);
            return; // Stop running this logic
        }

        // 2. Check for Player Below
        RaycastHit2D playerCheck = Physics2D.Raycast(transform.position, Vector2.down, 50f, playerLayer);
        bool isPlayerBelow = (playerCheck.collider != null);
        
        // 3. Check for Edge
        Vector2 edgeCheckPos = (Vector2)transform.position + new Vector2(direction * edgeRaycastDistance, 0);
        RaycastHit2D groundCheckEdge = Physics2D.Raycast(edgeCheckPos, Vector2.down, 1.5f, groundLayer);
        bool isNearEdge = (groundCheckEdge.collider == null);
        
        Debug.DrawRay(edgeCheckPos, Vector2.down * 1.5f, (isNearEdge ? Color.red : Color.green));
        
        // 4. Attack Condition
        if (isPlayerBelow && isNearEdge)
        {
            Debug.Log("<color=green>Sickle: Player is below AND I'm near an edge. Dropping!</color>", gameObject);
            DialogueManager.SimplePopUp(transform, attackDialogueKey, null);
            SetState(State.Dropping);
        }
    }

    void CooldownLogic()
    {
        stateTimer += Time.deltaTime;
        if (stateTimer > attackCooldown)
        {
            Debug.Log("Sickle: Cooldown finished, returning to Idle.", gameObject);
            SetState(State.Idle_OnGround);
        }
    }

    // --- COLLISION ---
    
    void OnCollisionEnter2D(Collision2D other)
    {
        if (currentState != State.Dropping) return;
        
        if (other.gameObject.CompareTag("Player"))
        {
            // HIT THE PLAYER! Set state to Attacking.
            SetState(State.Attacking_OnImpact);
        }
        else
        {
            // Hit the ground (miss)
            Debug.Log($"<color=orange>Sickle: ATTACK HIT {other.gameObject.name} (MISS)</color>", gameObject);
            rb.linearVelocity = Vector2.zero;
            SetState(State.Cooldown_OnGround);
        }
    }
    
    // --- ANIMATION EVENT ---
    
    public void OnAttackAnimationComplete()
    {
        Debug.Log("Sickle: Attack animation finished. Going to cooldown.", gameObject);
        SetState(State.Cooldown_OnGround);
    }
    

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        Debug.Log($"Sickle: Took {damageAmount} damage.", gameObject);
        currentHealth -= damageAmount;
        StartCoroutine(FlashRedEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Sickle: Is Dying.", gameObject);
        isDead = true;
        StartCoroutine(FadeOutEffect());
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
    
    void FindClosestHighPoint()
    {
        float minDistance = Mathf.Infinity;
        Transform closestPoint = null;

        foreach (Transform point in highPoints)
        {
            float dist = Vector2.Distance(player.position, point.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestPoint = point;
            }
        }
        targetHighPoint = closestPoint;
        if(targetHighPoint != null)
        {
            Debug.Log($"Sickle: Found closest high point: {targetHighPoint.name}", gameObject);
        }
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}