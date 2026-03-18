using System.Collections.Generic;
using UnityEngine;

public class BaseStats : MonoBehaviour
{
    [Header("Setup de Atributos")]
    [SerializeField] private List<Stat> playerStats = new List<Stat>();

    // Dicionário para busca rápida em tempo de execução
    private Dictionary<StatType, Stat> _statsDict = new Dictionary<StatType, Stat>();

    private void Awake()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        _statsDict.Clear();
        foreach (var stat in playerStats)
        {
            // Verificamos se o tipo não é nulo para evitar erros no Inspector
            if (stat.type != null && !_statsDict.ContainsKey(stat.type))
            {
                _statsDict.Add(stat.type, stat);
            }
        }
    }

    public int GetStatValue(StatType type)
    {
        if (_statsDict.TryGetValue(type, out Stat stat))
        {
            return stat.baseValue;
        }
        
        Debug.LogWarning($"O status {type.statName} não foi encontrado no Player!");
        return 0;
    }

    public void AddStatPoint(StatType type, int amount)
    {
        if (_statsDict.TryGetValue(type, out Stat stat))
        {
            stat.baseValue += amount;
            Debug.Log($"{type.statName} subiu para {stat.baseValue}!");
        }
    }
}