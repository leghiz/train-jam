using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "TrainJam/Item")]
public class ItemData : ScriptableObject
{
    public string id;
    public string displayName;
    public Sprite icon;
    public GameObject worldPrefab;

    [Header("Эффект")]
    public float heal;
    public float stressChange;
    public bool isKey;
    public bool consumeOnUse = true;
}
