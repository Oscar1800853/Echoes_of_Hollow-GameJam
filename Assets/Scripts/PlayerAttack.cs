using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Combos de ataque")]
    public List<ComboAttack> comboList = new List<ComboAttack>()
    {
        new ComboAttack() { nombre = "Corte rápido", damage = 15, range = 2f, angle = 60f, duration = 0.4f },
        new ComboAttack() { nombre = "Estocada", damage = 25, range = 2.5f, angle = 45f, duration = 0.6f },
        new ComboAttack() { nombre = "Golpe final", damage = 40, range = 3f, angle = 90f, duration = 0.8f }
    };

    [Header("Ajustes generales")]
    public float comboResetTime = 1f;
    public LayerMask enemyLayer; // si lo dejas en 0, el script buscará en todas las capas
    [Tooltip("Origen del ataque (por ejemplo punta de espada). Si es null se usa el transform del jugador.")]
    public Transform attackOrigin;

    private int comboIndex = 0;
    private bool isAttacking = false;
    private float lastAttackTime;
    private PlayerMovement movement;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        if (attackOrigin == null) attackOrigin = transform;
    }

    void Update()
    {
        if (Time.time - lastAttackTime > comboResetTime) comboIndex = 0;

        if (Input.GetMouseButtonDown(0) && !isAttacking)
            StartCoroutine(PerformAttack());
    }

    IEnumerator PerformAttack()
    {
        if (comboList == null || comboList.Count == 0)
        {
            Debug.LogWarning("No hay ataques en la lista de combos.");
            yield break;
        }

        isAttacking = true;
        ComboAttack currentAttack = comboList[comboIndex];
        Debug.Log("Ataque {comboIndex + 1}: {currentAttack.nombre} | Daño {currentAttack.damage}");

        ExecuteAttack(currentAttack);

        yield return new WaitForSeconds(currentAttack.duration);

        comboIndex++;
        if (comboIndex >= comboList.Count) comboIndex = 0;

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    void ExecuteAttack(ComboAttack attack)
    {
        Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position;

        // Si enemyLayer está vacío (valor 0) usamos OverlapSphere sin filtro para no excluir por error
        Collider[] hitColliders = (enemyLayer.value != 0)
            ? Physics.OverlapSphere(origin, attack.range, enemyLayer)
            : Physics.OverlapSphere(origin, attack.range);

        Debug.Log("[PlayerAttack] OverlapSphere encontró {hitColliders.Length} colliders para '{attack.nombre}' (range={attack.range}).");

        if (hitColliders.Length == 0)
        {
            Debug.LogWarning("[PlayerAttack] No se detectaron colliders. Revisa enemyLayer, colliders y que estén activos.");
        }

        foreach (Collider col in hitColliders)
        {
            // Ignorar cualquier collider que pertenezca al mismo root que el jugador (por ejemplo colliders del propio jugador)
            if (col.transform.root == transform.root)
            {
                //Debug.Log($"[PlayerAttack] Ignorando collider propio: {col.name}");
                continue;
            }
            // Usar el punto más cercano del collider para calcular el ángulo/range real
            Vector3 closestPoint = col.ClosestPoint(origin);
            Vector3 dirToTarget = (closestPoint - origin).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);

            if (angleToTarget <= attack.angle / 2f)
            {
                Enemy enemy = GetEnemyFromCollider(col);
                if (enemy != null)
                {
                    enemy.TakeDamage(attack.damage);
                    Debug.Log("Golpeó a {enemy.name} con {attack.nombre} (daño={attack.damage}, angle={angleToTarget:F1})");
                }
                else
                {
                    Debug.LogWarning("[PlayerAttack] Collider '{col.name}' (capa={LayerMask.LayerToName(col.gameObject.layer)}) no tiene componente Enemy (ni en parents/children).");
                }
            }
            else
            {
                Debug.Log("[PlayerAttack] '{col.name}' fuera de ángulo (angle={angleToTarget:F1}, requerido<={attack.angle/2f})");
            }
        }
    }

    private Enemy GetEnemyFromCollider(Collider col)
    {
        if (col == null) return null;
        Enemy e = col.GetComponent<Enemy>();
        if (e != null) return e;
        e = col.GetComponentInParent<Enemy>();
        if (e != null) return e;
        e = col.GetComponentInChildren<Enemy>();
        return e;
    }

    void OnDrawGizmosSelected()
    {
        if (comboList == null || comboList.Count == 0) return;
        ComboAttack currentAttack = comboList[Mathf.Clamp(comboIndex, 0, comboList.Count - 1)];
        Gizmos.color = Color.red;
        Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position;
        Vector3 forward = transform.forward;
        Quaternion left = Quaternion.Euler(0, -currentAttack.angle / 2f, 0);
        Quaternion right = Quaternion.Euler(0, currentAttack.angle / 2f, 0);
        Vector3 leftDir = left * forward;
        Vector3 rightDir = right * forward;
        Gizmos.DrawRay(origin, leftDir * currentAttack.range);
        Gizmos.DrawRay(origin, rightDir * currentAttack.range);
        Gizmos.DrawWireSphere(origin, currentAttack.range);
    }
}