using System.Collections.Generic;
using UnityEngine;
using Project.Scripts.Player;
using Project.Scripts.Hud;

namespace Project.Scripts.UI
{
    public class SkillTreeManager : MonoBehaviour
    {
        [Header("Configuração")]
        [Tooltip("Arrasta todos os botões (SkillSlotUI) da tua árvore para aqui.")]
        public List<SkillSlotUI> allSkillSlots;

        [Tooltip("Arrasta todas as linhas (SkillConnectionUI) da tua árvore para aqui.")]
        public List<SkillConnectionUI> allConnections;

        private PlayerSkills _playerSkills;
        private LevelUpUI _levelUpUI; // Para atualizarmos o texto do Pólen na tela

        private void Awake()
        {
            // Busca as referências na cena
            if (PlayerController.Instance != null)
            {
                _playerSkills = PlayerController.Instance.GetComponent<PlayerSkills>();
            }
            
            _levelUpUI = Object.FindFirstObjectByType<Project.Scripts.Hud.LevelUpUI>();
        }

        private void Start()
        {
            // Configura todos os nós inicialmente
            foreach (var slot in allSkillSlots)
            {
                slot.SetupSlot(this);
            }
        }

        // Sempre que o painel é ativado (quando abres a fogueira), atualiza as cores
        private void OnEnable()
        {
            RefreshAllSlots();
        }

        public void RefreshAllSlots()
        {
            if (_playerSkills == null) return;

            // 1. Atualiza os botões
            foreach (var slot in allSkillSlots)
            {
                slot.UpdateVisualState(_playerSkills);
            }
            
            // 2. Atualiza as linhas (NOVO!)
            foreach (var connection in allConnections)
            {
                connection.UpdateConnectionState(_playerSkills);
            }
            
            // 3. Atualiza a UI do pólen
            if (_levelUpUI != null) _levelUpUI.RefreshUI();
        }

        // Chamado por um nó quando o jogador clica nele
        public void OnSkillNodeClicked(SkillSlotUI clickedSlot)
        {
            if (_playerSkills == null || clickedSlot.skillData == null) return;

            // Tentamos comprar usando a lógica central
            if (_playerSkills.CanUnlock(clickedSlot.skillData))
            {
                _playerSkills.TryBuySkill(clickedSlot.skillData);
                
                // Se o sistema interconectado funcionar, a compra foi feita. Atualizamos a tela:
                RefreshAllSlots();
                Debug.Log($"Sucesso! Compraste a habilidade.");
            }
            else
            {
                Debug.LogWarning("Não podes comprar isto ainda! Verifica o Pólen ou os Pré-requisitos.");
            }
        }
    }
}