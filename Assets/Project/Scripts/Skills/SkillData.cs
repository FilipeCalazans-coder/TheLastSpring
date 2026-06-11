using UnityEngine;

namespace Project.Scripts.Skills
{
    [CreateAssetMenu(fileName = "NovaHabilidade", menuName = "Fiore/Habilidades/Habilidade Ativa")]
    public class SkillData : ScriptableObject
    {
        [Header("Informações Básicas")]
        public string skillName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;

        [Header("Economia e Regras")]
        [Tooltip("Quantidade de Pólen necessária para comprar.")]
        public int pollenCost = 100;
        
        [Tooltip("Habilidades que o jogador precisa ter ANTES de comprar esta.")]
        public SkillData[] prerequisites;

        [Header("Mecânica no Jogo")]
        [Tooltip("O código interno que o PlayerController vai ler para saber qual golpe executar (ex: 'DashAereo', 'AtaqueDuplo').")]
        public string activeMechanicID; 
    }
}