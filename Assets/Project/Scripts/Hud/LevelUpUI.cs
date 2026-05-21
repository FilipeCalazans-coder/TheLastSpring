using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Project.Scripts.Hud
{
    public class LevelUpUI : MonoBehaviour
    {
        [Header("Referências de Texto")]
        [SerializeField] private TextMeshProUGUI pointsText;
        [SerializeField] private TextMeshProUGUI atkText, defText, vitText, resText;
        [SerializeField] private TextMeshProUGUI characterLevelText;

        [Header("Status Types")]
        [SerializeField] private StatType atkType;
        [SerializeField] private StatType defType;
        [SerializeField] private StatType vitType;
        [SerializeField] private StatType resType;

        private PlayerProgression _progression;
        private BaseStats _stats;

        private void Awake()
        {
            FetchReferences();
        }

        private void OnEnable()
        {
            // Força a busca das referências antes de atualizar os textos
            FetchReferences();
            RefreshUI();
        }

        // Método de busca proativa para evitar o conflito do ciclo de vida da Unity
        private void FetchReferences()
        {
            if (_progression == null || _stats == null)
            {
                if (PlayerController.Instance != null)
                {
                    _progression = PlayerController.Instance.GetComponent<PlayerProgression>();
                    _stats = PlayerController.Instance.GetComponent<BaseStats>();
                }
            }
        }

        public void RefreshUI()
        {
            // Dupla checagem segura para garantir que os dados estão prontos
            FetchReferences();
            if (_progression == null || _stats == null) return;

            int currentPolen = _progression.GetCurrentSouls(); 
            int required = _progression.GetRequiredSoulsForNextLevel();

            pointsText.text = $"Pólen: {currentPolen} / Necessário: {required}";
            characterLevelText.text = "Nível: " + _progression.GetCurrentLevel();

            pointsText.color = currentPolen >= required ? Color.white : Color.red;

            atkText.text = "Ataque: " + _stats.GetStatValue(atkType);
            defText.text = "Defesa: " + _stats.GetStatValue(defType);
            vitText.text = "Vitalidade: " + _stats.GetStatValue(vitType);
            resText.text = "Resistência: " + _stats.GetStatValue(resType);
        }

        // --- MÉTODOS DOS BOTÕES DE UPGRADE ---
        public void UpgradeAtk() { _progression.AllocatePoint(atkType); RefreshUI(); }
        public void UpgradeDef() { _progression.AllocatePoint(defType); RefreshUI(); }
        
        public void UpgradeVit() 
        { 
            _progression.AllocatePoint(vitType); 
            PlayerController.Instance.GetComponent<PlayerHealth>()?.UpdateMaxHealth(); 
            RefreshUI(); 
        }
        
        public void UpgradeRes() 
        { 
            _progression.AllocatePoint(resType); 
            PlayerController.Instance.GetComponent<PlayerStamina>()?.UpdateMaxStamina(); 
            RefreshUI(); 
        }
    }
}