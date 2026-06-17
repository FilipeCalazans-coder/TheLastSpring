using UnityEngine;
using Project.Scripts.Inventory; // [NOVO] Necessário para aceder ao MerchantController

namespace Project.Scripts.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [Tooltip("O diálogo que vai tocar quando o jogador interagir com este NPC")]
        public DialogueData startingDialogue;
        
        private bool _isPlayerNear = false;
        private PlayerControls _playerControls;

        private void Awake()
        {
            _playerControls = new PlayerControls();
        }

        private void OnEnable()
        {
            _playerControls.Enable();
            // [NOVO] Começa a ouvir o evento do Manager
            DialogueManager.OnDialogueAction += ExecutarAcaoEspecial;
        }

        private void OnDisable()
        {
            _playerControls.Disable();
            // [NOVO] Pára de ouvir o evento para evitar bugs de memória
            DialogueManager.OnDialogueAction -= ExecutarAcaoEspecial;
        }

        private void Update()
        {
            if (_isPlayerNear && _playerControls.Menu.Interact.triggered)
            {
                if (DialogueManager.Instance != null && !DialogueManager.Instance.dialoguePanel.activeSelf)
                {
                    DialogueManager.Instance.StartDialogue(startingDialogue);
                }
            }
        }

        // ==========================================
        // [NOVO] O cérebro que decide qual script abrir
        // ==========================================
        private void ExecutarAcaoEspecial(string acao)
        {
            // Só executa se a Fiorella estiver perto DESTE NPC específico
            if (!_isPlayerNear) return;

            if (acao == "AbrirMercador")
            {
                MerchantController mercador = GetComponent<MerchantController>();
                if (mercador != null) mercador.OpenMerchantMenu();
            }
            else if (acao == "AbrirQuest")
            {
                NPCQuestGiver missao = GetComponent<NPCQuestGiver>();
                if (missao != null) missao.InteragirComNPC();
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