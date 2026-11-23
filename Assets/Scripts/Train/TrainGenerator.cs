using System;
using System.Collections.Generic;
using UnityEngine;

public class TrainGenerator : MonoBehaviour
{
    public TrainRunConfig config;

    [Header("Seed")]
    public bool randomSeed = true;

    public int seed;

    [Header("Запуск")]
    public bool generateOnStart = true;

    public Transform player;

    public event Action<TrainGenerator> Generated;
    public event Action<WagonLayout> WagonEntered;

    public IReadOnlyList<WagonLayout> Wagons => _wagons;
    public IReadOnlyList<GameObject> KeyHolders => _keyHolders;
    public IReadOnlyList<ItemPickup> WeaponPickups => _weaponPickups;
    public int CurrentIndex { get; private set; }
    public WagonLayout CurrentWagon => CurrentIndex >= 0 && CurrentIndex < _wagons.Count ? _wagons[CurrentIndex] : null;
    public bool HasNextWagon => CurrentIndex < _wagons.Count - 1;

    private readonly List<WagonLayout> _wagons = new List<WagonLayout>();
    private readonly List<WagonEntry> _entries = new List<WagonEntry>();
    private readonly List<GameObject> _keyHolders = new List<GameObject>();
    private readonly List<ItemPickup> _weaponPickups = new List<ItemPickup>();
    private readonly List<List<Vector3>> _enemyPositions = new List<List<Vector3>>();
    private readonly List<WagonSpace> _spaces = new List<WagonSpace>();
    private System.Random _rng;

    private void Start()
    {
        if (generateOnStart) Generate();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        if (randomSeed) seed = Guid.NewGuid().GetHashCode();
        Generate(seed);
    }

