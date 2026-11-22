using UnityEngine;

[CreateAssetMenu(fileName = "EncounterData", menuName = "Scriptable Objects/EncounterData")]
public class EncounterData : ScriptableObject
{
    [Header("Enemies")]
    [SerializeField] private GameObject[] battleEnemyPrefabs;

    [Header("Loot")]
    [SerializeField] private ItemPickup[] loot;
    [SerializeField] private LootChance[] randomLoot;

    public GameObject[] BattleEnemyPrefabs => battleEnemyPrefabs;
    public ItemPickup[] Loot => loot;
    public LootChance[] RandomLoot => randomLoot;

    [System.Serializable]
    public class LootChance
    {
        public ItemPickup item;
        [Range(0f, 1f)] public float chance;
    }
}