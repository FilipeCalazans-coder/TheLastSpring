using UnityEngine;

public class SoulDrop : MonoBehaviour
{
    private int _soulsStored;

    public void Initialize(int amount)
    {
        _soulsStored = amount;
        // Se a Fiorella morrer de novo, o script PlayerProgression já destrói a poça antiga
        // então o DontDestroyOnLoad é seguro aqui para manter a poça entre recarregamentos de cena
        DontDestroyOnLoad(gameObject); 
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerProgression progression = other.GetComponent<PlayerProgression>();
            if (progression != null)
            {
                // 1. Devolve as almas para a variável local do player
                progression.AddSouls(_soulsStored);
                
                // 2. FUNDAMENTAL: Avisa ao GameData que agora o player é dono dessas almas
                GameData.CurrentSouls = progression.GetCurrentSouls();
                
                // 3. Salva o progresso para garantir que a coleta foi registrada
                progression.SaveProgression();

                Debug.Log($"<color=cyan>Almas recuperadas: {_soulsStored}. Total agora: {GameData.CurrentSouls}</color>");
                
                Destroy(gameObject); 
            }
        }
    }
}