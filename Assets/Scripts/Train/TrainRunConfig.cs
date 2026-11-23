using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TrainRunConfig", menuName = "TrainJam/Train Run Config")]
public class TrainRunConfig : ScriptableObject
{
    [Header("Вагоны")]
    public WagonEntry startWagon = new WagonEntry();

    public WagonEntry[] storyWagons = Array.Empty<WagonEntry>();

    public WagonEntry[] randomWagons = Array.Empty<WagonEntry>();

    public WagonEntry endWagon = new WagonEntry();

    [Min(0)] public int randomBeforeStory = 2;

    [Min(0)] public int randomAfterStory = 2;

    [Header("Расстановка")]
    [Min(0f)] public float spacing = 0.3f;

    [Range(0f, 1f)] public float surfaceLootChance = 0.6f;

    [Header("Лут")]
    public List<LootEntry> loot = new List<LootEntry>();

    [Min(0f)] public float minNoteDistance = 3.5f;

    [Header("Враги")]
    public List<EnemyEntry> enemies = new List<EnemyEntry>();

    public GameObject keyHolderPrefab;

    [Min(0f)] public float minEnemyDistance = 5f;

    [Min(0f)] public float minEntryDistance = 6f;

    [Header("Оружие")]
    public WeaponSpawn[] weaponSpawns = Array.Empty<WeaponSpawn>();

    private void OnValidate()
    {
        startWagon?.Validate();
        endWagon?.Validate();
        foreach (var w in storyWagons) w?.Validate();
        foreach (var w in randomWagons) w?.Validate();
    }
}

[Serializable]
public class WeaponSpawn
{
    public WeaponData weapon;
    public bool countFromEnd;
    [Min(0)] public int wagonIndex;
}

[Serializable]
public class WagonEntry
{
    public WagonLayout wagon;

    public Vector2Int enemyCount;

    public bool hasKeyHolder;

    public Vector2Int lootCount;

    public void Validate()
    {
        enemyCount = ClampRange(enemyCount);
        lootCount = ClampRange(lootCount);
    }

    private static Vector2Int ClampRange(Vector2Int r)
    {
        r.x = Mathf.Max(0, r.x);
        r.y = Mathf.Max(r.x, r.y);
        return r;
    }
}

public enum LootPlacement
{
    Anywhere,
    FloorOnly,
    SurfaceOnly
}

[Serializable]
public class LootEntry
{
    public GameObject prefab;

    [Min(0)] public float weight = 1f;

    public LootPlacement placement = LootPlacement.Anywhere;

    [Min(0)] public int maxPerWagon;
}

[Serializable]
public class EnemyEntry
{
    public GameObject prefab;

    [Min(0)] public float weight = 1f;

    [Min(0)] public int minWagonIndex;
}
