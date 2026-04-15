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
    private GameObject _currentActiveSoulDrop; // Guarda a referência da poça atual no mapa

    private BaseStats _stats;

    private void Awake() => _stats = GetComponent<BaseStats>();

    public void AddSouls(int amount) => currentSouls += amount;

    // Cálculo de custo: Level 1 custa baseLevelCost, os próximos aumentam exponencialmente
    public int GetRequiredSoulsForNextLevel()
    {
        return Mathf.RoundToInt(baseLevelCost * Mathf.Pow(costMultiplier, characterLevel - 1));
    }

    public bool CanAffordUpgrade()
    {
        return currentSouls >= GetRequiredSoulsForNextLevel();
    }

    // Método principal: Você escolhe o que quer upar e paga com almas
    public void AllocatePoint(StatType stat)
    {
        int cost = GetRequiredSoulsForNextLevel();

        if (currentSouls >= cost)
        {
            currentSouls -= cost; // Paga o custo
            characterLevel++;      // O nível do personagem sobe
            
            if(_stats != null)
            {
                _stats.UpgradeStat(stat); // Aumenta o atributo real (HP, ATK, etc)
            }

        }
        else
        {
            Debug.Log($"<color=red>Almas insuficientes! Precisa de {cost}, mas você tem {currentSouls}.</color>");
        }
    }

    public void DropSoulsOnDeath()
    {
        // 1. Verificações de segurança (essencial para evitar NullReference)
        if (currentSouls <= 0 || soulDropPrefab == null) return;

        // 2. DESATIVA O COLLIDER DO PLAYER para evitar que ele colete as próprias almas instantaneamente
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null) 
        {
            playerCollider.enabled = false; 
            Debug.Log("<color=orange>Collider do Player desativado para evitar auto-coleta.</color>");
        }

        // 3. Gerenciamento da poça antiga (Regra Soulslike)
        if (_currentActiveSoulDrop != null) 
        {
            Destroy(_currentActiveSoulDrop);
        }

        // 4. Instancia a nova poça
        _currentActiveSoulDrop = Instantiate(soulDropPrefab, transform.position, Quaternion.identity);
        
        // 5. Inicializa com as almas atuais
        if (_currentActiveSoulDrop.TryGetComponent(out SoulDrop soulScript))
        {
            soulScript.Initialize(currentSouls);
        }

        // 6. Zera o saldo do player localmente
        currentSouls = 0; 

        // 7. ATUALIZA O GAMEDATA IMEDIATAMENTE (O pulo do gato!)
        GameData.CurrentSouls = 0;
        
        // Opcional: Se você quiser que o nível e atributos também sejam gravados na morte:
        SaveProgression(); 
        
    }

   public void SaveProgression()
    {
        GameData.PlayerLevel = characterLevel;
        
       GameData.CurrentSouls = this.currentSouls;

        // Salva os atributos (força, vida, etc)
        GameData.SavedStats.Clear();
        _stats = GetComponent<BaseStats>();
        if (_stats != null)
        {
            foreach (var stat in _stats.GetPlayerStatsList())
            {
                GameData.SavedStats.Add(stat.type, stat.baseValue);
            }
        }

        GameData.HasSaveData = true;
    }

    private void Start()
    {
        _stats = GetComponent<BaseStats>();

        if (GameData.HasSaveData)
        {
            characterLevel = GameData.PlayerLevel;
            currentSouls = GameData.CurrentSouls;

            this.characterLevel = GameData.PlayerLevel;
            this.currentSouls = GameData.CurrentSouls; // CARREGA AS ALMAS DE VOLTA
        // Atualiza a UI de almas aqui se necessário
            
            // CARREGAR ATRIBUTOS: Passa os valores salvos para o BaseStats
            if (_stats != null)
            {
                _stats.LoadSerializedStats(GameData.SavedStats); // Criaremos esse método abaixo
            }
        }
    }
    // Getters para a UI
    public int GetCurrentLevel() => characterLevel;
    public int GetCurrentSouls() => currentSouls;
}