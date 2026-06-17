using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System; 

namespace Project.Scripts.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance; 

        // O Evento Global que avisa os outros scripts
        public static event Action<string> OnDialogueAction;

        [Header("Referências da UI")]
        public GameObject dialoguePanel; 
        public TextMeshProUGUI nameText; 
        public TextMeshProUGUI dialogueText; 
        
        [Header("Opções de Resposta")]
        public Transform optionsParent; 
        public GameObject optionButtonPrefab; 

        private DialogueData _currentDialogue;
        private int _currentLineIndex = 0;
        private bool _isTyping = false;

        private void Awake()
        {
            Instance = this;
            dialoguePanel.SetActive(false);
        }

        public void StartDialogue(DialogueData dialogue)
        {
            // [CORREÇÃO 1] Pára qualquer texto "fantasma" que ainda esteja a ser escrito!
            StopAllCoroutines();
            _isTyping = false;

            _currentDialogue = dialogue;
            _currentLineIndex = 0;
            dialoguePanel.SetActive(true);
            
            // Limpa os botões antigos de forma segura
            foreach (Transform child in optionsParent) 
            {
                Destroy(child.gameObject);
            }
            
            StartCoroutine(TypeLine());
        }

        private IEnumerator TypeLine()
        {
            _isTyping = true;
            dialogueText.text = "";
            nameText.text = _currentDialogue.npcName;

            foreach (char c in _currentDialogue.npcLines[_currentLineIndex].ToCharArray())
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(0.02f); 
            }
            
            _isTyping = false;
            CheckShowOptions();
        }

        private void Update()
        {
            if (!dialoguePanel.activeSelf) return;

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (_isTyping)
                {
                    StopAllCoroutines();
                    dialogueText.text = _currentDialogue.npcLines[_currentLineIndex];
                    _isTyping = false;
                    CheckShowOptions();
                }
                else if (optionsParent.childCount == 0) 
                {
                    NextLine();
                }
            }
        }

        private void NextLine()
        {
            _currentLineIndex++;
            if (_currentLineIndex < _currentDialogue.npcLines.Length)
            {
                StartCoroutine(TypeLine());
            }
            else
            {
                EndDialogue();
            }
        }

        private void CheckShowOptions()
        {
            if (_currentLineIndex == _currentDialogue.npcLines.Length - 1 && _currentDialogue.options.Length > 0)
            {
                foreach (var option in _currentDialogue.options)
                {
                    GameObject btnObj = Instantiate(optionButtonPrefab, optionsParent);
                    btnObj.GetComponentInChildren<TextMeshProUGUI>().text = option.playerChoiceText;
                    
                    btnObj.GetComponent<Button>().onClick.AddListener(() => OnOptionSelected(option));
                }
            }
        }

        private void OnOptionSelected(DialogueOption option)
        {
            // [CORREÇÃO 2] Fecha ou avança a janela PRIMEIRO.
            if (option.nextDialogue != null)
            {
                StartDialogue(option.nextDialogue); 
            }
            else
            {
                EndDialogue(); 
            }

            // [CORREÇÃO 2] E só DEPOIS dispara o evento. Se o evento abrir um novo diálogo, não haverá conflitos!
            if (!string.IsNullOrEmpty(option.actionName))
            {
                OnDialogueAction?.Invoke(option.actionName);
            }
        }

        public void EndDialogue()
        {
            // [CORREÇÃO 1] Garante que a máquina de escrever se desliga ao afastar-se do NPC
            StopAllCoroutines();
            _isTyping = false;

            dialoguePanel.SetActive(false);
            foreach (Transform child in optionsParent) 
            {
                Destroy(child.gameObject);
            }
        }
    }
}