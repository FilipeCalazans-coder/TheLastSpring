using UnityEngine;

namespace Project.Scripts.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [Tooltip("O diálogo que vai tocar quando o jogador interagir com este NPC")]
        public DialogueData startingDialogue;
        
        private bool _isPlayerNear = false;
        
        // NOVA VARIÁVEL DO INPUT SYSTEM
        private PlayerControls _playerControls;

        private void Awake()
        {
            _playerControls = new PlayerControls();
        }

        private void OnEnable()
        {
            _playerControls.Enable();
        }

        private void OnDisable()
        {
            _playerControls.Disable();
        }

        private void Update()
        {
            // VERIFICAÇÃO COM O NOVO INPUT SYSTEM
            if (_isPlayerNear && _playerControls.Menu.Interact.triggered)
            {
                if (DialogueManager.Instance != null && !DialogueManager.Instance.dialoguePanel.activeSelf)
                {
                    DialogueManager.Instance.StartDialogue(startingDialogue);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlayerNear = true;
                Debug.Log("<color=cyan>Pressione [E] para interagir.</color>");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlayerNear = false;
                DialogueManager.Instance?.EndDialogue();
            }
        }
    }
}