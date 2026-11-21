using UnityEngine;

public abstract class PickupData : ScriptableObject
{
    public string id;
    public string displayName;
    public GameObject worldPrefab;
}
