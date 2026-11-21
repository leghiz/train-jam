using UnityEngine;

[CreateAssetMenu(fileName = "EncounterData", menuName = "Scriptable Objects/EncounterData")]
public class EncounterData : ScriptableObject
{
    [Header("Enemies")]
    [SerializeField] private GameObject[] battleEnemyPrefabs;

    [Header("Loot")]
    [SerializeField] private ItemPickup[] loot;

    public GameObject[] BattleEnemyPrefabs => battleEnemyPrefabs;
    public ItemPickup[] Loot => loot;
}