using UnityEngine;
using UnityEngine.SceneManagement;

public class IsoCameraFollow : MonoBehaviour
{
    public Transform target;        // El personaje que la cámara debe seguir
    public Vector3 offset = new Vector3(-10, 10, -10); // Posición isométrica relativa
    public float smoothSpeed = 5f;  // Suavidad del seguimiento

    private static IsoCameraFollow instance;

    void Awake()
    {
        // Singleton: asegura que solo haya una instancia de la cámara
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Suscribirse al evento de carga de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        FindPlayer();
    }

    void OnDestroy()
    {
        // Desuscribirse del evento al destruir
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Se llama cada vez que se carga una nueva escena
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Buscar al jugador en la nueva escena
        FindPlayer();
    }

    void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            Debug.Log($"[IsoCameraFollow] Jugador encontrado: {player.name}");
        }
        else
        {
            Debug.LogWarning("[IsoCameraFollow] No se encontró ningún objeto con tag 'Player'");
        }
    }

    void LateUpdate()
    {
        // Si no hay target, intentar buscarlo
        if (target == null)
        {
            FindPlayer();
            return;
        }

        // Posición deseada = personaje + offset
        Vector3 desiredPosition = target.position + offset;

        // Suavizado para que la cámara no se mueva bruscamente
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;

        // Apuntar hacia el personaje
        transform.LookAt(target);
    }
}
