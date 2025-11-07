using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    [Header("Health")]
    public int maxHealth = 15;
    public int currentHealth = 15;
    public bool isPrologue = true;

    private bool isDead = false;

    [Header("Movement")]
    public float speed;
    public float jumpHeight;
    public bool jumping = false;

    private Rigidbody2D rb;
    private Animator animator;

    [Header("Echo")]
    public bool isUsingEcho = false;
    public float echoTimeRemaining = 5f;

    private Vector2 startPosition;
    private Vector2 lastCheckpoint;
    
    [Header("Lives")]
    public int lives = 9;

    [Header("Attack")]
    public bool isAttacking = false;
    public int damage = 2;

    [Header("UI")]
    public Image healthFiller;
    public Text healthFillerText;
    public UIManager uiManager;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        lastCheckpoint = startPosition;
        
        currentHealth = maxHealth; 
        UpdateHealthUI();
        
        if (isPrologue)
        {
            // If we are in the prologue, tell the UI Manager to hide all star images.
            uiManager.HideLivesUI();
        }
        else
        {
            // If we are in the main level, show the correct star image.
            uiManager.UpdateLives(lives);
        }
    }
    void Update()
    {
        if (isDead || lives == 0) return;
        
        // Get movement input
        float move = Input.GetAxis("Horizontal");
        Vector2 position = transform.position;

        // Movement
        position.x = position.x + (speed * Time.deltaTime * move);
        transform.position = position;

        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && !jumping)
        {
            rb.AddForce(new Vector2(0, Mathf.Sqrt(-2 * Physics2D.gravity.y * jumpHeight)), ForceMode2D.Impulse);
            jumping = true;
        }

        if (!jumping && Input.GetKeyDown(KeyCode.F))
        {
            Attack();
        }

        UpdateAnimation(move);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        jumping = false;
        if (other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Boss"))
        {
            TakeDamage(5);
        }
    }

    private void UpdateAnimation(float move)
    {
        animator.SetFloat("Move", move);
        if (move > 0)
        {
            animator.SetInteger("Direction", 1);
        }
        else if (move < 0)
        {
            animator.SetInteger("Direction", -1);
        }
    }

    private void Attack()
    {
        if (animator.GetInteger("Direction").Equals(1))
        {
            animator.SetTrigger("Attack_Right");
        }
        else
        {
            animator.SetTrigger("Attack_Left");
        }

        isAttacking = true;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Player took {damage} damage. {currentHealth} HP remaining.");
        
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
            Debug.Log("Player has died in prologue. Starting cutscene.");

            if (CinematicController.Instance != null)
            {
                CinematicController.Instance.StartPrologueEnding();
            }

            this.enabled = false;
        }
        else
        {
            Debug.Log("Player has died in the Future.");
            lives--;

            uiManager.UpdateLives(lives);
            
            if (lives <= 0)
            {
                Debug.Log("Game Over! No lives left.");
                // TODO: Add game over screen or logic here.
                this.enabled = false; // Stop the player script
            }
            else
            {
                Debug.Log($"Respawning... {lives} lives remaining.");
                Respawn();
            }
        }
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
        healthFillerText.text = currentHealth + "/" + maxHealth;
    }
}