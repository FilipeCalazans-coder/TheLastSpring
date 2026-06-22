using UnityEngine;
using Project.Scripts.Dialogue;

namespace Project.Scripts.Dialogue
{
    [RequireComponent(typeof(Collider2D))]
    public class AutoDialogueTrigger : MonoBehaviour
    {
        [Header("Dados do Monólogo")]
        [Tooltip("O arquivo de diálogo contendo os pensamentos da Okyra.")]
        public DialogueData monologueData;

        [Header("Regras de Ativação")]
        [Tooltip("Se marcado, o monólogo acontecerá apenas na primeira vez que o jogador cruzar a área.")]
        public bool triggerOnce = true;
        
        private bool _hasTriggered = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Bloqueia a execução se já foi disparado e a regra for de disparo único
            if (_hasTriggered && triggerOnce) return;

            // Valida se quem cruzou a linha foi o Player
            if (other.CompareTag("Player"))
            {
                // Garante que o Manager existe e previne sobreposição se outro diálogo já estiver aberto
                if (DialogueManager.Instance != null && !DialogueManager.Instance.dialoguePanel.activeSelf)
                {
                    DialogueManager.Instance.StartDialogue(monologueData);
                    _hasTriggered = true;
                    Debug.Log("<color=cyan>[Sistema] Monólogo automático ativado com sucesso.</color>");
                }
            }
        }
    }
}