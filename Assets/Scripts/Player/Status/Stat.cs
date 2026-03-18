using UnityEngine;

[System.Serializable] 
public class Stat
{
    public StatType type; // Referência ao ScriptableObject acima
    public int baseValue;

    public Stat(StatType type, int baseValue)
    {
        this.type = type;
        this.baseValue = baseValue;
    }
}