using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Combos de ataque")]
    public List<ComboAttack> comboList = new List<ComboAttack>()
    {
        new ComboAttack()
        {
            nombre = "Corte rápido",
            damage = 15,
            range = 2f,
            angle = 60f,
            duration = 0.4f
        },
        new ComboAttack()
        {
            nombre = "Estocada",
            damage = 25,
            range = 2.5f,
            angle = 45f,
            duration = 0.6f
        },
        new ComboAttack()
        {
            nombre = "Golpe final",
            damage = 40,
            range = 3f,
            angle = 90f,
            duration = 0.8f
        }
    };

    [Header("Ajustes generales")]
    public float comboResetTime = 1f;
    public LayerMask enemyLayer;

    private int comboIndex = 0;
    private bool isAttacking = false;
    private float lastAttackTime;
    private PlayerMovement movement;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        if (Time.time - lastAttackTime > comboResetTime)
            comboIndex = 0;

        if (Input.GetMouseButtonDown(0) && !isAttacking)
            StartCoroutine(PerformAttack());
    }

    IEnumerator PerformAttack()
    {
        if (comboList.Count == 0)
        {
            Debug.LogWarning("⚠️ No hay ataques en la lista de combos.");
            yield break;
        }

        isAttacking = true;
        ComboAttack currentAttack = comboList[comboIndex];
        Debug.Log($"➡️ Ataque {comboIndex + 1}: {currentAttack.nombre} | Daño {currentAttack.damage}");

        ExecuteAttack(currentAttack);

        yield return new WaitForSeconds(currentAttack.duration);

        comboIndex++;
        if (comboIndex >= comboList.Count)
            comboIndex = 0;

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    void ExecuteAttack(ComboAttack attack)
    {
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, attack.range, enemyLayer);

        foreach (Collider enemyCollider in hitEnemies)
        {
            Vector3 directionToEnemy = (enemyCollider.transform.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToEnemy) <= attack.angle / 2f)
            {
                Enemy enemy = enemyCollider.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(attack.damage);
                    Debug.Log($"💥 Golpeó a {enemy.name} con {attack.nombre}");
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (comboList == null || comboList.Count == 0) return;

        ComboAttack currentAttack = comboList[Mathf.Clamp(comboIndex, 0, comboList.Count - 1)];

        Gizmos.color = Color.red;
        Vector3 forward = transform.forward;
        Quaternion leftRayRotation = Quaternion.Euler(0, -currentAttack.angle / 2f, 0);
        Quaternion rightRayRotation = Quaternion.Euler(0, currentAttack.angle / 2f, 0);
        Vector3 leftRayDirection = leftRayRotation * forward;
        Vector3 rightRayDirection = rightRayRotation * forward;
        Gizmos.DrawRay(transform.position, leftRayDirection * currentAttack.range);
        Gizmos.DrawRay(transform.position, rightRayDirection * currentAttack.range);
        Gizmos.DrawWireSphere(transform.position, currentAttack.range);
    }
}
