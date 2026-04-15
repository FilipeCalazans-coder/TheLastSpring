using System.Collections.Generic;

public static class GameData
{
    public static int PlayerLevel = 1;
    public static int CurrentSouls = 0;
    public static bool HasSaveData = false; // Verifique se o nome é este
    public static Dictionary<StatType, int> SavedStats = new Dictionary<StatType, int>();
}