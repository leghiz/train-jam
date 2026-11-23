using UnityEngine;

[CreateAssetMenu(fileName = "BattleConfig", menuName = "TrainJam/Battle Config")]
public class BattleConfig : ScriptableObject
{
    [Header("Ход")]
    public float playerTurnDuration = 5f;
    public float enemyAimDelay = 0.8f;
    public float enemyAfterFireDelay = 0.8f;

    [Header("Очередь ходов")]
    public float playerTurnChance = 0.35f;
    public int maxConsecutiveEnemyTurns = 3;
    public int maxConsecutivePlayerTurns = 1;
    public int turnQueueLength = 5;

    [Header("Стрельба")]
    public float fireRate = 1f;
    public float armorPiercingChance = 0.5f;
    public int ammoQueueLength = 3;
    public int playerDamage = 20;
    public int enemyDamage = 20;

    [Header("Урон по телу")]
    public float headMultiplier = 1.2f;
    public float bodyMultiplier = 1f;
    public float legsMultiplier = 0.7f;
    public float armorPiercingBodyMultiplier = 0.1f;

    [Header("Урон по броне")]
    public float armorPiercingArmorMultiplier = 1f;
    public float normalArmorMultiplier = 0.4f;

    [Header("Конец боя")]
    public float endDelay = 1f;

    [Header("Звуки")]
    public AudioClip shotSound;
    [Range(0f, 1f)] public float shotVolume = 0.7f;
    public AudioClip shellSound;
    [Range(0f, 1f)] public float shellVolume = 0.5f;
    public float shellDelay = 0.35f;
    public AudioClip reloadSound;
    [Range(0f, 1f)] public float reloadVolume = 0.6f;
    public AudioClip armorHitSound;
    public AudioClip bodyHitSound;
    [Range(0f, 1f)] public float hitVolume = 0.8f;

    [Header("Эффекты выстрела")]
    public GameObject shotSmokePrefab;
    public float shotShake = 0.55f;
    public float shotKick = 1f;
    public float enemyShotShake = 0.2f;
    public float playerHitShake = 0.6f;
}
