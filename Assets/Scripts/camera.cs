using UnityEngine;

public class IsoCameraFollow : MonoBehaviour
{
    public Transform target;        // El personaje que la cámara debe seguir
    public Vector3 offset = new Vector3(-10, 10, -10); // Posición isométrica relativa
    public float smoothSpeed = 5f;  // Suavidad del seguimiento

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
    }
    void LateUpdate()
    {
        if (target == null) return;

        // Posición deseada = personaje + offset
        Vector3 desiredPosition = target.position + offset;

        // Suavizado para que la cámara no se mueva bruscamente
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;

        // Apuntar hacia el personaje
        transform.LookAt(target);
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

}
