using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Project.Scripts.Inventory;

public class BonfireManager : MonoBehaviour
{
    public static BonfireManager Instance { get; private set; }

    // ESTRUTURA DE CONTROLO: Guarda a referência de todos os scripts de vida dos monstros da fase
    private List<EnemyHealth> _registeredEnemies = new List<EnemyHealth>();

    public ChestInventory bonfireChest;

    [Header("Gerenciamento de Respawn")]
    [SerializeField] private Transform defaultSpawnPoint;

    [Header("Interface")]
    [SerializeField] private Project.Scripts.Hud.BonfireUI bonfireUI;
    private Vector3 _currentCheckpoint;

    // Removemos as variáveis globais nulas do Start para evitar referências perdidas

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        _currentCheckpoint = defaultSpawnPoint != null ? defaultSpawnPoint.position : Vector3.zero;
    }

    // Método chamado pelo BonfireTrigger quando o Player aperta "E"
    public void RestAtBonfire(Transform bonfireLocation)
    {
        Debug.Log("<color=#FFD700>Descansando no Brotinho de Resplendor...</color>");

        // 3. Reset do Mundo (Inimigos)
        RespawnEnemies();

        _currentCheckpoint = bonfireLocation.position;

        // CORREÇÃO: Busca o Player dinamicamente para garantir que a fogueira ache a Fiorella atual da cena
        PlayerController player = PlayerController.Instance;
        if (player != null)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            PlayerStamina stamina = player.GetComponent<PlayerStamina>();

            if (health != null) health.UpdateMaxHealth(); 
            if (stamina != null) stamina.ResetStamina();

            player.GetComponent<PlayerProgression>()?.SaveProgression();
        }

        if (bonfireUI != null)
        {
            bonfireUI.ShowMenu();
            Time.timeScale = 0f; // Pausa o mundo
        }
    }

    // CORREÇÃO CRUCIAL: Modificado para garantir que o teleporte aconteça mesmo se a referência antiga quebrou
    public void RespawnPlayerAtLastBonfire()
    {
        // Garante que o tempo do jogo voltou ao normal caso ela tenha morrido em transição de menus
        Time.timeScale = 1f; 

        PlayerController player = PlayerController.Instance;

        if (player != null)
        {
            // 1. Teleporta Fiorella para a coordenada do Checkpoint salvo
            player.transform.position = _currentCheckpoint;
            
            // 2. Reativa o corpo físico/colisores
            PlayerProgression prog = player.GetComponent<PlayerProgression>();
            if (prog != null) prog.ReEnablePlayerCollider();
            
            // 3. Recupera os status restaurando os scripts direto do player ativo
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            PlayerStamina stamina = player.GetComponent<PlayerStamina>();

            if (health != null) health.UpdateMaxHealth();
            if (stamina != null) stamina.ResetStamina();
            
            RespawnEnemies();

            // 4. GARANTE A REATIVAÇÃO DOS CONTROLES: Se o script de vida desativou o controller na morte, reativamos aqui!
            player.enabled = true;

            Debug.Log($"<color=green>Fiorella teleportada com sucesso para {_currentCheckpoint} e controles reativados!</color>");
        }
        else
        {
            // Log de erro explícito caso o Player não tenha a Tag correta ou o script não esteja na cena
            Debug.LogError("ERRO CRÍTICO: BonfireManager não conseguiu encontrar a instância do PlayerController na cena para realizar o Respawn!");
        }
    }

// Método que cada inimigo chamará no seu próprio Start() para entrar na lista
    public void RegisterEnemy(EnemyHealth enemy)
    {
        if (!_registeredEnemies.Contains(enemy))
        {
            _registeredEnemies.Add(enemy);
        }
    }

    // CORREÇÃO DA MÁGICA: Chamado dentro do método RestAtBonfire() e RespawnPlayerAtLastBonfire()
    private void RespawnEnemies()
    {
        Debug.Log($"<color=#00FF7F>Brotinho de Resplendor restaurando {_registeredEnemies.Count} inimigos comuns...</color>");

        foreach (EnemyHealth enemy in _registeredEnemies)
        {
            if (enemy != null)
            {
                // Executa a restauração completa dentro de cada monstro registrado
                enemy.ResetEnemyToDefaultState();
            }
        }
    }

    public void CloseMenuAndResumeGame()
    {
        Time.timeScale = 1f; 
    }
}