    public void Generate(int newSeed)
    {
        if (!config) return;

        Clear();
        seed = newSeed;
        _rng = new System.Random(seed);

        foreach (var entry in BuildSequence())
        {
            SpawnWagon(entry);
        }

        foreach (var wagon in _wagons)
        {
            _spaces.Add(new WagonSpace(wagon, config.spacing));
            _enemyPositions.Add(new List<Vector3>());
        }

        SpawnWeapons();
        SpawnEnemies();
        SpawnLoot();
        EnterWagon(0);

#if UNITY_EDITOR
        if (!Application.isPlaying) UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif

        Generated?.Invoke(this);
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        _wagons.Clear();
        _entries.Clear();
        _keyHolders.Clear();
        _weaponPickups.Clear();
        _enemyPositions.Clear();
        _spaces.Clear();
        CurrentIndex = 0;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Discard(transform.GetChild(i).gameObject);
        }
    }

    public bool EnterNextWagon()
    {
        if (!HasNextWagon) return false;
        EnterWagon(CurrentIndex + 1);
        return true;
    }

    public void EnterWagon(int index)
    {
        if (index < 0 || index >= _wagons.Count) return;
        CurrentIndex = index;

        for (int i = 0; i < _wagons.Count; i++)
        {
            _wagons[i].gameObject.SetActive(i == index);
        }

        MovePlayerTo(_wagons[index]);
        WagonEntered?.Invoke(_wagons[index]);
    }

    private List<WagonEntry> BuildSequence()
    {
        var sequence = new List<WagonEntry>();
        if (IsSet(config.startWagon)) sequence.Add(config.startWagon);

        var randoms = PickRandomWagons(config.randomBeforeStory + config.randomAfterStory);
        int before = Mathf.Min(config.randomBeforeStory, randoms.Count);

        sequence.AddRange(randoms.GetRange(0, before));

        var story = new List<WagonEntry>();
        foreach (var entry in config.storyWagons)
        {
            if (IsSet(entry)) story.Add(entry);
        }
        if (story.Count > 0) sequence.Add(story[_rng.Next(story.Count)]);

        sequence.AddRange(randoms.GetRange(before, randoms.Count - before));

        if (IsSet(config.endWagon)) sequence.Add(config.endWagon);

        return sequence;
    }

    private static bool IsSet(WagonEntry entry) => entry != null && entry.wagon;

    private List<WagonEntry> PickRandomWagons(int count)
    {
        var pool = new List<WagonEntry>();
        foreach (var entry in config.randomWagons)
        {
            if (IsSet(entry)) pool.Add(entry);
        }

        var result = new List<WagonEntry>();
        if (pool.Count == 0) return result;

        var bag = new List<WagonEntry>();
        while (result.Count < count)
        {
            if (bag.Count == 0)
            {
                bag.AddRange(pool);
                Shuffle(bag);
                if (result.Count > 0 && bag.Count > 1 && bag[0] == result[result.Count - 1])
                {
                    (bag[0], bag[bag.Count - 1]) = (bag[bag.Count - 1], bag[0]);
                }
            }
            result.Add(bag[0]);
            bag.RemoveAt(0);
        }
        return result;
    }

    private void SpawnWagon(WagonEntry entry)
    {
        var wagon = Instantiate(entry.wagon, transform);
        wagon.name = entry.wagon.name;
        wagon.transform.localPosition = Vector3.zero;
        wagon.transform.localRotation = Quaternion.identity;
        _wagons.Add(wagon);
        _entries.Add(entry);
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < _wagons.Count; i++)
        {
            if (_entries[i].hasKeyHolder) SpawnKeyHolder(i);
        }

        for (int i = 0; i < _wagons.Count; i++)
        {
            var wagon = _wagons[i];
            var allowed = config.enemies.FindAll(e => e.prefab && e.weight > 0f && i >= e.minWagonIndex);
            int wanted = RandomRange(_entries[i].enemyCount);
            if (allowed.Count == 0 || wanted == 0) continue;

            for (int k = 0; k < wanted; k++)
            {
                var entry = PickWeighted(allowed, e => e.weight);
                var enemy = Create(entry.prefab, wagon, FacingEntry(wagon));
                int wagonIndex = i;
                if (_spaces[i].TryPlaceOnFloor(enemy.transform, _rng, p => FarFromEnemies(wagonIndex, p) && FarFromEntry(wagonIndex, p)))
                {
                    _enemyPositions[i].Add(enemy.transform.position);
                }
                else
                {
                    Discard(enemy);
                    break;
                }
            }
        }
    }

    private void SpawnKeyHolder(int wagonIndex)
    {
        if (!config.keyHolderPrefab) return;

        var wagon = _wagons[wagonIndex];
        var holder = Create(config.keyHolderPrefab, wagon, FacingEntry(wagon));
        var space = _spaces[wagonIndex];
        bool placed = space.TryPlaceOnFloor(holder.transform, _rng, p => FarFromEnemies(wagonIndex, p) && FarFromEntry(wagonIndex, p))
                   || space.TryPlaceOnFloor(holder.transform, _rng, p => FarFromEntry(wagonIndex, p))
                   || space.TryPlaceOnFloor(holder.transform, _rng);

        if (placed)
        {
            _keyHolders.Add(holder);
            _enemyPositions[wagonIndex].Add(holder.transform.position);
        }
        else
        {
            Discard(holder);
        }
    }

    private bool FarFromEnemies(int wagonIndex, Vector3 point)
    {
        float min = config.minEnemyDistance;
        if (min <= 0f) return true;
        foreach (var p in _enemyPositions[wagonIndex])
        {
            float dx = p.x - point.x, dz = p.z - point.z;
            if (dx * dx + dz * dz < min * min) return false;
        }
        return true;
    }

    private bool FarFromEntry(int wagonIndex, Vector3 point)
    {
        float min = config.minEntryDistance;
        WagonLayout wagon = _wagons[wagonIndex];
        Transform spawn = wagon.playerSpawn != null ? wagon.playerSpawn : wagon.entry;

        if (min <= 0f || spawn == null)
            return true;

        return FlatDistance(spawn.position, point) >= min;
    }

    private bool FarFromNotes(int wagonIndex, Vector3 point)
    {
        float min = config.minNoteDistance;

        if (min <= 0f)
            return true;

        NoteInteraction[] notes = _wagons[wagonIndex].GetComponentsInChildren<NoteInteraction>(true);

        foreach (NoteInteraction note in notes)
        {
            if (FlatDistance(note.transform.position, point) < min)
                return false;
        }

        return true;
    }

    private static float FlatDistance(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    private void SpawnLoot()
    {
        var loot = config.loot.FindAll(l => l.prefab && l.weight > 0f);
        if (loot.Count == 0) return;

        for (int i = 0; i < _wagons.Count; i++)
        {
            int wagonIndex = i;
            int wanted = RandomRange(_entries[i].lootCount);
            var counts = new Dictionary<LootEntry, int>();

            for (int k = 0; k < wanted; k++)
            {
                var available = loot.FindAll(l => l.maxPerWagon <= 0 || !counts.TryGetValue(l, out var c) || c < l.maxPerWagon);
                if (available.Count == 0) break;

                var entry = PickWeighted(available, l => l.weight);
                counts.TryGetValue(entry, out var n);
                counts[entry] = n + 1;
                TryPlace(entry.prefab, entry.placement, i, p => FarFromNotes(wagonIndex, p));
            }
        }
    }

    private void SpawnWeapons()
    {
        foreach (WeaponSpawn rule in config.weaponSpawns)
        {
            if (rule == null || rule.weapon == null || rule.weapon.worldPrefab == null)
                continue;

            int index;
            if (WeaponProgress.IsUnlocked(rule.weapon))
                index = 0;
            else if (rule.countFromEnd)
                index = _wagons.Count - 1 - rule.wagonIndex;
            else
                index = rule.wagonIndex;

            if (index < 0 || index >= _wagons.Count)
                continue;

            int wagonIndex = index;
            Func<Vector3, bool> awayFromNotes = p => FarFromNotes(wagonIndex, p);

            GameObject obj = TryPlace(rule.weapon.worldPrefab, LootPlacement.SurfaceOnly, index, awayFromNotes);
            if (obj == null)
                obj = TryPlace(rule.weapon.worldPrefab, LootPlacement.FloorOnly, index, awayFromNotes);
            if (obj == null)
                obj = TryPlace(rule.weapon.worldPrefab, LootPlacement.SurfaceOnly, index);
            if (obj == null)
                obj = TryPlace(rule.weapon.worldPrefab, LootPlacement.FloorOnly, index);
            if (obj == null)
                continue;

            ItemPickup pickup = obj.GetComponent<ItemPickup>();
            if (pickup != null)
                _weaponPickups.Add(pickup);
        }
    }

    private GameObject TryPlace(GameObject prefab, LootPlacement placement, int wagonIndex, Func<Vector3, bool> accept = null)
    {
        var space = _spaces[wagonIndex];
        Quaternion randomYaw = Quaternion.Euler(0f, (float)_rng.NextDouble() * 360f, 0f);
        var obj = Create(prefab, _wagons[wagonIndex], randomYaw);
        var t = obj.transform;

        bool placed;
        switch (placement)
        {
            case LootPlacement.FloorOnly:
                placed = space.TryPlaceOnFloor(t, _rng, accept);
                break;
            case LootPlacement.SurfaceOnly:
                placed = space.TryPlaceOnSurface(t, _rng, accept);
                break;
            default:
                bool surfaceFirst = space.HasSurfaces && _rng.NextDouble() < config.surfaceLootChance;
                placed = surfaceFirst
                    ? space.TryPlaceOnSurface(t, _rng, accept) || space.TryPlaceOnFloor(t, _rng, accept)
                    : space.TryPlaceOnFloor(t, _rng, accept) || space.TryPlaceOnSurface(t, _rng, accept);
                break;
        }

        if (placed) return obj;
        Discard(obj);
        return null;
    }

    private void MovePlayerTo(WagonLayout wagon)
    {
        if (!player) return;

        var point = wagon.playerSpawn ? wagon.playerSpawn : wagon.entry;
        if (!point) point = wagon.transform;

        var controller = player.GetComponent<CharacterController>();
        if (controller) controller.enabled = false;

        player.SetPositionAndRotation(point.position, point.rotation);

        var body = player.GetComponent<Rigidbody>();
        if (body)
        {
            body.position = point.position;
            body.rotation = point.rotation;
            if (!body.isKinematic) body.linearVelocity = Vector3.zero;
        }

        if (controller) controller.enabled = true;
    }

    private static GameObject Create(GameObject prefab, WagonLayout wagon, Quaternion localRotation)
    {
        var obj = Instantiate(prefab, wagon.transform);
        obj.name = prefab.name;
        obj.transform.localRotation = localRotation;
        return obj;
    }

    private static Quaternion FacingEntry(WagonLayout wagon)
    {
        if (!wagon.entry || !wagon.exit) return Quaternion.identity;
        var dir = wagon.transform.InverseTransformDirection(wagon.entry.position - wagon.exit.position);
        dir.y = 0f;
        return dir.sqrMagnitude > 0.001f ? Quaternion.LookRotation(dir) : Quaternion.identity;
    }

    private static void Discard(GameObject go)
    {
        if (Application.isPlaying) Destroy(go);
        else DestroyImmediate(go);
    }

    private int RandomRange(Vector2Int range)
    {
        return _rng.Next(range.x, range.y + 1);
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private T PickWeighted<T>(List<T> items, Func<T, float> weight)
    {
        float total = 0f;
        foreach (var item in items) total += weight(item);

        float roll = (float)_rng.NextDouble() * total;
        foreach (var item in items)
        {
            roll -= weight(item);
            if (roll <= 0f) return item;
        }
        return items[items.Count - 1];
    }
}
