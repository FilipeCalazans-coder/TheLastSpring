using System.Collections.Generic;
using UnityEngine;
using Project.Scripts.Skills;

namespace Project.Scripts.Player
{
    public class PlayerSkills : MonoBehaviour
    {
        [Header("Memória de Combate")]
        [Tooltip("Lista de habilidades que a Fiorella já aprendeu.")]
        public List<SkillData> unlockedSkills = new List<SkillData>();

        private PlayerProgression _progression;

        private void Awake()
        {
            _progression = GetComponent<PlayerProgression>();
        }

        // 1. Verifica de forma rápida se uma habilidade já foi comprada
        public bool HasSkill(SkillData skill)
        {
            return unlockedSkills.Contains(skill);
        }

        // 2. Analisa todas as regras do sistema antes de permitir a compra
        public bool CanUnlock(SkillData skill)
        {
            // Regra A: Já possui esta habilidade?
            if (HasSkill(skill)) return false;

            // Regra B: Tem Pólen suficiente na carteira?
            if (_progression != null && _progression.GetCurrentSouls() < skill.pollenCost) return false;

            // Regra C: Verificação sistémica dos Pré-requisitos
            if (skill.prerequisites != null)
            {
                foreach (SkillData prereq in skill.prerequisites)
                {
                    if (!HasSkill(prereq)) return false; // Falhou num dos requisitos
                }
            }

            return true; // Passou em todos os testes!
        }

        // 3. Tenta efetuar a compra interagindo com os teus outros scripts
        public void TryBuySkill(SkillData skill)
        {
            if (CanUnlock(skill))
            {
                // Consome o Pólen usando o teu sistema já existente
                _progression.SpendSouls(skill.pollenCost);
                
                // Grava a habilidade na memória
                unlockedSkills.Add(skill);
                
                Debug.Log($"<color=cyan>Habilidade Ativa Adquirida: {skill.skillName}!</color>");
            }
            else
            {
                Debug.LogWarning("<color=red>A compra falhou. Pólen insuficiente ou pré-requisitos em falta.</color>");
            }
        }

        // NOVO: Procura se a Fiorella tem uma habilidade através do ID de mecânica
        public bool HasSkillByID(string mechanicID)
        {
            foreach (var skill in unlockedSkills)
            {
                if (skill.activeMechanicID == mechanicID)
                {
                    return true;
                }
            }
            return false;
        }
    }
}