using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Atributos del enemigo")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Feedback opcional")]
    public GameObject deathEffect; // Part�culas o animaci�n al morir

    void Start()
    {
        currentHealth = maxHealth;
    }

    // Funci�n que recibe da�o
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " recibie " + amount + " de danio. Vida restante: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " ha muerto.");

        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

}
