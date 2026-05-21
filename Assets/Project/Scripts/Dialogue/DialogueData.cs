using UnityEngine;

namespace Project.Scripts.Dialogue
{
    [System.Serializable]
    public struct DialogueOption
    {
        public string playerChoiceText; // Texto do botão (Ex: "Onde estou?")
        public DialogueData nextDialogue; // Para qual diálogo essa opção leva? (Vazio encerra a conversa)
    }

    [CreateAssetMenu(fileName = "NovoDialogo", menuName = "Fiore/Dialogo/Nó")]
    public class DialogueData : ScriptableObject
    {
        [Header("Informações do NPC")]
        public string npcName; // Nome de quem fala (Ex: "Ancião Armilla")
        
        [Header("Falas (Separadas por tela)")]
        [TextArea(3, 5)]
        public string[] npcLines; // Array de frases. Cada item é uma "página" de texto.

        [Header("Respostas do Jogador")]
        public DialogueOption[] options; // As opções que aparecem no fim das falas
    }
}