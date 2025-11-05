using UnityEngine;

public class SpawnTrigger : MonoBehaviour
{
    public GameObject[] enemiesToSpawn;
    
    public bool destroyAfterTrigger = true;

    private bool hasTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check for Player tag and ensure it only happens once
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;
            SpawnAllEntities();
        }
    }

    void SpawnAllEntities()
    {
        foreach (GameObject enemy in enemiesToSpawn)
        {
            if (enemy != null)
            {
                enemy.SetActive(true);
            }
        }
        
        if (destroyAfterTrigger)
        {
            Destroy(gameObject);
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}