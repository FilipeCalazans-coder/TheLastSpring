using System.Collections.Generic;
using UnityEngine;

// 1. A ESTRUTURA DOS DADOS DA QUEST
// Usamos [System.Serializable] para podermos ver e editar as quests no Inspector da Unity!
[System.Serializable]
public class Quest
{
    [Tooltip("Um nome único para o código encontrar a quest (ex: matar_golem)")]
    public string questID;
    
    public string tituloDaQuest;
    
    [TextArea]
    public string descricao;

    [Header("Objetivos")]
    public int objetivoTotal; // Quantos monstros matar ou itens recolher?
    public int objetivoAtual; // Quantos a Fiorella já conseguiu?

    [Header("Status")]
    public bool emProgresso;
    public bool concluida;
}

// 2. O GESTOR CENTRAL DE QUESTS
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instancia;

    [Header("Banco de Missões")]
    [Tooltip("Crie e configure as suas quests aqui!")]
    public List<Quest> listaDeQuests = new List<Quest>();

    private void Awake()
    {
        // Padrão Singleton para o Diálogo e os Inimigos acharem o QuestManager facilmente
        if (Instancia == null)
        {
            Instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ==========================================
    // MÉTODOS PARA O SISTEMA DE DIÁLOGO USAR
    // ==========================================

    /// <summary>
    /// O Sistema de Diálogo chama isto quando o NPC termina de dar a missão.
    /// </summary>
    public void AceitarQuest(string id)
    {
        Quest quest = EncontrarQuest(id);
        if (quest != null && !quest.emProgresso && !quest.concluida)
        {
            quest.emProgresso = true;
            Debug.Log($"<color=cyan>[Quest] Missão Aceite: {quest.tituloDaQuest}</color>");
        }
    }

    /// <summary>
    /// O Sistema de Diálogo chama isto para verificar se a Fiorella tem a recompensa.
    /// </summary>
    public void TentarEntregarQuest(string id)
    {
        Quest quest = EncontrarQuest(id);
        if (quest != null && quest.emProgresso)
        {
            // Verifica se a Fiorella bateu a meta
            if (quest.objetivoAtual >= quest.objetivoTotal)
            {
                quest.emProgresso = false;
                quest.concluida = true;
                Debug.Log($"<color=green>[Quest] Missão Concluída: {quest.tituloDaQuest}! Entregando recompensa...</color>");
                
                // Aqui podemos adicionar a chamada para o PlayerProgression dar o Pólen/Item!
            }
            else
            {
                Debug.Log($"<color=yellow>[Quest] Ainda não terminaste! Faltam {quest.objetivoTotal - quest.objetivoAtual}.</color>");
            }
        }
    }

    // ==========================================
    // MÉTODOS PARA O MUNDO DO JOGO USAR (Inimigos, Itens)
    // ==========================================

    /// <summary>
    /// Quando um inimigo morre ou a Fiorella pega um item, chamamos isto para progredir!
    /// </summary>
    public void ProgredirQuest(string id, int quantidade)
    {
        Quest quest = EncontrarQuest(id);
        if (quest != null && quest.emProgresso && !quest.concluida)
        {
            quest.objetivoAtual += quantidade;
            Debug.Log($"[Quest] Progresso em {quest.tituloDaQuest}: {quest.objetivoAtual}/{quest.objetivoTotal}");

            if (quest.objetivoAtual >= quest.objetivoTotal)
            {
                Debug.Log($"<color=green>[Quest] Objetivo alcançado! Volta ao NPC para entregar.</color>");
                // Opcional: Tocar um som de "Tcharan!"
            }
        }
    }

    // Função auxiliar para procurar na lista
    public Quest EncontrarQuest(string id)
    {
        foreach (Quest q in listaDeQuests)
        {
            if (q.questID == id) return q;
        }
        Debug.LogWarning($"[Quest] A missão com o ID '{id}' não foi encontrada.");
        return null;
    }
}