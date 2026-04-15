using UnityEngine;

[CreateAssetMenu(fileName = "New Stat Type", menuName = "RPG/Stats/Stat Type")]
public class StatType : ScriptableObject
{
    public string statName;
    public Sprite icon;
}