using UnityEngine;

public class SoulDrop : MonoBehaviour
{
    private int _soulsStored;

    public void Initialize(int amount)
    {
        _soulsStored = amount;
        // Impede que a poça seja destruída quando a cena recarregar
        DontDestroyOnLoad(gameObject); 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerProgression progression = other.GetComponent<PlayerProgression>();
            if (progression != null)
            {
                progression.AddSouls(_soulsStored);
                // FEEDBACK VISUAL: Importante para o "suco" do jogo
                Debug.Log($"<color=yellow>Almas recuperadas: {_soulsStored}</color>");
                Destroy(gameObject); 
            }
        }
    }
}