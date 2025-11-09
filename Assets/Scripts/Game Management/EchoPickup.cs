using System.Collections;
using UnityEngine;

public class EchoPickup : MonoBehaviour
{
    [Header("Visual Settings")]
    public float floatSpeed = 1f;
    public float floatHeight = 0.3f;
    public float rotationSpeed = 90f;
    public Color glowColor = new Color(0f, 1f, 1f, 0.7f); // Cyan glow
    
    [Header("Lifetime")]
    public float lifetime = 15f; // Disappears after 15 seconds
    public float blinkStartTime = 12f; // Starts blinking at 12 seconds
    
    private Vector3 startPosition;
    private float spawnTime;
    private SpriteRenderer spriteRenderer;
    
    void Start()
    {
        startPosition = transform.position;
        spawnTime = Time.time;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = glowColor;
        }
        
        // Start fade out coroutine
        StartCoroutine(LifetimeCoroutine());
    }
    
    void Update()
    {
        // Floating animation
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // Rotation animation
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
    
    IEnumerator LifetimeCoroutine()
    {
        // Wait until blink time
        yield return new WaitForSeconds(blinkStartTime);
        
        // Blink effect
        float blinkDuration = lifetime - blinkStartTime;
        float elapsed = 0f;
        
        while (elapsed < blinkDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            
            yield return new WaitForSeconds(0.2f);
            elapsed += 0.2f;
        }
        
        // Destroy after lifetime
        Destroy(gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Collect the Echo charge
            if (EchoManager.Instance != null)
            {
                EchoManager.Instance.CollectEcho();
            }
            
            // Play collection sound
            AudioManager.Instance?.PlaySFX(null); // TODO: Add echo pickup sound
            
            // Destroy pickup
            Destroy(gameObject);
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}