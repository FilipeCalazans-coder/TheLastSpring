using UnityEngine;
using Project.Scripts.Inventory;
using Project.Scripts.Dialogue; // [NOVO] Precisamos disto para usar o DialogueData e DialogueManager

public class NPCQuestGiver : MonoBehaviour
{
    [Header("Configuração da Quest")]
    public string questID = "Artefato Antigo";
    public string idDoItemNecessario = "Artefato Antigo";

    // ==========================================
    // [NOVO] DIÁLOGOS DE REAÇÃO DA QUEST
    // ==========================================
    [Header("Diálogos da Quest")]
    [Tooltip("Diálogo quando a Fiorella aceita a missão pela primeira vez.")]
    public DialogueData dialogoAceitarMissao;
    
    [Tooltip("Diálogo quando a Fiorella fala com ele mas AINDA NÃO tem o item.")]
    public DialogueData dialogoAindaProcurando;
    
    [Tooltip("Diálogo de SUCESSO quando a Fiorella entrega o item!")]
    public DialogueData dialogoEntregarItem;
    
    [Tooltip("Diálogo padrão para quando a missão já foi concluída no passado.")]
    public DialogueData dialogoJaConcluida;

    public void InteragirComNPC()
    {
        Quest minhaQuest = QuestManager.Instancia.EncontrarQuest(questID);

        if (minhaQuest == null) 
        {
            Debug.LogError("[NPC] Ops! Esqueceste-te de criar esta quest no QuestManager.");
            return;
        }

        // CENÁRIO A: A missão já foi concluída no passado.
        if (minhaQuest.concluida)
        {
            TocarDialogo(dialogoJaConcluida);
            return;
        }

        // CENÁRIO B: A Fiorella ainda não aceitou a missão.
        if (!minhaQuest.emProgresso && !minhaQuest.concluida)
        {
            QuestManager.Instancia.AceitarQuest(questID);
            TocarDialogo(dialogoAceitarMissao);
            return;
        }

        // CENÁRIO C: A missão está em progresso. O NPC vai checar o inventário!
        if (minhaQuest.emProgresso)
        {
            bool temOItem = VerificarInventarioDaFiorella(idDoItemNecessario);

            if (temOItem)
            {
                // 1. Remove o item do inventário
                RemoverItemDaFiorella(idDoItemNecessario);

                // 2. Conclui a missão
                QuestManager.Instancia.ProgredirQuest(questID, 1);
                QuestManager.Instancia.TentarEntregarQuest(questID);

                // 3. Toca o diálogo de SUCESSO!
                TocarDialogo(dialogoEntregarItem);
            }
            else
            {
                // Toca o diálogo pedindo para continuar a procurar
                TocarDialogo(dialogoAindaProcurando);
            }
        }
    }

    // ==========================================
    // [NOVO] FUNÇÃO AUXILIAR PARA TOCAR O DIÁLOGO
    // ==========================================
    private void TocarDialogo(DialogueData dialogo)
    {
        if (dialogo != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogo);
        }
        else
        {
            Debug.LogWarning("[NPC] Nenhum diálogo configurado para este estado da Quest!");
        }
    }

    // ==========================================
    // INTEGRAÇÃO COM O SEU SISTEMA DE INVENTÁRIO
    // ==========================================
    private bool VerificarInventarioDaFiorella(string nomeDoItem)
    {
        InventoryManager inv = Object.FindFirstObjectByType<InventoryManager>();
        if (inv != null)
        {
            foreach (ItemData item in inv.items)
            {
                if (item != null && item.itemName == nomeDoItem) return true;
            }
        }
        return false;
    }

    private void RemoverItemDaFiorella(string nomeDoItem)
    {
        InventoryManager inv = Object.FindFirstObjectByType<InventoryManager>();
        if (inv != null)
        {
            for (int i = 0; i < inv.items.Count; i++)
            {
                if (inv.items[i] != null && inv.items[i].itemName == nomeDoItem)
                {
                    inv.items.RemoveAt(i);
                    InventoryUI ui = Object.FindFirstObjectByType<InventoryUI>();
                    if (ui != null) ui.UpdateUI();
                    break; 
                }
            }
        }
    }
}