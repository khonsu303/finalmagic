using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Salud del Jugador")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI")]
    public UnityEngine.UI.Text healthText;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log("Jugador recibe " + damage + " de daño. Salud restante: " + currentHealth);

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "Salud: " + currentHealth + "/" + maxHealth;
        }
    }

    void Die()
    {
        Debug.Log("Jugador muerto!");
        // Aquí puedes agregar lógica de game over o respawn
    }
}