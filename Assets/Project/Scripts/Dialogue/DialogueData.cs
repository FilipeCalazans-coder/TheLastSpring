using UnityEngine;

namespace Project.Scripts.Dialogue
{
    [System.Serializable]
    public struct DialogueOption
    {
        public string playerChoiceText;
        public DialogueData nextDialogue;
        
        [Tooltip("Nome da ação especial a disparar (ex: AbrirMercador, AbrirQuest)")]
        public string actionName;
    }

    [CreateAssetMenu(fileName = "NovoDialogo", menuName = "Fiore/Dialogo/No")]
    public class DialogueData : ScriptableObject
    {
        [Header("Informações do NPC")]
        public string npcName;
        
        public Sprite npcPortrait; 

        // NOVO: Controle de espelhamento do sprite
        [Tooltip("Se marcado, o retrato do personagem será espelhado horizontalmente.")]
        public bool flipPortrait; 

        [Header("Falas (Separadas por tela)")]
        [TextArea(3, 5)]
        public string[] npcLines;

        [Header("Respostas do Jogador")]
        public DialogueOption[] options;

        [Header("Controle de Jogador")]
        [Tooltip("Se marcado, a Okyra será forçada a ficar parada durante este diálogo.")]
        public bool freezePlayer = true;
    }
}