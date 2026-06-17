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

    // [SISTEMA DE I-FRAMES] Tempo que o inimigo fica invulnerável após um hit
    [Header("Invulnerabilidade (I-Frames)")]
    [SerializeField] private float tempoInvulneravel = 0.2f;
    private bool _estaInvulneravel = false;

    // Adicione uma variável para o nome do boss no topo do seu script de vida:
    public string nomeDoBoss = "O Invocador Raiz";
    public bool isBoss = false; // Marque isso como TRUE no inspector do seu boss

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
        _initialPosition = transform.position;
        _estaInvulneravel = false; // Garante que começa vulnerável

        if (BonfireManager.Instance != null)
        {
            BonfireManager.Instance.RegisterEnemy(this);
        }

        //if (isBoss && BossHealthBar.Instancia != null)
        //{
        //    BossHealthBar.Instancia.AtivarBarra(nomeDoBoss, startingHealth); 
        //}
    }

    public void TakeDamage(int damage)
    {
        // Se já estiver morto, ignora
        if (currentHealth <= 0) return;

        // [SISTEMA DE I-FRAMES] Se estiver no período de invulnerabilidade, ignora os múltiplos hits!
        if (_estaInvulneravel) return;

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

        // Tremor de câmera
        if (CameraShake.Instance != null)
        {
            if (currentHealth <= 0)
            {
                CameraShake.Instance.Shake(0.15f, 0.20f); 
            }
            else
            {
                CameraShake.Instance.Shake(0.1f, 0.12f);  
            }
        }

        if (isBoss && BossHealthBar.Instancia != null)
        {
            BossHealthBar.Instancia.AtualizarVida(currentHealth);
        }

        // [SISTEMA DE I-FRAMES] Inicia o período de invulnerabilidade
        StartCoroutine(RotinaInvulnerabilidade());

        StartCoroutine(CheckDetectDeathRoutine());
    }

    // ==========================================
    // [NOVO] Chamado pelo Controlador do Boss quando ele acorda
    // ==========================================
    public void AtivarBarraBoss()
    {
        if (isBoss && BossHealthBar.Instancia != null)
        {
            BossHealthBar.Instancia.AtivarBarra(nomeDoBoss, startingHealth);
        }
    }

    // ==========================================
    // [SISTEMA DE I-FRAMES] ROTINA DE TRAVA
    // ==========================================
    private IEnumerator RotinaInvulnerabilidade()
    {
        _estaInvulneravel = true;
        
        // Espera a fração de segundo definida no Inspector (padrão 0.2s)
        yield return new WaitForSeconds(tempoInvulneravel);
        
        _estaInvulneravel = false;
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
            if (PlayerController.Instance != null && !_hasDroppedSouls)
            {
                PlayerProgression playerXP = PlayerController.Instance.GetComponent<PlayerProgression>();
                if (playerXP != null)
                {
                    playerXP.AddSouls(polenDropped);
                    _hasDroppedSouls = true; 
                }
            }

            if (deathVFXPrefab) Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            
            if (Project.Scripts.Audio.AudioManager.Instance != null)
            {
                Project.Scripts.Audio.AudioManager.Instance.PlayAmbientMusic();
            }

            if (isBoss && BossHealthBar.Instancia != null)
            {
                BossHealthBar.Instancia.DesativarBarra();
            }

            SummonerBossController bossCtrl = GetComponent<SummonerBossController>();
            if (bossCtrl != null)
            {
                bossCtrl.OnBossDeath();
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void ResetEnemyToDefaultState()
    {
        transform.position = _initialPosition;
        currentHealth = startingHealth;
        _hasDroppedSouls = false;
        _estaInvulneravel = false; // [SISTEMA DE I-FRAMES] Zera a invulnerabilidade ao reviver

        if (_healthBar != null)
        {
            _healthBar.UpdateHealthBar(currentHealth, startingHealth);
        }

        GetComponent<EnemyIA>()?.ResetSpawnAnchor(_initialPosition);
        gameObject.SetActive(true);
    }
}