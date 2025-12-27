using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Daño por contacto (temporal para pruebas)")]
    [SerializeField] private float damagePerContact = 10f;
    [SerializeField] private float damageCooldown = 1f; // Tiempo entre daños
    
    private float lastDamageTime = -999f;

    public static PlayerHealth instance;

    void Awake()
    {
        // Singleton: mantener la vida persistente entre escenas
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        // Si es la primera vez, inicializar la vida
        if (currentHealth == 0)
        {
            currentHealth = maxHealth;
        }
    }

    void Start()
    {
        Debug.Log($"[PlayerHealth] Vida actual: {currentHealth}/{maxHealth}");
    }

    void OnTriggerEnter(Collider other)
    {
        // Detectar colisión con enemigos
        if (other.CompareTag("enemy"))
        {
            TakeDamage(damagePerContact);
        }
    }

    void OnTriggerStay(Collider other)
    {
        // Daño continuo mientras está en contacto con el enemigo
        if (other.CompareTag("enemy"))
        {
            if (Time.time >= lastDamageTime + damageCooldown)
            {
                TakeDamage(damagePerContact);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (Time.time < lastDamageTime + damageCooldown)
            return;

        currentHealth -= damage;
        lastDamageTime = Time.time;

        Debug.Log($"[PlayerHealth] ¡Recibió {damage} de daño! Vida restante: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"[PlayerHealth] Curado {amount}. Vida actual: {currentHealth}/{maxHealth}");
    }

    void Die()
    {
        Debug.Log("[PlayerHealth] ¡El jugador ha muerto!");
        
        // Aquí puedes añadir lógica de muerte (reiniciar nivel, game over, etc.)
        // Por ahora solo reiniciamos la vida para pruebas
        currentHealth = maxHealth;
        
        // Ejemplo: recargar la escena actual
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Métodos públicos para acceder a la vida desde otros scripts
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;

    // Para resetear la vida (útil para testing)
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        Debug.Log("[PlayerHealth] Vida reseteada al máximo");
    }

    // Gizmo para visualizar el estado de vida en el editor
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // Verde si tiene más del 50% de vida, amarillo si está entre 25-50%, rojo si menos del 25%
            if (currentHealth / maxHealth > 0.5f)
                Gizmos.color = Color.green;
            else if (currentHealth / maxHealth > 0.25f)
                Gizmos.color = Color.yellow;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);
        }
    }
}