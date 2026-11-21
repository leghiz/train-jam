using UnityEngine;

public static class WeaponProgress
{
    private const string Prefix = "weapon_unlocked_";

    public static bool IsUnlocked(WeaponData weapon)
    {
        return weapon != null && PlayerPrefs.GetInt(Prefix + weapon.id, 0) == 1;
    }

    public static void Unlock(WeaponData weapon)
    {
        if (weapon == null)
            return;

        PlayerPrefs.SetInt(Prefix + weapon.id, 1);
        PlayerPrefs.Save();
    }

    public static void Lock(WeaponData weapon)
    {
        if (weapon == null)
            return;

        PlayerPrefs.DeleteKey(Prefix + weapon.id);
        PlayerPrefs.Save();
    }
}
