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
        public static event Action<string> OnDialogueAction;

        // NOVO: Evento que avisa o jogo se o jogador deve ser congelado
        public static event Action<bool> OnDialogueStateChanged;

        [Header("Referências da UI")]
        public GameObject dialoguePanel;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI dialogueText;
        
        // NOVO: Referência para a imagem de retrato na UI
        public Image portraitImage; 

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
            StopAllCoroutines();
            _isTyping = false;
            _currentDialogue = dialogue;
            _currentLineIndex = 0;
            dialoguePanel.SetActive(true);

            // Dispara o evento avisando o estado de congelamento deste diálogo específico
            OnDialogueStateChanged?.Invoke(dialogue.freezePlayer);

            // ==========================================
            // LÓGICA DO RETRATO E ESPELHAMENTO (FLIP)
            // ==========================================
            if (portraitImage != null)
            {
                if (dialogue.npcPortrait != null)
                {
                    portraitImage.sprite = dialogue.npcPortrait;
                    
                    // Lê a variável e inverte o eixo X do RectTransform se for verdadeiro
                    Vector3 currentScale = portraitImage.rectTransform.localScale;
                    currentScale.x = dialogue.flipPortrait ? -1f : 1f;
                    portraitImage.rectTransform.localScale = currentScale;

                    portraitImage.gameObject.SetActive(true);
                }
                else
                {
                    portraitImage.gameObject.SetActive(false); 
                }
            }

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
            if (option.nextDialogue != null)
            {
                StartDialogue(option.nextDialogue);
            }
            else
            {
                EndDialogue();
            }

            if (!string.IsNullOrEmpty(option.actionName))
            {
                OnDialogueAction?.Invoke(option.actionName);
            }
        }

        public void EndDialogue()
        {
            StopAllCoroutines();
            _isTyping = false;
            dialoguePanel.SetActive(false);

            foreach (Transform child in optionsParent)
            {
                Destroy(child.gameObject);
            }

            // Avisa o jogo que o diálogo acabou e o jogador pode voltar a mover-se (false)
            OnDialogueStateChanged?.Invoke(false);
        }
    }
}