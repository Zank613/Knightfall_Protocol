using System.Collections;
using UnityEngine;

public class Samurai : MonoBehaviour
{
    #region Stats
    [Header("Boss Stats")]
    public int maxHealth = 50;
    public int currentHealth;
    public int attackDamage = 7;
    
    private bool isDefeated = false;
    private bool isFightStarted = false;
    #endregion

    #region Combat Behavior
    [Header("Combat")]
    public float moveSpeed = 3.5f;
    public float detectionRange = 15f;
    public float attackRange = 2f;
    public float attackCooldown = 2.5f;
    
    [Header("Attack Patterns")]
    public float sideAttackChance = 0.6f; // 60% chance for side attack, 40% for down attack
    public float downAttackRange = 1.5f;
    
    private bool isAttacking = false;
    private float nextAttackTime = 0f;
    #endregion

    #region Dialogue
    [Header("Dialogue Keys")]
    public string introDialogue = "SamuraiIntro";
    public string midFightDialogue = "SamuraiMidFight";
    public string defeatDialogue = "SamuraiDefeat";
    
    private bool hasSpokenMidFight = false;
    #endregion

    #region References
    [Header("References")]
    public Transform player;
    
    private Animator animator;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        InitializeComponents();
        currentHealth = maxHealth;
        
        // Boss starts with physics enabled so he can fall
        // He'll be frozen by the cinematic AFTER landing
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        this.enabled = false; // Disable AI until cinematic starts
    }

    void Update()
    {
        // Don't do anything if defeated or fight hasn't started
        if (isDefeated || !isFightStarted) return;
        
        // Don't move while attacking
        if (isAttacking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("IsWalking", false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Mid-fight dialogue trigger (at 50% health)
        if (!hasSpokenMidFight && currentHealth <= maxHealth / 2)
        {
            TriggerMidFightDialogue();
        }

        // Combat AI
        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            StartCoroutine(PerformAttack());
        }
        else if (distanceToPlayer > attackRange && distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            StopMoving();
        }
        
        // Debug: Show current state
        // Debug.Log($"Samurai State - Attacking: {isAttacking}, Walking: {animator.GetBool("IsWalking")}, Distance: {distanceToPlayer:F2}");
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }
    #endregion

    #region Cinematic Control
    public void FreezeForCinematic()
    {
        isFightStarted = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; // Freeze physics
        animator.SetBool("IsWalking", false);
        this.enabled = false; // Disable Update
    }

    public void UnfreezeAndStartFight()
    {
        isFightStarted = true;
        rb.bodyType = RigidbodyType2D.Dynamic; // Enable physics
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        this.enabled = true; // Enable Update
        
        Debug.Log("<color=cyan>Samurai Boss Fight Started!</color>");
    }
    #endregion

    #region Movement
    private void ChasePlayer()
    {
        animator.SetBool("IsWalking", true);
        
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        Flip(direction);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    public void PlayBossFootstepSound()
    {
        AudioManager.Instance?.PlayBossFootstep();
    }

    private void StopMoving()
    {
        animator.SetBool("IsWalking", false);
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        
        // Still face the player even when idle
        if (player != null)
        {
            Flip(Mathf.Sign(player.position.x - transform.position.x));
        }
    }

    private void Flip(float direction)
    {
        if (direction == 0) return;
        
        spriteRenderer.flipX = direction < 0;
    }
    #endregion

    #region Combat System
    IEnumerator PerformAttack()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;
        StopMoving();

        // Choose attack type based on probability
        bool useSideAttack = Random.value <= sideAttackChance;
        
        AudioManager.Instance?.PlayBossAttack();
        
        if (useSideAttack)
        {
            animator.SetTrigger("AttackSide");
            Debug.Log("<color=yellow>Samurai: Side Attack!</color>");
        }
        else
        {
            animator.SetTrigger("AttackDown");
            Debug.Log("<color=yellow>Samurai: Down Attack!</color>");
        }

        yield return null;
    }

    /// <summary>
    /// Called by Animation Event for Side Attack
    /// </summary>
    public void DealDamageToPlayerSide()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange)
        {
            Debug.Log("<color=red>Samurai Side Attack Hit Player!</color>");
            player.GetComponent<Player>().TakeDamage(attackDamage);
        }
    }

    /// <summary>
    /// Called by Animation Event for Down Attack (slightly shorter range)
    /// </summary>
    public void DealDamageToPlayerDown()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= downAttackRange)
        {
            Debug.Log("<color=red>Samurai Down Attack Hit Player!</color>");
            player.GetComponent<Player>().TakeDamage(attackDamage);
        }
    }

    /// <summary>
    /// Called by Animation Event at the end of attack animations
    /// </summary>
    public void OnAttackAnimationComplete()
    {
        isAttacking = false;
        
        // Reset animator to prevent stuck states
        animator.ResetTrigger("AttackSide");
        animator.ResetTrigger("AttackDown");
    }
    #endregion

    #region Damage & Defeat
    public void TakeDamage(int damage)
    {
        if (isDefeated) return;

        currentHealth -= damage;
        Debug.Log($"<color=cyan>Samurai took {damage} damage! HP: {currentHealth}/{maxHealth}</color>");
        
        AudioManager.Instance?.PlayBossHurt();
        
        // Flash effect
        StartCoroutine(FlashRedEffect());

        if (currentHealth <= 0)
        {
            StartCoroutine(DefeatSequence());
        }
    }

    IEnumerator FlashRedEffect()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = Color.white;
    }

    IEnumerator DefeatSequence()
    {
        isDefeated = true;
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        
        Debug.Log("<color=green>Samurai Defeated!</color>");
        
        AudioManager.Instance?.PlayBossDefeat();
        
        // Play death animation (falling to knees)
        animator.SetTrigger("Death");
        
        // Wait a moment for the animation to play
        yield return new WaitForSeconds(1f);
        
        // Freeze the animation at the kneeling pose
        animator.speed = 0f;
        
        // Show defeat dialogue
        DialogueManager.SimplePopUp(transform, defeatDialogue, OnDefeatDialogueComplete);
    }

    private void OnDefeatDialogueComplete()
    {
        Debug.Log("Samurai boss fight complete!");
        
        // Notify game manager or trigger next event
        if (CinematicController.Instance != null)
        {
            // CinematicController.Instance.OnSamuraiDefeated();
        }
    }
    #endregion

    #region Dialogue Triggers
    private void TriggerMidFightDialogue()
    {
        hasSpokenMidFight = true;
        
        // Pause the fight temporarily
        StartCoroutine(MidFightDialogueSequence());
    }

    IEnumerator MidFightDialogueSequence()
    {
        // Pause movement
        bool wasAttacking = isAttacking;
        isAttacking = true; // Prevents new attacks
        StopMoving();
        
        // Show dialogue
        DialogueManager.SimplePopUp(transform, midFightDialogue, null);
        
        // Wait for dialogue to finish (adjust time as needed)
        yield return new WaitForSeconds(3f);
        
        // Resume fight
        isAttacking = wasAttacking;
    }
    #endregion

    #region Debug
    void OnDrawGizmosSelected()
    {
        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Down attack range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, downAttackRange);
        
        // Detection range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
    #endregion
}