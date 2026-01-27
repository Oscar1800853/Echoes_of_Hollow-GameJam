using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManagerPersistance : MonoBehaviour
{
    public static PauseManagerPersistance Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!EsEscenaDeJuego(scene.name))
        {
            // Cerrar el menú de pausa si estaba abierto
            PauseManager pauseManager = GetComponentInChildren<PauseManager>();
            if (pauseManager != null)
            {
                pauseManager.ClosePauseMenu();
            }

            if (Instance == this)
            {
                Instance = null;
            }
            Destroy(gameObject);
        }
    }

    private bool EsEscenaDeJuego(string nombreEscena)
    {
        return nombreEscena.Contains("Nivel");
    }
}
