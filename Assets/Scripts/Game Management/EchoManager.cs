using UnityEngine;

public class EchoManager : MonoBehaviour
{
    public static EchoManager Instance;

    #region Echo Settings
    [Header("Echo Pickup")]
    public GameObject echoPickupPrefab;
    [Range(0f, 1f)] public float dropChance = 0.3f; // 30% chance to drop
    
    [Header("Echo Ghost")]
    public GameObject echoGhostPrefab;
    
    [Header("Player Reference")]
    public Player player;
    #endregion

    #region Echo State
    private GameObject lastKilledEnemy;
    private Vector3 lastKilledEnemyPosition;
    private bool hasEchoCharge = false;
    #endregion

    #region Echo Power Levels (Based on Lives Lost)
    [System.Serializable]
    public class EchoPowerLevel
    {
        public int lives;
        public int ghostCount;
        public float ghostSpeed;
        public float stunDuration;
        public int damagePerHit;
        public float duration;
    }

    [Header("Echo Power Scaling")]
    public EchoPowerLevel[] powerLevels = new EchoPowerLevel[]
    {
        new EchoPowerLevel { lives = 9, ghostCount = 1, ghostSpeed = 3f, stunDuration = 1.5f, damagePerHit = 0, duration = 10f },
        new EchoPowerLevel { lives = 8, ghostCount = 1, ghostSpeed = 4f, stunDuration = 1.5f, damagePerHit = 0, duration = 10f },
        new EchoPowerLevel { lives = 7, ghostCount = 1, ghostSpeed = 4f, stunDuration = 2f, damagePerHit = 0, duration = 10f },
        new EchoPowerLevel { lives = 6, ghostCount = 1, ghostSpeed = 4f, stunDuration = 2f, damagePerHit = 1, duration = 10f },
        new EchoPowerLevel { lives = 5, ghostCount = 2, ghostSpeed = 4f, stunDuration = 2f, damagePerHit = 1, duration = 12f },
        new EchoPowerLevel { lives = 4, ghostCount = 2, ghostSpeed = 5f, stunDuration = 2.5f, damagePerHit = 1, duration = 12f },
        new EchoPowerLevel { lives = 3, ghostCount = 3, ghostSpeed = 5f, stunDuration = 2.5f, damagePerHit = 2, duration = 15f },
        new EchoPowerLevel { lives = 2, ghostCount = 3, ghostSpeed = 6f, stunDuration = 3f, damagePerHit = 2, duration = 15f },
        new EchoPowerLevel { lives = 1, ghostCount = 4, ghostSpeed = 6f, stunDuration = 3.5f, damagePerHit = 3, duration = 20f }
    };
    #endregion

    #region Initialization
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }
    }

    void Update()
    {
        // Press E to activate Echo
        if (Input.GetKeyDown(KeyCode.E) && hasEchoCharge)
        {
            ActivateEcho();
        }
    }
    #endregion

    #region Enemy Death Handler
    /// <summary>
    /// Called when any enemy dies. Chance to spawn Echo pickup.
    /// </summary>
    public void OnEnemyKilled(GameObject enemy)
    {
        lastKilledEnemy = enemy;
        lastKilledEnemyPosition = enemy.transform.position;

        Debug.Log($"<color=cyan>Echo: Enemy killed at {lastKilledEnemyPosition}</color>");

        // Random chance to drop Echo pickup
        if (Random.value <= dropChance)
        {
            SpawnEchoPickup(lastKilledEnemyPosition);
        }
    }
    #endregion

    #region Echo Pickup System
    void SpawnEchoPickup(Vector3 position)
    {
        if (echoPickupPrefab == null)
        {
            Debug.LogWarning("Echo: No pickup prefab assigned!");
            return;
        }

        GameObject pickup = Instantiate(echoPickupPrefab, position, Quaternion.identity);
        Debug.Log("<color=green>Echo: Pickup spawned!</color>");
    }

    /// <summary>
    /// Called by EchoPickup when player collects it
    /// </summary>
    public void CollectEcho()
    {
        hasEchoCharge = true;
        Debug.Log("<color=yellow>Echo: Charge collected! Press E to activate.</color>");
        
        // TODO: Show UI indicator that Echo is ready
    }
    #endregion

    #region Echo Activation
    void ActivateEcho()
    {
        if (!hasEchoCharge) return;

        hasEchoCharge = false;

        // Get current power level based on player lives
        EchoPowerLevel power = GetCurrentPowerLevel();

        Debug.Log($"<color=magenta>=== ECHO ACTIVATED ===</color>");
        Debug.Log($"<color=magenta>Lives: {player.lives} | Ghosts: {power.ghostCount} | Damage: {power.damagePerHit}</color>");

        // Spawn ghosts at last killed enemy position
        for (int i = 0; i < power.ghostCount; i++)
        {
            SpawnEchoGhost(power, i);
        }

        AudioManager.Instance?.PlaySFX(null); // TODO: Add Echo activation sound
    }

    void SpawnEchoGhost(EchoPowerLevel power, int index)
    {
        if (echoGhostPrefab == null)
        {
            Debug.LogError("Echo: No ghost prefab assigned!");
            return;
        }

        // Spawn position with slight offset for multiple ghosts
        Vector3 spawnPos = lastKilledEnemyPosition + new Vector3(index * 0.5f, 0, 0);
        
        GameObject ghostObj = Instantiate(echoGhostPrefab, spawnPos, Quaternion.identity);
        EchoGhost ghost = ghostObj.GetComponent<EchoGhost>();

        if (ghost != null)
        {
            ghost.Initialize(player.transform, power);
        }

        Debug.Log($"<color=cyan>Echo Ghost #{index + 1} spawned!</color>");
    }
    #endregion

    #region Power Level System
    EchoPowerLevel GetCurrentPowerLevel()
    {
        // Find the power level that matches current lives
        foreach (var level in powerLevels)
        {
            if (player.lives >= level.lives)
            {
                return level;
            }
        }

        // If lives somehow go below 1, return the strongest level
        return powerLevels[powerLevels.Length - 1];
    }

    /// <summary>
    /// Get info about current Echo power for UI display
    /// </summary>
    public string GetEchoPowerInfo()
    {
        EchoPowerLevel power = GetCurrentPowerLevel();
        return $"Ghosts: {power.ghostCount} | Speed: {power.ghostSpeed} | Stun: {power.stunDuration}s | Damage: {power.damagePerHit}";
    }
    #endregion

    #region Public Accessors
    public bool HasEchoCharge()
    {
        return hasEchoCharge;
    }
    #endregion
}