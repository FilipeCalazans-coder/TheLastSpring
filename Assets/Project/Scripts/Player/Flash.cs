using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Flash : MonoBehaviour
{
    [SerializeField] private Material whiteFlashMat;
    [SerializeField] private float restoreDefaultMatTime = .2f;

    private SpriteRenderer[] spriteRenderers;
    private Material[] defaultMats;

    private void Awake()
    {
        // Pega o SpriteRenderer do pr\u00f3prio GameObject E de todos os filhos
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        defaultMats = new Material[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                defaultMats[i] = spriteRenderers[i].material;
        }
    }

    public float GetRestoreMatTime()
    {
        return restoreDefaultMatTime;
    }

    public IEnumerator FlashRoutine()
    {
        // Troca todos pra material de flash
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
                spriteRenderers[i].material = whiteFlashMat;
        }

        yield return new WaitForSeconds(restoreDefaultMatTime);

        // Restaura cada um pro seu pr\u00f3prio material original
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null && defaultMats[i] != null)
                spriteRenderers[i].material = defaultMats[i];
        }
    }
}