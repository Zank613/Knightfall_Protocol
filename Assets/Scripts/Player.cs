using System;
using UnityEngine;

public class Player : MonoBehaviour
{
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
}
