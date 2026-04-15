using UnityEngine;

[System.Serializable]
public class Stat
{
    public StatType type;
    public int baseValue;
    [SerializeField] private float percentMultiplier = 0f; // 0.1f seria +10%

    public Stat(StatType type, int baseValue)
    {
        this.type = type;
        this.baseValue = baseValue;
    }

    // A "Mágica" acontece aqui: o Getter calcula o valor real na hora
    public int CalculatedValue 
    {
        get 
        {
            float total = baseValue * (1 + percentMultiplier);
            return Mathf.FloorToInt(total); // Arredonda para baixo para evitar quebrados
        }
    }

    
    public void AddBaseValue(int amount) => baseValue += amount;
    
    public void AddPercentModifier(float percentage) => percentMultiplier += percentage;

    public float AsPercentage => baseValue / 100f;
}