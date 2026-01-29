using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class FinalBoss : MonoBehaviour
{
    [Header("Atributos del jefe final")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("IA")]
    public NavMeshAgent agent;
    public Transform player;
    public LayerMask whatIsGround;
    public LayerMask whatIsPlayer;


    //movement


    //ataque
    public float timeBetweenAttacks = 8f;
    public float damageAmount = 10f;
    public bool isAttacking;
    public float attackDelay;


    //estados
    public float sightRange = 10f, attackRange = 2f;
    private bool playerInSightRange, playerInAttackRange;
    public float timeBeforeDying = 3f;
    private bool isDead;

    //Animator animator

    private void Awake()
    {
        //buscar al jugador si no esta asignado
        if (player == null)
        {
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogError("No se encuentra al jugador");
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.LogError("No hay NavMeshAgent asignado al boss final");

        //animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (isDead) return;

        //comprobar si el jugador esta en rango
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        //Logica estados
        if (playerInSightRange && !playerInAttackRange)
        {
            ChasePlayer();
        }
        else if (playerInAttackRange)
        {
            AttackPlayer();
        }
    }

    //Seguir al jugador
    private void ChasePlayer()
    {
        agent.SetDestination(player.position);
        //animator.SetFloat("perseguir",0.1f);
    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);
        //animator.SetFloat("moverse", 0f);

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

        //animator.SetTrigger("atacar");

        yield return new WaitForSeconds(attackDelay);

        Debug.Log("el jefe ataca");

        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }

        yield return new WaitForSeconds(timeBetweenAttacks);

        isAttacking = false;
    }

    public void TakeDamange(int amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + "recibio" + amount + "de daño. Vida restante" + currentHealth);

        if (currentHealth <= 0)
            Die();
    }


    //muerte

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        agent.enabled = false;
        GetComponent<Collider>().enabled = false;

        StartCoroutine(DyingCoroutine());
    }

    private IEnumerator DyingCoroutine()
    {
        //animator.SetTrigger ("morir");

        yield return new WaitForSeconds(timeBeforeDying);

        SceneManager.LoadScene("CinemFinal");
    }

    // Visualizar rangos en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
