using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    #region Health & Lives
    [Header("Health")]
    public int maxHealth = 15;
    public int currentHealth = 15;
    
    [Header("Lives")]
    public int lives = 9;
    public bool isPrologue = true;
    
    private bool isDead = false;
    private GameObject lastEnemyThatHitMe; // Track who killed the Player
    #endregion

    #region Movement
    [Header("Movement")]
    public float speed = 5f;
    public float jumpHeight = 2f;
    
    private bool jumping = false;
    private Rigidbody2D rb;
    #endregion

    #region Combat
    [Header("Attack")]
    public int damage = 2;
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public float attackKnockbackForce = 8f;
    
    private bool isAttacking = false;
    private float lastAttackTime = 0f;
    private float attackCooldown = 0.3f;
    #endregion

    #region Echo System
    [Header("Echo")]
    public bool isUsingEcho = false;
    public float echoTimeRemaining = 5f;
    #endregion

    #region UI & References
    [Header("UI")]
    public Image healthFiller;
    public Text healthFillerText;
    public UIManager uiManager;
    
    private Animator animator;
    private Vector2 startPosition;
    private Vector2 lastCheckpoint;
    #endregion

    #region Unity Lifecycle
    void Start()
    {
        InitializeComponents();
        InitializeHealth();
        InitializeUI();
    }

    void Update()
    {
        if (isDead || lives <= 0) return;

        // Reset attack state each frame (safety fallback if animation event fails)
        isAttacking = false;

        HandleMovement();
        HandleJump();
        HandleAttack();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        jumping = false;
        
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Boss"))
        {
            // Track which enemy hit us (for death scene)
            lastEnemyThatHitMe = other.gameObject;
            
            TakeDamage(5);
        }
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        lastCheckpoint = startPosition;
    }

    private void InitializeHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    private void InitializeUI()
    {
        if (isPrologue)
        {
            uiManager.HideLivesUI();
        }
        else
        {
            uiManager.UpdateLives(lives);
        }
    }
    #endregion

    #region Movement & Input
    private void HandleMovement()
    {
        float move = Input.GetAxis("Horizontal");
        
        // Move player
        Vector2 position = transform.position;
        position.x += speed * Time.deltaTime * move;
        transform.position = position;
        
        // Update animation
        UpdateAnimation(move);
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !jumping)
        {
            float jumpForce = Mathf.Sqrt(-2 * Physics2D.gravity.y * jumpHeight);
            rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            jumping = true;
        }
    }

    private void HandleAttack()
    {
        if (!jumping && !isAttacking && Input.GetKeyDown(KeyCode.F))
        {
            Attack();
        }
    }

    private void UpdateAnimation(float move)
    {
        animator.SetFloat("Move", move);
        
        if (move > 0)
            animator.SetInteger("Direction", 1);
        else if (move < 0)
            animator.SetInteger("Direction", -1);
    }
    #endregion

    #region Combat System
    private void Attack()
    {
        // Trigger correct attack animation based on facing direction
        string attackTrigger = animator.GetInteger("Direction") == 1 ? "Attack_Right" : "Attack_Left";
        animator.SetTrigger(attackTrigger);
        isAttacking = true;
    }

    /// <summary>
    /// Called by Animation Event. Detects and damages enemies in range.
    /// </summary>
    public void DealDamageToEnemies()
    {
        // Prevent multiple hits in same attack
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        Debug.Log("<color=yellow>ATTACK EVENT FIRED!</color>");

        // Find all colliders in attack range
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

        foreach (Collider2D hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log($"<color=green>Player hit: {hit.name}</color>");
                
                // Deal damage
                hit.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

                // Apply knockback
                ApplyKnockback(hit);
            }
        }
    }

    private void ApplyKnockback(Collider2D enemy)
    {
        Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
        if (enemyRb == null) return;

        Vector2 knockDir = (enemy.transform.position - transform.position).normalized;
        knockDir.y += 0.3f; // Add upward pop
        enemyRb.AddForce(knockDir * attackKnockbackForce, ForceMode2D.Impulse);
    }

    /// <summary>
    /// Called by Animation Event on the last frame of attack animation.
    /// </summary>
    public void OnAttackAnimationComplete()
    {
        isAttacking = false;
    }
    #endregion

    #region Health & Death System
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log($"Player took {damageAmount} damage. {currentHealth} HP remaining.");
        
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (isPrologue)
        {
            HandlePrologueDeath();
        }
        else
        {
            HandleFutureDeath();
        }
    }

    private void HandlePrologueDeath()
    {
        Debug.Log("Player has died in prologue. Starting cutscene.");
        
        if (CinematicController.Instance != null)
        {
            CinematicController.Instance.StartPrologueEnding();
        }
        
        this.enabled = false;
    }

    private void HandleFutureDeath()
    {
        Debug.Log("Player has died in the Future.");
        
        lives--;
        uiManager.UpdateLives(lives);

        if (lives <= 3)
        {
            GameOver();
        }
        else
        {
            Debug.Log($"Respawning... {lives} lives remaining.");
            Respawn();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over! No lives left.");
        
        // Register death and load death scene
        if (lastEnemyThatHitMe != null)
        {
            DeathData.RegisterDeath(lastEnemyThatHitMe, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        
        DeathData.LoadDeathScene();
        
        this.enabled = false;
    }

    private void Respawn()
    {
        transform.position = lastCheckpoint;
        currentHealth = maxHealth;
        UpdateHealthUI();
        isDead = false;
    }

    private void UpdateHealthUI()
    {
        healthFiller.fillAmount = (float)currentHealth / maxHealth;
        healthFillerText.text = $"{currentHealth}/{maxHealth}";
    }
    #endregion

    #region Public Utility Methods
    public void SetCheckpoint(Vector2 newCheckpoint)
    {
        lastCheckpoint = newCheckpoint;
        Debug.Log($"Checkpoint set at {newCheckpoint}");
    }

    public bool IsAlive()
    {
        return !isDead && lives > 0;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
        Debug.Log($"Player healed {amount} HP. Current HP: {currentHealth}");
    }

    /// <summary>
    /// Called by enemies when they hit the player
    /// </summary>
    public void RegisterHitByEnemy(GameObject enemy)
    {
        lastEnemyThatHitMe = enemy;
    }
    #endregion

    #region Debug
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
    #endregion
    
    public void PlayFootstepSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPlayerFootstep();
        }
    }
}