using UnityEngine;

// Este script é um sistema independente que destrói o objeto após um tempo,
// garantindo que a memória do jogo se mantém limpa.
public class VFXDestroyer : MonoBehaviour
{
    [Tooltip("Tempo em segundos que a animação demora a tocar até ao fim.")]
    public float tempoDeVida = 0.5f;

    private void Start()
    {
        // A função Destroy do Unity aceita um segundo parâmetro (tempo de atraso).
        // Assim que o objeto nasce (Start), ele agenda a sua própria destruição.
        Destroy(gameObject, tempoDeVida);
    }
}