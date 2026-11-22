public static class BattleContext
{
    public static EncounterData CurrentEncounter { get; private set; }

    public static EnemyEncounter CurrentEnemy { get; private set; }

    public static bool EnemyAttackedFirst { get; private set; }

    public static WeaponData Weapon { get; private set; }

    public static InventorySystem Inventory { get; private set; }

    public static bool HasEncounter => CurrentEncounter != null;

    public static void SetEncounter(EncounterData encounter)
    {
        CurrentEncounter = encounter;
    }

    public static void SetEnemy(EnemyEncounter enemy)
    {
        CurrentEnemy = enemy;
    }

    public static void SetEnemyAttackedFirst(bool enemyAttackedFirst)
    {
        EnemyAttackedFirst = enemyAttackedFirst;
    }

    public static void SetInventory(InventorySystem inventory)
    {
        Inventory = inventory;

        if (inventory != null)
            Weapon = inventory.Weapon;
    }

    public static void Clear()
    {
        CurrentEncounter = null;
        CurrentEnemy = null;
        EnemyAttackedFirst = false;
        Weapon = null;
        Inventory = null;
    }
}