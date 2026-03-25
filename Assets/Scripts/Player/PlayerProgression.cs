using UnityEngine;

public class PlayerProgression : MonoBehaviour
{
    [Header("Nível do Personagem")]
    [SerializeField] private int characterLevel = 1;
    [SerializeField] private int availablePoints = 0;

    [Header("Configurações de Custo")]
    [SerializeField] private int baseLevelUpCost = 100;
    [SerializeField] private float costMultiplier = 1.2f;

    [Header("Recursos")]
    [SerializeField] private int currentSouls = 0;

    private BaseStats _stats;

    private void Awake() => _stats = GetComponent<BaseStats>();

    public void AddSouls(int amount) => currentSouls += amount;

    // O custo agora depende apenas do Nível Geral do Personagem
    public int GetNextLevelCost()
    {
        // Ex: Level 1 custa 100. Level 2 custa 100 * (2^1.2)...
        return Mathf.RoundToInt(baseLevelUpCost * Mathf.Pow(characterLevel, costMultiplier));
    }

    // Função para comprar um Level Geral
    public void BuyLevel()
    {
        int cost = GetNextLevelCost();

        if (currentSouls >= cost)
        {
            currentSouls -= cost;
            characterLevel++;
            availablePoints++; // Ganha um ponto para gastar
            Debug.Log($"<color=green>Subiu para o Level {characterLevel}! Pontos disponíveis: {availablePoints}</color>");
        }
        else
        {
            Debug.Log($"<color=red>Faltam {cost - currentSouls} almas para o próximo nível.</color>");
        }
    }

    // Função para alocar o ponto que você ganhou
    public void AllocatePoint(StatType type)
    {
        if (availablePoints > 0)
        {
            availablePoints--;
            _stats.AddStatPoint(type, 1);
            Debug.Log($"Ponto alocado em {type.statName}. Restam: {availablePoints}");
        }
        else
        {
            Debug.Log("Você não tem pontos de atributo disponíveis! Suba de nível primeiro.");
        }
    }


    public int GetAvailablePoints() => availablePoints;
    public int GetCurrentLevel() => characterLevel;

    public int GetCurrentSouls() => currentSouls;
}