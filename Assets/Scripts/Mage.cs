using System.Collections;
using UnityEngine;

public class Mage : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    public int teleportTime = 5;

    public Player player;

    public float direction;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 7f,
            LayerMask.GetMask("Player"));

        direction = player.transform.position.x;

        if (direction > 0)
        {
            animator.SetInteger("Direction", 1);
        }
        else
        {
            animator.SetInteger("Direction", 0);
        }

        if (hit.collider != null && Input.GetKeyDown(KeyCode.E))
        {
            GiveMission();
            StartCoroutine(WaitCoroutine(teleportTime));
            animator.SetTrigger("Teleport");
            Destroy(this.gameObject);
        }
    }

    private void GiveMission()
    {
        
    }

    IEnumerator WaitCoroutine(int time)
    {
        yield return new WaitForSecondsRealtime(time);
    }
}
