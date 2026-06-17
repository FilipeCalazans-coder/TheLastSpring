using UnityEngine;

namespace Project.Scripts.Dialogue
{
    [System.Serializable]
    public struct DialogueOption
    {
        public string playerChoiceText; 
        public DialogueData nextDialogue; 
        
        // [NOVO] O nome da ação que queremos disparar para os outros sistemas
        [Tooltip("Nome da ação especial a disparar (ex: AbrirMercador, AbrirQuest)")]
        public string actionName; 
    }

    [CreateAssetMenu(fileName = "NovoDialogo", menuName = "Fiore/Dialogo/No")]
    public class DialogueData : ScriptableObject
    {
        [Header("Informações do NPC")]
        public string npcName; 
        
        [Header("Falas (Separadas por tela)")]
        [TextArea(3, 5)]
        public string[] npcLines; 

        [Header("Respostas do Jogador")]
        public DialogueOption[] options; 
    }
}