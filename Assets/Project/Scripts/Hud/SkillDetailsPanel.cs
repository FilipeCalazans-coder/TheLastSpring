using UnityEngine;
using UnityEngine.UI;
using TMPro; // Usado para os textos
using Project.Scripts.Skills;
using Project.Scripts.Player;



namespace Project.Scripts.UI
{
    public class SkillDetailsPanel : MonoBehaviour
    {
        [Header("Referências Visuais")]
        public GameObject panelRoot; // O painel principal (para podermos esconder)
        public TextMeshProUGUI skillNameText;
        public TextMeshProUGUI skillDescriptionText;
        public TextMeshProUGUI skillCostText;
        
        [Header("Botão de Ação")]
        public Button buyButton;
        public TextMeshProUGUI buyButtonText;

        private SkillData _currentSkill;
        private SkillTreeManager _treeManager;
        private PlayerSkills _playerSkills;

        // Configuração inicial chamada pelo Manager
        public void Setup(SkillTreeManager manager, PlayerSkills playerSkills)
        {
            _treeManager = manager;
            _playerSkills = playerSkills;
            ClearPanel(); // Esconde o painel no início
        }

        // Chamado quando o jogador clica num nó da árvore
        public void UpdatePanel(SkillData skill)
        {
            if (skill == null) return;
            
            _currentSkill = skill;
            panelRoot.SetActive(true); // Mostra o painel

            // Preenche os dados usando o que já definiste no teu ScriptableObject!
            skillNameText.text = skill.skillName;
            skillDescriptionText.text = skill.description;
            skillCostText.text = $"Custo: {skill.pollenCost} Pólen";

            UpdateButtonState();
        }

        // Atualiza a cor e o texto do botão de compra dependendo da carteira da Fiorella
        public void UpdateButtonState()
        {
            if (_currentSkill == null || _playerSkills == null) return;

            buyButton.onClick.RemoveAllListeners(); // Limpa memórias de cliques antigos
            buyButton.onClick.AddListener(OnBuyClicked); // Conecta o clique à função de compra

            if (_playerSkills.HasSkill(_currentSkill))
            {
                buyButtonText.text = "COMPRADO";
                buyButton.interactable = false; // Desativa o botão
            }
            else if (_playerSkills.CanUnlock(_currentSkill))
            {
                buyButtonText.text = "DESBLOQUEAR";
                buyButton.interactable = true; // Permite comprar
            }
            else
            {
                buyButtonText.text = "BLOQUEADO";
                buyButton.interactable = false; // Faltam requisitos ou Pólen
            }
        }

        private void OnBuyClicked()
        {
            if (_currentSkill != null && _treeManager != null)
            {
                // Avisa o Manager para processar a compra
                _treeManager.ProcessSkillPurchase(_currentSkill);
                
                // Atualiza o estado deste botão para dizer "COMPRADO"
                UpdateButtonState(); 
            }
        }

        public void ClearPanel()
        {
            _currentSkill = null;
            panelRoot.SetActive(false);
        }
    }
}