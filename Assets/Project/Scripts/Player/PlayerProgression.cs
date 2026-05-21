using UnityEngine;

public class PlayerProgression : MonoBehaviour
{
    [Header("Nível do Personagem")]
    [SerializeField] private int characterLevel = 1;

    [Header("Configurações de Custo (Soulslike)")]
    [SerializeField] private int baseLevelCost = 100;
    [SerializeField] private float costMultiplier = 1.2f;

    [Header("Recursos")]
    [SerializeField] private int currentSouls = 0;

    [Header("Mecânica de Morte")]
    [SerializeField] private GameObject soulDropPrefab;
    private GameObject _currentActiveSoulDrop;

    private BaseStats _stats;

    private void Awake() => _stats = GetComponent<BaseStats>();

    // Adiciona almas ao saldo
    public void AddSouls(int amount) => currentSouls += amount;

    // Método para subtrair almas (Novo!)
    public bool SpendSouls(int amount)
    {
        if (currentSouls >= amount)
        {
            currentSouls -= amount;
            SaveProgression(); // Salva imediatamente para evitar perda de dados
            return true;
        }
        return false;
    }

    public int GetRequiredSoulsForNextLevel()
    {
        return Mathf.RoundToInt(baseLevelCost * Mathf.Pow(costMultiplier, characterLevel - 1));
    }

    public void AllocatePoint(StatType stat)
    {
        int cost = GetRequiredSoulsForNextLevel();
        if (currentSouls >= cost)
        {
            currentSouls -= cost;
            characterLevel++;
            if(_stats != null) _stats.UpgradeStat(stat);
            SaveProgression();
        }
    }

    public void DropSoulsOnDeath()
    {
        if (currentSouls <= 0 || soulDropPrefab == null) return;

        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null) playerCollider.enabled = false; 

        if (_currentActiveSoulDrop != null) Destroy(_currentActiveSoulDrop);

        _currentActiveSoulDrop = Instantiate(soulDropPrefab, transform.position, Quaternion.identity);
        
        if (_currentActiveSoulDrop.TryGetComponent(out SoulDrop soulScript))
        {
            soulScript.Initialize(currentSouls);
        }

        currentSouls = 0; 
        GameData.CurrentSouls = 0;
        SaveProgression();
    }

    public void ReEnablePlayerCollider()
    {
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null) playerCollider.enabled = true;
    }

    public void SaveProgression()
    {
        GameData.PlayerLevel = characterLevel;
        GameData.CurrentSouls = currentSouls;
        GameData.HasSaveData = true;
    }

    private void Start()
    {
        _stats = GetComponent<BaseStats>();
        if (GameData.HasSaveData)
        {
            characterLevel = GameData.PlayerLevel;
            currentSouls = GameData.CurrentSouls;
        }
    }

    public int GetCurrentLevel() => characterLevel;
    public int GetCurrentSouls() => currentSouls;
}