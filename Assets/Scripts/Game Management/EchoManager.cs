using UnityEngine;
using UnityEngine.UI;

public class EchoManager : MonoBehaviour
{
    public static EchoManager Instance;

    [Header("Echo Pickup")]
    public GameObject echoPickupPrefab;
    [Range(0f, 1f)] public float dropChance = 0.3f;
    
    [Header("Echo Ghost")]
    public GameObject echoGhostPrefab;
    
    [Header("Player Reference")]
    public Player player;

    [Header("UI")]
    public Text echoChargeText;
    public Image echoIcon;

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

    private GameObject lastKilledEnemy;
    private Vector3 lastKilledEnemyPosition;
    private Sprite lastKilledEnemySprite;
    private bool lastKilledEnemyFlipX;
    private int echoCharges = 0;

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

        UpdateEchoUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && echoCharges > 0)
        {
            ActivateEcho();
        }
    }

    public void OnEnemyKilled(GameObject enemy)
    {
        lastKilledEnemy = enemy;
        lastKilledEnemyPosition = enemy.transform.position;

        SpriteRenderer enemySprite = enemy.GetComponent<SpriteRenderer>();
        if (enemySprite != null)
        {
            lastKilledEnemySprite = enemySprite.sprite;
            lastKilledEnemyFlipX = enemySprite.flipX;
            Debug.Log($"<color=cyan>Echo: Captured sprite from {enemy.name}</color>");
        }

        Debug.Log($"<color=cyan>Echo: Enemy killed at {lastKilledEnemyPosition}</color>");

        if (Random.value <= dropChance)
        {
            SpawnEchoPickup(lastKilledEnemyPosition);
        }
    }

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

    public void CollectEcho()
    {
        echoCharges++;
        UpdateEchoUI();
        Debug.Log($"<color=yellow>Echo: Charge collected! Total charges: {echoCharges}. Press G to activate.</color>");
        
        if (AudioManager.Instance != null)
        {
            // AudioManager.Instance.PlaySFX(echoCollectSound);
        }
    }

    void ActivateEcho()
    {
        if (echoCharges <= 0) return;

        echoCharges--;
        UpdateEchoUI();

        EchoPowerLevel power = GetCurrentPowerLevel();

        Debug.Log($"<color=magenta>=== ECHO ACTIVATED ===</color>");
        Debug.Log($"<color=magenta>Lives: {player.lives} | Ghosts: {power.ghostCount} | Damage: {power.damagePerHit}</color>");
        Debug.Log($"<color=magenta>Charges remaining: {echoCharges}</color>");

        for (int i = 0; i < power.ghostCount; i++)
        {
            SpawnEchoGhost(power, i);
        }

        if (AudioManager.Instance != null)
        {
            // AudioManager.Instance.PlaySFX(echoActivateSound);
        }
    }

    void SpawnEchoGhost(EchoPowerLevel power, int index)
    {
        if (echoGhostPrefab == null)
        {
            Debug.LogError("Echo: No ghost prefab assigned!");
            return;
        }

        Vector3 spawnPos = lastKilledEnemyPosition + new Vector3(index * 0.5f, 0, 0);
        
        GameObject ghostObj = Instantiate(echoGhostPrefab, spawnPos, Quaternion.identity);
        EchoGhost ghost = ghostObj.GetComponent<EchoGhost>();

        if (ghost != null)
        {
            ghost.Initialize(player.transform, power, lastKilledEnemySprite, lastKilledEnemyFlipX);
        }

        Debug.Log($"<color=cyan>Echo Ghost #{index + 1} spawned with sprite!</color>");
    }

    EchoPowerLevel GetCurrentPowerLevel()
    {
        foreach (var level in powerLevels)
        {
            if (player.lives >= level.lives)
            {
                return level;
            }
        }

        return powerLevels[powerLevels.Length - 1];
    }

    void UpdateEchoUI()
    {
        if (echoChargeText != null)
        {
            echoChargeText.text = echoCharges.ToString();
        }

        if (echoIcon != null)
        {
            if (echoCharges > 0)
            {
                echoIcon.color = Color.white;
            }
            else
            {
                echoIcon.color = new Color(1f, 1f, 1f, 0.3f);
            }
        }
    }

    public bool HasEchoCharge()
    {
        return echoCharges > 0;
    }

    public int GetEchoCharges()
    {
        return echoCharges;
    }

    public string GetEchoPowerInfo()
    {
        EchoPowerLevel power = GetCurrentPowerLevel();
        return $"Ghosts: {power.ghostCount} | Speed: {power.ghostSpeed} | Stun: {power.stunDuration}s | Damage: {power.damagePerHit}";
    }
}