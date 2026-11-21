using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public static class WeaponProgressMenu
{
    [MenuItem("TrainJam/Weapons/Reset Progress")]
    private static void ResetProgress()
    {
        foreach (WeaponData weapon in FindAll())
            WeaponProgress.Lock(weapon);
    }

    [MenuItem("TrainJam/Weapons/Unlock All")]
    private static void UnlockAll()
    {
        foreach (WeaponData weapon in FindAll())
            WeaponProgress.Unlock(weapon);
    }

    private static IEnumerable<WeaponData> FindAll()
    {
        return AssetDatabase.FindAssets("t:WeaponData")
            .Select(guid => AssetDatabase.LoadAssetAtPath<WeaponData>(AssetDatabase.GUIDToAssetPath(guid)));
    }
}
