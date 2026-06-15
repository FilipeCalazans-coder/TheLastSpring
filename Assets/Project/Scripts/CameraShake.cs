using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3 _shakeOffset;
    private Coroutine _shakeCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Ativa o efeito de tremor na câmera.
    /// </summary>
    public void Shake(float duration, float magnitude)
    {
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
        }

        _shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Calcula um desvio aleatório para este frame
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // Guarda o desvio na variável
            _shakeOffset = new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reseta o desvio ao terminar o tempo
        _shakeOffset = Vector3.zero;
        _shakeCoroutine = null;
    }

    // A MÁGICA ACONTECE AQUI:
    // O LateUpdate roda depois que o Cinemachine já moveu a Main Camera.
    // Nós apenas adicionamos o nosso tremor por cima da posição calculada!
    private void LateUpdate()
    {
        if (_shakeOffset != Vector3.zero)
        {
            transform.position += _shakeOffset;
        }
    }
}