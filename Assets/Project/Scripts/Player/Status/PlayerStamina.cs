using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerStamina : MonoBehaviour
{
    [Header("Configurações")]
    [SerializeField] private StatType enduranceStat;
    [SerializeField] private float regenRate = 20f; // Quanto recupera por segundo
    [SerializeField] private float regenDelay = 1f; // Quanto tempo espera após usar para começar a recuperar

    [Header("UI")]
    [SerializeField] private Slider staminaBar;

    private float _currentStamina;
    private float _maxStamina;
    private BaseStats _stats;
    private Coroutine _regenCoroutine;

    private void Awake() => _stats = GetComponent<BaseStats>();

    private void Start()
    {
        UpdateMaxStamina();
        _currentStamina = _maxStamina;
        UpdateUI();
    }

    public void SetStaminaBar(Slider bar)
    {
        staminaBar = bar;
        UpdateUI();
    }

    public void UpdateMaxStamina()
    {
        // 1 ponto de Resistência = 10 de Estamina
        _maxStamina = _stats.GetStatValue(enduranceStat) * 10f;
        UpdateUI();
    }

    // Método principal para outras ações chamarem (Dash, Ataque)
    public bool TrySpendStamina(float amount)
    {
        if (_currentStamina >= amount)
        {
            _currentStamina -= amount;
            UpdateUI();

            // Reinicia a regeneração
            if (_regenCoroutine != null) StopCoroutine(_regenCoroutine);
            _regenCoroutine = StartCoroutine(RegenStaminaRoutine());
            
            return true; // Conseguiu gastar
        }

        Debug.Log("Estamina insuficiente!");
        return false; // Não tem energia suficiente
    }

    private IEnumerator RegenStaminaRoutine()
    {
        yield return new WaitForSeconds(regenDelay);

        while (_currentStamina < _maxStamina)
        {
            _currentStamina += regenRate * Time.deltaTime;
            _currentStamina = Mathf.Min(_currentStamina, _maxStamina);
            UpdateUI();
            yield return null;
        }
    }

    private void UpdateUI()
    {
        if (staminaBar != null)
        {
            staminaBar.maxValue = _maxStamina;
            staminaBar.value = _currentStamina;
        }
    }
}