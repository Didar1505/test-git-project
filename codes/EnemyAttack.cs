
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public int damage = 10;
    public float attackRate = 1f;

    private float nextAttackTime;

    public void TryAttack(PlayerHealth health)
    {
        if (health == null || attackRate <= 0f)
            return;

        if (Time.time < nextAttackTime)
            return;

        health.TakeDamage(damage);
        nextAttackTime = Time.time + 1f / attackRate;
    }
}
