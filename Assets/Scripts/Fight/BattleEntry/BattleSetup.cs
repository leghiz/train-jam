
using UnityEngine;

public class BattleSetup : MonoBehaviour
{
    [Header("Battle Player")]
    [SerializeField] private GameObject battlePlayerPrefab;
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Enemy Spawn Points")]
    [SerializeField] private Transform[] enemySpawnPoints;

    private GameObject spawnedPlayer;
    private PlayerStats battlePlayerStats;

    public void Setup(PlayerStats originalPlayerStats)
    {
        if (!BattleContext.HasEncounter)
        {
            return;
        }

        SpawnPlayer(originalPlayerStats);
        SpawnEnemies();
    }

    private void SpawnPlayer(PlayerStats originalPlayerStats)
    {
        if (battlePlayerPrefab == null)
        {
            return;
        }

        if (playerSpawnPoint == null)
        {
            return;
        }

        if (originalPlayerStats == null)
        {
            return;
        }

        spawnedPlayer = Instantiate(
            battlePlayerPrefab,
            playerSpawnPoint.position,
            playerSpawnPoint.rotation
        );

        battlePlayerStats = spawnedPlayer.GetComponent<PlayerStats>();

        if (battlePlayerStats == null)
            return;

        battlePlayerStats.CopyFrom(originalPlayerStats);
    }

    private void SpawnEnemies()
    {
        GameObject[] enemies =
            BattleContext.CurrentEncounter.BattleEnemyPrefabs;

        if (enemies == null || enemies.Length == 0)
        {
            return;
        }

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] == null)
            {
                continue;
            }

            if (enemySpawnPoints == null ||
                i >= enemySpawnPoints.Length ||
                enemySpawnPoints[i] == null)
            {
                continue;
            }

            Instantiate(
                enemies[i],
                enemySpawnPoints[i].position,
                enemySpawnPoints[i].rotation
            );
        }
    }

    public void CopyPlayerStatsTo(PlayerStats originalPlayerStats)
    {
        if (battlePlayerStats == null)
        {
            return;
        }

        originalPlayerStats.CopyFrom(battlePlayerStats);
    }
}
