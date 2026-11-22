using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleEnd : MonoBehaviour
{
    [SerializeField] private BattleConfig config;
    [SerializeField] private float lootSpread = 0.8f;
    [SerializeField] private float lootDistance = 1.2f;
    [SerializeField] private float lootFanAngle = 35f;
    [SerializeField] private float lootClearance = 0.3f;

    private const int LootDirections = 8;

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
        if (enemy.EncounterData == null)
            return;

        List<ItemPickup> loot = RollLoot(enemy.EncounterData);
        Transform parent = enemy.transform.parent;
        Transform hero = null;

        if (BattleContext.Inventory != null)
            hero = BattleContext.Inventory.transform;

        for (int i = 0; i < loot.Count; i++)
        {
            Vector3 position;

            if (hero != null)
            {
                position = FindLootPosition(hero, enemy.transform, i, loot.Count);
            }
            else
            {
                float angle = i * Mathf.PI * 2f / loot.Count;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * lootSpread;
                position = enemy.transform.position + offset;
            }

            ItemPickup item = Instantiate(loot[i], position, Quaternion.identity, parent);

            if (parent == null)
                SceneManager.MoveGameObjectToScene(item.gameObject, enemy.gameObject.scene);
        }
    }

    private List<ItemPickup> RollLoot(EncounterData data)
    {
        List<ItemPickup> result = new List<ItemPickup>();

        if (data.Loot != null)
        {
            foreach (ItemPickup item in data.Loot)
            {
                if (item != null)
                    result.Add(item);
            }
        }

        if (data.RandomLoot != null)
        {
            foreach (EncounterData.LootChance entry in data.RandomLoot)
            {
                if (entry.item != null && Random.value < entry.chance)
                    result.Add(entry.item);
            }
        }

        return result;
    }

    private Vector3 FindLootPosition(Transform hero, Transform enemy, int index, int count)
    {
        Vector3 forward = Vector3.ProjectOnPlane(hero.forward, Vector3.up).normalized;
        float startAngle = (index - (count - 1) * 0.5f) * lootFanAngle;

        for (int i = 0; i < LootDirections; i++)
        {
            float angle = startAngle + i * 360f / LootDirections;
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * forward;
            Vector3 point = hero.position + direction * lootDistance;

            if (IsLootPlaceFree(hero, enemy, point))
                return point;
        }

        return hero.position;
    }

    private bool IsLootPlaceFree(Transform hero, Transform enemy, Vector3 point)
    {
        Vector3 lift = Vector3.up * (lootClearance + 0.05f);
        Vector3 center = point + lift;
        Collider[] hits = Physics.OverlapSphere(center, lootClearance, ~0, QueryTriggerInteraction.Ignore);

        foreach (Collider hit in hits)
        {
            if (IsIgnored(hit, hero, enemy))
                continue;

            return false;
        }

        Vector3 from = hero.position + lift;
        RaycastHit[] lineHits = Physics.RaycastAll(from, center - from, Vector3.Distance(from, center), ~0, QueryTriggerInteraction.Ignore);

        foreach (RaycastHit lineHit in lineHits)
        {
            if (IsIgnored(lineHit.collider, hero, enemy))
                continue;

            return false;
        }

        return true;
    }

    private bool IsIgnored(Collider hit, Transform hero, Transform enemy)
    {
        if (hit.gameObject.scene != hero.gameObject.scene)
            return true;

        if (hit.transform.IsChildOf(hero))
            return true;

        return hit.transform.IsChildOf(enemy);
    }
}
