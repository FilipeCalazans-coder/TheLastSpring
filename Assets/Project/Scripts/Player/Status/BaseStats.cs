using System.Collections.Generic;
using UnityEngine;



public class BaseStats : MonoBehaviour
{
    [Header("Setup de Atributos")]
    [SerializeField] private List<Stat> playerStats = new List<Stat>();

    private Dictionary<StatType, Stat> _statsDict = new Dictionary<StatType, Stat>();

    public List<Stat> GetPlayerStatsList() => playerStats;

    private void Awake()
    {
        InitializeDictionary();
    }


    private void InitializeDictionary()
    {
        _statsDict.Clear();
        foreach (var stat in playerStats)
        {
            if (stat != null && stat.type != null) 
            {
                if (!_statsDict.TryAdd(stat.type, stat))
                {
                    Debug.LogWarning($"Atributo duplicado detectado: {stat.type.statName} em {gameObject.name}");
                }
            }
            else
            {
                Debug.LogError($"Existe um slot de Status vazio ou sem tipo no objeto {gameObject.name}!");
            }
        }
    }

    // --- MÉTODOS DE BUSCA ---
    public int GetStatValue(StatType type)
    {
        if (type != null && _statsDict.TryGetValue(type, out Stat stat))
        {
            return stat.CalculatedValue;
        }
        
        Debug.LogWarning($"O status {(type != null ? type.statName : "Nulo")} não foi encontrado!");
        return 0;
    }

    // --- MÉTODOS DE UPGRADE (Utilizados pelo PlayerProgression) ---
    public void UpgradeStat(StatType type)
    {
        AddStatPoint(type, 1);
    }

    public void AddStatPoint(StatType type, int amount)
    {
        if (_statsDict.TryGetValue(type, out Stat stat))
        {
            stat.AddBaseValue(amount); 
            Debug.Log($"<color=cyan>{type.statName} base aumentado em {amount}. Valor atual: {stat.CalculatedValue}</color>");
        }
    }

    public void AddStatPercentage(StatType type, float percentAmount)
    {
        if (_statsDict.TryGetValue(type, out Stat stat))
        {
            stat.AddPercentModifier(percentAmount);
            Debug.Log($"{type.statName} recebeu bônus de {percentAmount * 100}%");
        }
    }

    // Recebe os dados do GameData e aplica nos atributos reais
    public void LoadSerializedStats(Dictionary<StatType, int> savedData)
    {
        foreach (var entry in savedData)
        {
            if (_statsDict.TryGetValue(entry.Key, out Stat stat))
            {
                stat.baseValue = entry.Value;
            }
        }
        Debug.Log("<color=green>Atributos recarregados com sucesso!</color>");
    }
}