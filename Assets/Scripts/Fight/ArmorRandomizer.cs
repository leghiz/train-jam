using UnityEngine;

public class ArmorRandomizer : MonoBehaviour
{
    private void Awake()
    {
        Armor[] plates = GetComponentsInChildren<Armor>();

        if (plates.Length == 0)
            return;

        Armor open = plates[Random.Range(0, plates.Length)];
        open.gameObject.SetActive(false);
        Destroy(open.gameObject);
    }
}
