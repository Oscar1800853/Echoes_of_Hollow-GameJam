using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [Header("Atributos del enemigo")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Feedback opcional")]
    //public GameObject deathEffect;

    [Header("IA")]
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;

    // Patrulla
    public Vector3 walkPoint;
    private bool walkPointSet;
    public float walkPointRange;

    // Ataque
    public float timeBetweenAttacks = 8f;
    private bool isAttacking;

    // Estados
    public float sightRange = 10f, attackRange = 2f;
    private bool playerInSightRange, playerInAttackRange;

    Animator animator;

    private void Awake()
    {
        // Busca el jugador automáticamente si no está asignado
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogError("No se encontró ningún GameObject llamado 'Player'. Asignalo en el Inspector.");
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.LogError("El Enemy necesita un NavMeshAgent. Agrega uno al GameObject.");
        
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        // Comprueba si el jugador está en rango
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        // Lógica de estados
        if (!playerInSightRange && !playerInAttackRange)
        {
            Patroling();
        }
        else if (playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }
        else if (playerInAttackRange)
        {
            AttackPlayer();
        }
    }

    #region Patrullaje
    private void Patroling()
    {
        if (!walkPointSet) SearchWalkPoint();

        if (walkPointSet)
            agent.SetDestination(walkPoint);

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        // Llegó al punto
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
        animator.SetFloat("moverse", 0.1f);
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        // Verifica que el punto esté sobre el suelo
        if (Physics.Raycast(walkPoint, Vector3.down, 2f, whatIsGround))
            walkPointSet = true;
    }
    #endregion

    #region Persecución y Ataque
    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
        animator.SetFloat("moverse", 0.1f);
    }

    private void AttackPlayer()
    {
        // El enemigo se queda quieto
        agent.SetDestination(transform.position);
        animator.SetFloat("moverse", 0f);

        // Mira al jugador
        Vector3 lookPos = player.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        if (!isAttacking)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        animator.SetTrigger("atacar");

        Debug.Log("¡Ataca!");
        // Aquí pondrías la lógica de daño al jugador

        yield return new WaitForSeconds(timeBetweenAttacks);

        isAttacking = false;
    }
    #endregion

    #region Daño y Muerte
    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " recibió " + amount + " de daño. Vida restante: " + currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " ha muerto.");

        /*if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);*/

        Destroy(gameObject);
    }
    #endregion

    // Visualizar rangos en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

