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
            // SEGURANÇA: Verifica se você não deixou um slot vazio no Inspector
            if (stat != null && stat.type != null) 
            {
                if (!_statsDict.ContainsKey(stat.type))
                {
                    _statsDict.Add(stat.type, stat);
                }
            }
            else
            {
                Debug.LogError($"Existe um slot de Status vazio no objeto {gameObject.name}!");
            }
        }
    }

    public int GetStatValue(StatType type)
    {
        if (_statsDict.TryGetValue(type, out Stat stat))
        {
            // Retornamos o valor já com a porcentagem aplicada
            return stat.CalculatedValue;
        }
        
        Debug.LogWarning($"O status {type.statName} não foi encontrado!");
        return 0;
    }

// Novo método para subir porcentagem (ex: um buff ou passiva)
    public void AddStatPercentage(StatType type, float percentAmount)
    {
        if (_statsDict.TryGetValue(type, out Stat stat))
        {
            stat.AddPercentModifier(percentAmount);
            Debug.Log($"{type.statName} agora tem bônus de {percentAmount * 100}%");
        }
    }

    // Verifique se está EXATAMENTE assim no BaseStats.cs
    public void AddStatPoint(StatType type, int amount)
    {
        if (_statsDict.TryGetValue(type, out Stat stat))
        {
            stat.AddBaseValue(amount); // Ou stat.baseValue += amount se você não criou o método
            Debug.Log($"{type.statName} subiu para {stat.CalculatedValue}!");
        }
    }
}