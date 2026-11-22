using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerEnemyCollision : MonoBehaviour
{
    private PlayerHealth health;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        EnemyAttack enemy =
            hit.collider.GetComponentInParent<EnemyAttack>();

        if (enemy != null)
        {
            enemy.TryAttack(health);
        }
    }
}
