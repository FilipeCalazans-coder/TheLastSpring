using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int startingHealth = 3;
    [SerializeField] private GameObject deathVFXPrefab;
    [SerializeField] private float knockBackThrust = 15f;
    
    [Header("Resistência")]
    [SerializeField] private StatType defenseStatType;

    [Header("Recompensa")]
    [SerializeField] private int polenDropped = 50; 

    private EnemyHealthBar _healthBar;
    private int currentHealth;
    private KnockBack knockBack;
    private Flash flash;
    private BaseStats _myStats; 

    // ARQUITETURA DE RESPONDERS: Guarda dados de fábrica para o Respawn
    private Vector3 _initialPosition;
    private bool _hasDroppedSouls = false;

    private void Awake()
    {
        _healthBar = GetComponentInChildren<EnemyHealthBar>();
        flash = GetComponent<Flash>();
        knockBack = GetComponent<KnockBack>();
        _myStats = GetComponent<BaseStats>();

        if (_myStats == null) 
        {
            Debug.LogWarning($"BaseStats não encontrado no {gameObject.name}. O inimigo terá 0 de defesa.");
        }
    }

    private void Start()
    {
        currentHealth = startingHealth;
        // 1. Grava a posição exata do monstro no mapa
        _initialPosition = transform.position;

        // 2. REGISTRO AUTOMÁTICO: O monstro se apresenta ao cérebro da fogueira
        if (BonfireManager.Instance != null)
        {
            BonfireManager.Instance.RegisterEnemy(this);
        }
    }

    public void TakeDamage(int damage)
    {
        // Se já estiver morto (desativado ou no frame de morte), ignora dano residual
        if (currentHealth <= 0) return;

        float finalDamage = damage;

        if (_myStats != null && defenseStatType != null)
        {
            int defenseValue = _myStats.GetStatValue(defenseStatType);
            float mitigation = defenseValue / 100f;
            mitigation = Mathf.Clamp(mitigation, 0f, 0.9f);
            finalDamage = damage * (1f - mitigation);

            Debug.Log($"{gameObject.name} mitigou o dano de {damage} para {Mathf.RoundToInt(finalDamage)}");
        }

        currentHealth -= Mathf.RoundToInt(finalDamage);

        if (_healthBar != null)
        {
            _healthBar.UpdateHealthBar(currentHealth, startingHealth);
        }
        
        if(knockBack) knockBack.GetKnockedBack(PlayerController.Instance.transform, knockBackThrust);
        if(flash) StartCoroutine(flash.FlashRoutine());

        // ==========================================
        // CORREÇÃO: TREMOR DE CÂMERA AO CAUSAR DANO
        // ==========================================
        if (CameraShake.Instance != null)
        {
            // O sistema agora avalia a consequência do golpe:
            if (currentHealth <= 0)
            {
                // Golpe Fatal: Tremor mais forte e um pouco mais demorado
                CameraShake.Instance.Shake(0.15f, 0.20f); 
            }
            else
            {
                // Golpe Normal: Tremor rápido e leve
                CameraShake.Instance.Shake(0.1f, 0.12f);  
            }
        }

        StartCoroutine(CheckDetectDeathRoutine());
    }

    private IEnumerator CheckDetectDeathRoutine()
    {
        float waitTime = flash != null ? flash.GetRestoreMatTime() : 0.1f;
        yield return new WaitForSeconds(waitTime);
        DetectDeath();
    }

    public void DetectDeath()
    {
        if (currentHealth <= 0)
        {
            // Entrega das Almas apenas uma vez por ciclo de fogueira
            if (PlayerController.Instance != null && !_hasDroppedSouls)
            {
                PlayerProgression playerXP = PlayerController.Instance.GetComponent<PlayerProgression>();
                if (playerXP != null)
                {
                    playerXP.AddSouls(polenDropped);
                    _hasDroppedSouls = true; // Impede ganho infinito de Pólen
                }
            }

            if (deathVFXPrefab) Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            
            // NOVO: O inimigo morreu! Volta a tocar uma das músicas calmas!
            if (Project.Scripts.Audio.AudioManager.Instance != null)
            {
                Project.Scripts.Audio.AudioManager.Instance.PlayAmbientMusic();
            }
            
            // Desativamos o objeto para ele não sumir da hierarquia
            gameObject.SetActive(false);
        }
    }

    // INTERFACE DE COMANDO DA FOGUEIRA: Reseta o monstro para as configurações de fábrica
    public void ResetEnemyToDefaultState()
    {
        // 1. Move o monstro de volta para onde ele nasceu
        transform.position = _initialPosition;
        
        // 2. Restaura os status vitais
        currentHealth = startingHealth;
        _hasDroppedSouls = false;

        // 3. Atualiza a barra de vida para o visual cheio
        if (_healthBar != null)
        {
            _healthBar.UpdateHealthBar(currentHealth, startingHealth);
        }

        // 4. Se o monstro possui IA de Patrulha, força o script de IA a atualizar a sua posição âncora
        GetComponent<EnemyIA>()?.ResetSpawnAnchor(_initialPosition);

        // 5. Acorda o GameObject novamente na cena
        gameObject.SetActive(true);
    }
}