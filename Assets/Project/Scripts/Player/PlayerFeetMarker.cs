using UnityEngine;

/// <summary>
/// Marcador que identifica o collider dos pés do Player.
/// Usado por hazards de chão (resina tóxica, água, espinhos, etc.)
/// para detectar quando o Player realmente "pisa" em cima de algo —
/// diferente do collider de corpo que detecta dano de inimigos.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PlayerFeetMarker : MonoBehaviour
{
    // Intencionalmente vazio — funciona apenas como tag de identificação via GetComponent.
}