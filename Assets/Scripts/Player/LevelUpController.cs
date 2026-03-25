using UnityEngine;

public class LevelUpController : MonoBehaviour
{
    [SerializeField] private StatType attackStat;
    [SerializeField] private StatType defenseStat;

    private PlayerProgression _progression;

    private void Awake() => _progression = GetComponent<PlayerProgression>();

    private void Update()
    {
        // Aperte ESPAÇO para pagar almas e subir de Level Geral
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _progression.BuyLevel();
        }

        // Se tiver pontos, aperte 1 para colocar em Ataque
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _progression.AllocatePoint(attackStat);
        }

        // Se tiver pontos, aperte 2 para colocar em Defesa
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _progression.AllocatePoint(defenseStat);
        }
    }
}