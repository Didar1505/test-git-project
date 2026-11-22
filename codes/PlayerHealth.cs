
using UnityEngine;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public TMP_Text healthText;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthText();
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
            return;

        currentHealth = Mathf.Max(0, currentHealth - damage);

        UpdateHealthText();

        if (currentHealth == 0)
        {
            Debug.Log("Игрок погиб");
            Destroy(gameObject);
        }
    }

    public bool Heal(int amount)
    {
        if (currentHealth <= 0 || currentHealth >= maxHealth)
            return false;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        UpdateHealthText();
        return true;
    }

    void UpdateHealthText()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " +
                currentHealth + " / " + maxHealth;
        }
    }
}
