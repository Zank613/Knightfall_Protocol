using System;
using TreeEditor;
using UnityEngine;

public class Player : MonoBehaviour
{

    [Header("Health")] 
    public int currentHealth = 15;
    public bool isPrologue = true;

    private bool isDead = false;
    
    public float speed;
    public float jumpHeight;

    public bool jumping = false;

    private Rigidbody2D rb;
    private Animator animator;

    private bool isPowerUp = false;
    private float powerUpTimeRemaining = 5;
    public float defaultPowerUpTime = 5;

    private Vector2 startPosition;
    private Vector2 lastCheckpoint;

    private int lives = 9;

    public bool isAttacking = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        lastCheckpoint = startPosition;
    }
    void Update()
    {
        if (isDead && lives == 0) return;
        // TODO: Add game over screen.
        
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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BossTrigger"))
        {
            
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

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
    }

    private void Die()
    {
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
            Debug.Log("Player has died in the Future. Starting Knightfall Protocol.");
        }
    }
}
