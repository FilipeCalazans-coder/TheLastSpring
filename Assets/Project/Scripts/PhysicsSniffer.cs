using UnityEngine;

public class PhysicsSniffer : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("COLISÃO COM: " + collision.gameObject.name + " na layer " + LayerMask.LayerToName(collision.gameObject.layer));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("TRIGGER COM: " + other.name + " na layer " + LayerMask.LayerToName(other.gameObject.layer));
    }
}