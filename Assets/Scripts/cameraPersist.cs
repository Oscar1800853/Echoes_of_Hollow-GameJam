using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class CameraPersist : MonoBehaviour
{
    public static CameraPersist instance;

    [Header("Configuración")]
    public string escenaActivacion = "Nivel1Inicio";
    public CinemachineCamera vcam;

    private bool seguimientoActivo = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        VerificarEscenaActual();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        VerificarEscenaActual();
    }

    void VerificarEscenaActual()
    {
        string escenaActual = SceneManager.GetActiveScene().name;

        if (escenaActual == escenaActivacion || seguimientoActivo)
        {
            seguimientoActivo = true;
            AsignarJugador();
        }
        else
        {
            if (vcam != null)
                vcam.Follow = null;
        }
    }

    void AsignarJugador()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && vcam != null)
        {
            vcam.Follow = player.transform;
            Debug.Log("[CameraPersist] Siguiendo al jugador");
        }
        else
        {
            Debug.LogWarning("[CameraPersist] No se encontró el jugador o la vcam");
        }
    }
}

