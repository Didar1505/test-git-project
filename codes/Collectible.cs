
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Health Pickup")]
    public int healAmount = 25;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth health =
            other.GetComponent<PlayerHealth>();

        if (health == null)
            return;

        if (health.Heal(healAmount))
        {
            Debug.Log("Heart collected! +" + healAmount + " HP");
            Destroy(gameObject);
        }
    }
}
