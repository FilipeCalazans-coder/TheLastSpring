using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Project.Scripts.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance; // Padrão Singleton para fácil acesso

        [Header("Referências da UI")]
        public GameObject dialoguePanel; // O painel inteiro do diálogo
        public TextMeshProUGUI nameText; // Texto do nome do NPC
        public TextMeshProUGUI dialogueText; // Texto da fala
        
        [Header("Opções de Resposta")]
        public Transform optionsParent; // Onde os botões vão aparecer (Um layout group)
        public GameObject optionButtonPrefab; // O molde do botão de resposta

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
            _currentDialogue = dialogue;
            _currentLineIndex = 0;
            dialoguePanel.SetActive(true);
            
            // Limpa botões antigos se houver
            foreach (Transform child in optionsParent) Destroy(child.gameObject);
            
            StartCoroutine(TypeLine());
        }

        // Efeito de máquina de escrever
        private IEnumerator TypeLine()
        {
            _isTyping = true;
            dialogueText.text = "";
            nameText.text = _currentDialogue.npcName;

            // Digita letra por letra
            foreach (char c in _currentDialogue.npcLines[_currentLineIndex].ToCharArray())
            {
                dialogueText.text += c;
                yield return new WaitForSeconds(0.02f); // Velocidade da digitação
            }
            
            _isTyping = false;
            CheckShowOptions();
        }

        private void Update()
        {
            if (!dialoguePanel.activeSelf) return;

            // Clique do mouse ou Espaço avança o diálogo
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (_isTyping)
                {
                    // Se clicar enquanto digita, preenche a frase toda de uma vez
                    StopAllCoroutines();
                    dialogueText.text = _currentDialogue.npcLines[_currentLineIndex];
                    _isTyping = false;
                    CheckShowOptions();
                }
                else if (optionsParent.childCount == 0) // Só avança se NÃO houver botões na tela
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
            // Se for a última fala e existirem opções configuradas, cria os botões
            if (_currentLineIndex == _currentDialogue.npcLines.Length - 1 && _currentDialogue.options.Length > 0)
            {
                foreach (var option in _currentDialogue.options)
                {
                    GameObject btnObj = Instantiate(optionButtonPrefab, optionsParent);
                    btnObj.GetComponentInChildren<TextMeshProUGUI>().text = option.playerChoiceText;
                    
                    // Adiciona o evento de clique via código
                    btnObj.GetComponent<Button>().onClick.AddListener(() => OnOptionSelected(option));
                }
            }
        }

        private void OnOptionSelected(DialogueOption option)
        {
            if (option.nextDialogue != null)
            {
                StartDialogue(option.nextDialogue); // Vai para o próximo nó
            }
            else
            {
                EndDialogue(); // Se o nó estiver vazio, fecha a conversa
            }
        }

        public void EndDialogue()
        {
            dialoguePanel.SetActive(false);
            foreach (Transform child in optionsParent) Destroy(child.gameObject);
        }
    }
}