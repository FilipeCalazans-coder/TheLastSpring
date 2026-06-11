using UnityEngine;
using UnityEngine.UI;
using Project.Scripts.Skills;
using Project.Scripts.Player;

namespace Project.Scripts.UI
{
    public class SkillSlotUI : MonoBehaviour
    {
        [Header("Configuração do Nó")]
        [Tooltip("A receita (ScriptableObject) que este botão representa.")]
        public SkillData skillData; 
        
        [Header("Referências Visuais")]
        public Image iconImage;
        public Button skillButton;

        [Header("Cores de Estado")]
        public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Cinza escuro (Trancado)
        public Color availableColor = new Color(1f, 1f, 1f, 1f);    // Branco (Pode comprar)
        public Color unlockedColor = new Color(1f, 0.8f, 0f, 1f);   // Dourado (Já comprado)

        private SkillTreeManager _treeManager;

        public void SetupSlot(SkillTreeManager manager)
        {
            _treeManager = manager;
            
            if (skillData != null && iconImage != null)
            {
                iconImage.sprite = skillData.icon;
            }

            // O botão avisa o Manager quando é clicado
            skillButton.onClick.AddListener(OnSlotClicked);
        }

        // Esta função aplica a lógica de consequências: avalia o estado e muda o visual
        public void UpdateVisualState(PlayerSkills playerSkills)
        {
            if (skillData == null || playerSkills == null) return;

            if (playerSkills.HasSkill(skillData))
            {
                iconImage.color = unlockedColor; // Já comprou
                skillButton.interactable = false; // Não precisa clicar mais
            }
            else if (playerSkills.CanUnlock(skillData))
            {
                iconImage.color = availableColor; // Cumpre os pré-requisitos e tem Pólen
                skillButton.interactable = true;
            }
            else
            {
                iconImage.color = lockedColor; // Faltam pré-requisitos ou Pólen
                skillButton.interactable = true; // Deixamos clicável para o Manager poder avisar o jogador do erro
            }
        }

        private void OnSlotClicked()
        {
            if (_treeManager != null)
            {
                _treeManager.OnSkillNodeClicked(this);
            }
        }
    }
}