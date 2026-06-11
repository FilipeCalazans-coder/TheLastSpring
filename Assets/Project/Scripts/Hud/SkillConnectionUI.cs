using UnityEngine;
using UnityEngine.UI;
using Project.Scripts.Skills;
using Project.Scripts.Player;

namespace Project.Scripts.UI
{
    public class SkillConnectionUI : MonoBehaviour
    {
        [Header("Nó de Destino")]
        [Tooltip("A habilidade para a qual esta linha aponta.")]
        public SkillData targetSkill; // A Habilidade B (O destino da linha)

        [Header("Referência Visual")]
        public Image lineImage;

        [Header("Cores da Linha")]
        public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Cinza escuro (Trancado)
        public Color availableColor = new Color(1f, 1f, 1f, 1f);    // Branco (Caminho aberto)
        public Color unlockedColor = new Color(1f, 0.8f, 0f, 1f);   // Dourado (Caminho percorrido)

        // O Maestro (Manager) vai chamar esta função para atualizar a cor
        public void UpdateConnectionState(PlayerSkills playerSkills)
        {
            if (targetSkill == null || playerSkills == null || lineImage == null) return;

            if (playerSkills.HasSkill(targetSkill))
            {
                // Já comprou a habilidade para onde a linha aponta
                lineImage.color = unlockedColor;
            }
            else if (playerSkills.CanUnlock(targetSkill))
            {
                // A habilidade destino cumpriu os pré-requisitos e pode ser comprada
                lineImage.color = availableColor;
            }
            else
            {
                // A habilidade destino ainda está trancada
                lineImage.color = lockedColor;
            }
        }
    }
}