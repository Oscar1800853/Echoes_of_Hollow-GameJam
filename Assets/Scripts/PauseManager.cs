using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    private bool isPaused = false;

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
        // Asegurar que el menú esté cerrado al cargar una nueva escena
        if (isPaused)
        {
            ClosePauseMenu();
        }
    }

    void Update()
    {
        // Pulsar ESC abre o cierra
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ClosePauseMenu();   // ESC cierra si está abierto
            else
                OpenPauseMenu();    // ESC abre si está cerrado
        }
    }

    public void OpenPauseMenu()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ClosePauseMenu()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Se ha cerrado el juego");
    }
}
