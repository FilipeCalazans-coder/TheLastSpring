using System.Collections.Generic;
using UnityEngine;
using Project.Scripts.Player;
using Project.Scripts.Hud; 
using Project.Scripts.Skills;

namespace Project.Scripts.UI
{
    public class SkillTreeManager : MonoBehaviour
    {
        [Header("Referências de UI")]
        public SkillDetailsPanel detailsPanel; // NOVO: Referência para o painel de detalhes

        [Header("Configuração")]
        [Tooltip("Arrasta todos os botões (SkillSlotUI) da tua árvore para aqui.")]
        public List<SkillSlotUI> allSkillSlots;
        
        [Tooltip("Arrasta todas as linhas (SkillConnectionUI) da tua árvore para aqui.")]
        public List<SkillConnectionUI> allConnections;

        private PlayerSkills _playerSkills;
        private LevelUpUI _levelUpUI;

        private void Awake()
        {
            if (PlayerController.Instance != null)
            {
                _playerSkills = PlayerController.Instance.GetComponent<PlayerSkills>();
            }
            
            _levelUpUI = Object.FindFirstObjectByType<LevelUpUI>();
        }

        private void Start()
        {
            // Inicializa todos os botões
            foreach (var slot in allSkillSlots)
            {
                slot.SetupSlot(this);
            }

            // Inicializa o painel de detalhes
            if (detailsPanel != null && _playerSkills != null)
            {
                detailsPanel.Setup(this, _playerSkills);
            }
        }

        private void OnEnable()
        {
            RefreshAllSlots();
            if (detailsPanel != null) detailsPanel.ClearPanel(); // Esconde os detalhes ao reabrir a fogueira
        }

        public void RefreshAllSlots()
        {
            if (_playerSkills == null) return;

            foreach (var slot in allSkillSlots)
            {
                slot.UpdateVisualState(_playerSkills);
            }
            
            foreach (var connection in allConnections)
            {
                connection.UpdateConnectionState(_playerSkills);
            }
            
            if (_levelUpUI != null) _levelUpUI.RefreshUI();
            
            // Se o painel de detalhes estiver aberto, atualiza-o também
            if (detailsPanel != null) detailsPanel.UpdateButtonState();
        }

        // ATUALIZADO: Agora apenas abre os detalhes, não compra logo!
        public void OnSkillNodeClicked(SkillSlotUI clickedSlot)
        {
            if (detailsPanel != null && clickedSlot.skillData != null)
            {
                detailsPanel.UpdatePanel(clickedSlot.skillData);
            }
        }

        // NOVO: Chamado pelo Botão de Comprar dentro do SkillDetailsPanel
        public void ProcessSkillPurchase(SkillData skill)
        {
            if (_playerSkills == null) return;

            if (_playerSkills.CanUnlock(skill))
            {
                _playerSkills.TryBuySkill(skill);
                RefreshAllSlots();
                Debug.Log($"<color=yellow>Habilidade desbloqueada com sucesso: {skill.skillName}</color>");
            }
        }
    }
}