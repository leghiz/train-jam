using System.Collections.Generic;
using UnityEngine;

public class ArmorRandomizer : MonoBehaviour
{
    private void Awake()
    {
        Armor[] plates = GetComponentsInChildren<Armor>();
        List<Armor> slots = new List<Armor>();

        foreach (Armor plate in plates)
        {
            if (plate.pair != null && slots.Contains(plate.pair))
                continue;

            slots.Add(plate);
        }

        if (slots.Count == 0)
            return;

        Armor open = slots[Random.Range(0, slots.Count)];

        if (open.pair != null)
        {
            open.pair.gameObject.SetActive(false);
            Destroy(open.pair.gameObject);
        }

        open.gameObject.SetActive(false);
        Destroy(open.gameObject);
    }
}
