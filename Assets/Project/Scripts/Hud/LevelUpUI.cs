using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    [Header("Referências de Texto")]
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private TextMeshProUGUI atkText, defText, vitText, resText;
    [SerializeField] private TextMeshProUGUI characterLevelText;

    [Header("Painel Visual")]
    [SerializeField] private GameObject visualPanel; // O painel cinza

    [Header("Status Types")]
    [SerializeField] private StatType atkType;
    [SerializeField] private StatType defType;
    [SerializeField] private StatType vitType;
    [SerializeField] private StatType resType;

    private PlayerProgression _progression;
    private BaseStats _stats;
    private bool _isOpen = false;
    private PlayerControls _playerControls;

    private void Awake()
    {
        _playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        _playerControls.Enable();
        _playerControls.Menu.Enable(); // Garante que a tecla de abrir/fechar sempre funcione
    }
    
    private void Start()
    {
        if (PlayerController.Instance != null)
        {
            _progression = PlayerController.Instance.GetComponent<PlayerProgression>();
            _stats = PlayerController.Instance.GetComponent<BaseStats>();
        }

        _playerControls.Menu.ToggleMenu.performed += _ => ToggleWindow();
        
        visualPanel.SetActive(false);
        _isOpen = false;
    }

    private void OnDisable()
    {
        _playerControls.Disable();
    }

    public void ToggleWindow()
    {
        _isOpen = !_isOpen;
        
        
        visualPanel.SetActive(_isOpen);
        
        if (_isOpen) 
        {
            RefreshUI();
            _playerControls.Combat.Disable(); 
            _playerControls.Movement.Disable();
        }
        else 
        {
            _playerControls.Combat.Enable();
            _playerControls.Movement.Enable();
        }
    }

    public void RefreshUI()
    {
        if (_progression == null || _stats == null) return;

        int currentSouls = _progression.GetCurrentSouls();
        int required = _progression.GetRequiredSoulsForNextLevel();

        pointsText.text = $"Almas: {currentSouls} / Necessário: {required}";
        
        // ATUALIZAÇÃO: Mostra o nível atual na janela de atributos
        characterLevelText.text = "Nível: " + _progression.GetCurrentLevel();

        pointsText.color = currentSouls >= required ? Color.white : Color.red;

        atkText.text = "Ataque: " + _stats.GetStatValue(atkType);
        defText.text = "Defesa: " + _stats.GetStatValue(defType) + "%";
        vitText.text = "Vitalidade: " + _stats.GetStatValue(vitType);
        resText.text = "Resistência: " + _stats.GetStatValue(resType);
    }

    // Métodos dos Botões
    public void UpgradeAtk() { _progression.AllocatePoint(atkType); RefreshUI(); }
    public void UpgradeDef() { _progression.AllocatePoint(defType); RefreshUI(); }
    public void UpgradeVit() 
    { 
        _progression.AllocatePoint(vitType); 
        PlayerController.Instance.GetComponent<PlayerHealth>().UpdateMaxHealth(); 
        RefreshUI(); 
    }
    public void UpgradeRes() 
    { 
        _progression.AllocatePoint(resType); 
        PlayerController.Instance.GetComponent<PlayerStamina>().UpdateMaxStamina(); 
        RefreshUI(); 
    }
}