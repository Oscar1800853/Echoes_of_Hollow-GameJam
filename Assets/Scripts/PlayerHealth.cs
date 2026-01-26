using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Configuración de muerte")]
    [SerializeField] private float timeBeforeMenu = 3f;
    [SerializeField] private string menuSceneName = "menu";
    
    private float lastDamageTime = -999f;

    public static PlayerHealth instance;

    private PlayerHealthBar healthBarCache;

    private bool isDead = false;
 // Métodos públicos para acceder a la vida desde otros scripts
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;


    void Awake()
    {
        // Singleton: mantener la vida persistente entre escenas
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Si es la primera vez, inicializar la vida
        if (currentHealth == 0)
        {
            currentHealth = maxHealth;
        }
    }

    void Start()
    {
        Debug.Log($"[PlayerHealth] Vida actual: {currentHealth}/{maxHealth}");
        healthBarCache = FindFirstObjectByType<PlayerHealthBar>();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        lastDamageTime = Time.time;

        Debug.Log($"[PlayerHealth] ¡Recibió {damage} de daño! Vida restante: {currentHealth}/{maxHealth}");

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBarCache != null)
        {
            healthBarCache.SetValue(currentHealth);
        }
    }
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"[PlayerHealth] Curado {amount}. Vida actual: {currentHealth}/{maxHealth}");
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        
        Debug.Log("[PlayerHealth] ¡El jugador ha muerto!");

        StartCoroutine(DeathRoutine());
       
    }
   
    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(timeBeforeMenu);

        Debug.Log("[PlayerHealth] volviendo al menú...");
        
        if (instance == this)
        {
            instance = null;
        }

        Destroy(gameObject);

        SceneManager.LoadScene(menuSceneName);


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