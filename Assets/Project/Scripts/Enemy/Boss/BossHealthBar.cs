using UnityEngine;
using UnityEngine.UI; // Necessário para controlar o Slider
using TMPro; // Necessário se estiver usando TextMeshPro para o texto do nome do boss

public class BossHealthBar : MonoBehaviour
{
    // O Singleton permite que qualquer script no jogo encontre esta barra facilmente
    public static BossHealthBar Instancia;

    [Header("Elementos Visuais")]
    [Tooltip("O objeto pai que contém a barra e o texto (para ligar/desligar tudo de uma vez).")]
    public GameObject painelDaBarra;
    
    [Tooltip("A barra deslizante que mostra a vida.")]
    public Slider sliderDeVida;
    
    [Tooltip("O texto que mostra o nome do boss.")]
    public TextMeshProUGUI textoNomeBoss; // Se usar TextMeshPro, mude "Text" para "TextMeshProUGUI"

    private void Awake()
    {
        // Configura o Singleton
        if (Instancia == null)
        {
            Instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // Garante que a barra comece invisível
        DesativarBarra();
    }

    /// <summary>
    /// Mostra a barra na tela e configura a vida máxima.
    /// </summary>
    public void AtivarBarra(string nomeDoBoss, int vidaMaxima)
    {
        painelDaBarra.SetActive(true);
        
        if (textoNomeBoss != null)
        {
            textoNomeBoss.text = nomeDoBoss;
        }

        if (sliderDeVida != null)
        {
            sliderDeVida.maxValue = vidaMaxima;
            sliderDeVida.value = vidaMaxima;
        }
    }

    /// <summary>
    /// Atualiza o preenchimento da barra.
    /// </summary>
    public void AtualizarVida(int vidaAtual)
    {
        if (sliderDeVida != null)
        {
            sliderDeVida.value = vidaAtual;
        }
    }

    /// <summary>
    /// Esconde a barra da tela.
    /// </summary>
    public void DesativarBarra()
    {
        if (painelDaBarra != null)
        {
            painelDaBarra.SetActive(false);
        }
    }
}