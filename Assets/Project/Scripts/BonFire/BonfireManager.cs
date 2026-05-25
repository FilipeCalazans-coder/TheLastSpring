using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Project.Scripts.Inventory;

public class BonfireManager : MonoBehaviour
{
    public static BonfireManager Instance { get; private set; }

    public ChestInventory bonfireChest;

    [Header("Gerenciamento de Respawn")]
    [SerializeField] private Transform defaultSpawnPoint;

    [Header("Interface")]
    [SerializeField] private Project.Scripts.Hud.BonfireUI bonfireUI;
    private Vector3 _currentCheckpoint;

    private PlayerHealth _playerHealth;
    private PlayerStamina _playerStamina; // Adicionei suporte a estamina
    private PlayerController _playerController;

    private void Awake()
    {
        // Padrão Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // O Manager sobrevive entre cenas
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Define o checkpoint inicial caso o jogador morra antes de achar uma fogueira
        _currentCheckpoint = defaultSpawnPoint != null ? defaultSpawnPoint.position : Vector3.zero;
    }

    private void Start()
    {
        // Busca as referências do Player na cena
        if (PlayerController.Instance != null)
        {
            _playerController = PlayerController.Instance;
            _playerHealth = _playerController.GetComponent<PlayerHealth>();
            _playerStamina = _playerController.GetComponent<PlayerStamina>();
        }
    }

    // Método chamado pelo BonfireTrigger quando o Player aperta "E"
    public void RestAtBonfire(Transform bonfireLocation)
    {
        Debug.Log("<color=#FFD700>Descansando no Brotinho de Resplendor...</color>");

        // 1. Atualiza o Checkpoint para a posição desta fogueira
        _currentCheckpoint = bonfireLocation.position;

        // 2. Cura Total (HP e Estamina)
        if (_playerHealth != null)
        {
            // O UpdateMaxHealth já recarrega a vida para o máximo baseado na Vitalidade
            _playerHealth.UpdateMaxHealth(); 
        }
        
        if (_playerStamina != null)
        {
            _playerStamina.ResetStamina();
        }

        // 3. Reset do Mundo (Inimigos)
        RespawnEnemies();

        // 4. Salva o Jogo (Integração com o seu PlayerProgression)
        if (_playerController != null)
        {
            _playerController.GetComponent<PlayerProgression>()?.SaveProgression();
        }
        
        // 5. Abre o Menu e pausa o jogo
        if (bonfireUI != null)
        {
            bonfireUI.ShowMenu();
            Time.timeScale = 0f; // Congela os inimigos e o jogador enquanto o menu está aberto
        }
    }

    public void RespawnPlayerAtLastBonfire()
    {
        if (_playerController != null)
        {
            // 1. Teleporta
            _playerController.transform.position = _currentCheckpoint;
            
            // 2. REATIVAR COLLIDER (A correção que faltava)
            PlayerProgression prog = _playerController.GetComponent<PlayerProgression>();
            if (prog != null) prog.ReEnablePlayerCollider();
            
            // 3. Restaura vida e estamina
            _playerHealth?.UpdateMaxHealth();
            _playerStamina?.ResetStamina();
            
            Debug.Log("Fiorella renasceu com o corpo físico reativado.");
        }
    }

    private void RespawnEnemies()
    {
        // Aqui você pode adicionar a lógica para reviver inimigos comuns.
        // A forma mais comum de fazer isso na Unity é guardar a posição inicial
        // de cada inimigo em um array no Start() da cena, e agora instanciar o Prefab neles.
        Debug.Log("Inimigos comuns foram revividos pelo descanso na Fogueira.");
    }

    public void CloseMenuAndResumeGame()
    {
        Time.timeScale = 1f; // Descongela o jogo
    }
}