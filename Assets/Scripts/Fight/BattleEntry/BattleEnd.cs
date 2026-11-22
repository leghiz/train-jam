using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleEnd : MonoBehaviour
{
    [SerializeField] private BattleConfig config;
    [SerializeField] private float lootSpread = 0.8f;

    private PlayerStats battlePlayerStats;
    private bool hadEnemies;
    private bool isEnding;

    private void Update()
    {
        if (isEnding)
            return;

        if (battlePlayerStats == null)
            FindBattlePlayer();

        if (CountEnemies() > 0)
        {
            hadEnemies = true;
            return;
        }

        if (hadEnemies)
            StartCoroutine(EndRoutine(true));
    }

    private void OnDisable()
    {
        if (battlePlayerStats != null)
            battlePlayerStats.Died -= OnPlayerDied;
    }

    private void FindBattlePlayer()
    {
        PlayerStats[] players = FindObjectsByType<PlayerStats>();

        foreach (PlayerStats player in players)
        {
            if (player.gameObject.scene != gameObject.scene)
                continue;

            battlePlayerStats = player;
            battlePlayerStats.Died += OnPlayerDied;
            return;
        }
    }

    private int CountEnemies()
    {
        EnemyArmor[] enemies = FindObjectsByType<EnemyArmor>();
        int count = 0;

        foreach (EnemyArmor enemy in enemies)
        {
            if (enemy.gameObject.scene == gameObject.scene)
                count++;
        }

        return count;
    }

    private void OnPlayerDied()
    {
        if (isEnding)
            return;

        StartCoroutine(EndRoutine(false));
    }

    private IEnumerator EndRoutine(bool isVictory)
    {
        isEnding = true;

        if (TurnManager.Instance != null)
            TurnManager.Instance.StopAllCoroutines();

        yield return new WaitForSeconds(config.endDelay);

        if (isVictory)
            RemoveDefeatedEnemy();

        if (BattleManager.Instance != null)
            BattleManager.Instance.EndBattle();
    }

    private void RemoveDefeatedEnemy()
    {
        EnemyEncounter enemy = BattleContext.CurrentEnemy;

        if (enemy == null)
            return;

        SpawnLoot(enemy);
        Destroy(enemy.gameObject);
    }

    private void SpawnLoot(EnemyEncounter enemy)
    {
        if (enemy.EncounterData == null || enemy.EncounterData.Loot == null)
            return;

        ItemPickup[] loot = enemy.EncounterData.Loot;
        Transform parent = enemy.transform.parent;

        for (int i = 0; i < loot.Length; i++)
        {
            if (loot[i] == null)
                continue;

            float angle = i * Mathf.PI * 2f / loot.Length;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * lootSpread;

            ItemPickup item = Instantiate(loot[i], enemy.transform.position + offset, Quaternion.identity, parent);

            if (parent == null)
                SceneManager.MoveGameObjectToScene(item.gameObject, enemy.gameObject.scene);
        }
    }
}
