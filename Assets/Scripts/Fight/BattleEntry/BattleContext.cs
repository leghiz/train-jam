public static class BattleContext
{
    public static EncounterData CurrentEncounter { get; private set; }

    public static EnemyEncounter CurrentEnemy { get; private set; }

    public static bool HasEncounter => CurrentEncounter != null;

    public static void SetEncounter(EncounterData encounter)
    {
        CurrentEncounter = encounter;
    }

    public static void SetEnemy(EnemyEncounter enemy)
    {
        CurrentEnemy = enemy;
    }

    public static void Clear()
    {
        CurrentEncounter = null;
        CurrentEnemy = null;
    }
}