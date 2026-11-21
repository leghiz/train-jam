using UnityEngine;

public class EnemyEncounter : MonoBehaviour
{
    [SerializeField] private EncounterData encounterData;

    public EncounterData EncounterData => encounterData;
}