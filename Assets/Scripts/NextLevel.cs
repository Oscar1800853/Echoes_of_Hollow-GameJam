using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Personaje")]
    public PlayerMovement player;                  // Script de movimiento del jugador
    public Animator playerAnimator;                // Animator del jugador
    public string transitionAnim = "Teleport";    // Trigger para animación de transición

    [Header("Escena")]
    public string nextSceneName;                   // Nombre de la escena a cargar (vacío si solo animación)

    [Header("Cámara")]
    public IsoCameraFollow cameraFollow;           // Script de cámara isométrica (debe persistir)

    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isTriggered && other.CompareTag("Player"))
        {
            isTriggered = true;

            // Desactivar movimiento del jugador actual
            if (player != null)
            {
                player.enabled = false;
                Debug.Log("Movimiento del jugador desactivado.");
            }

            // Lanzar animación si hay Animator
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger(transitionAnim);
                Debug.Log("Animación de transición iniciada: " + transitionAnim);
                StartCoroutine(WaitForAnimation());
            }
            else
            {
                Debug.Log("No se encontró animator. Cargando escena directamente...");
                StartCoroutine(LoadNextSceneOrReactivatePlayer());
            }
        }
    }

    private System.Collections.IEnumerator WaitForAnimation()
    {
        // Esperar un frame para asegurar que el trigger se registre
        yield return null;

        // Obtener duración de la animación activa
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        float animDuration = stateInfo.length;
        Debug.Log("Duración de la animación: " + animDuration + "s");

        // Esperar a que termine la animación
        yield return new WaitForSeconds(animDuration);

        // Proceder a cargar escena o reactivar jugador
        yield return StartCoroutine(LoadNextSceneOrReactivatePlayer());
    }

    private System.Collections.IEnumerator LoadNextSceneOrReactivatePlayer()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("Cargando escena: " + nextSceneName);

            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);

            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            // Esperar un frame extra para asegurar carga completa
            yield return null;

            // Buscar el nuevo jugador en la escena cargada
            GameObject newPlayer = GameObject.FindGameObjectWithTag("Player");
            if (newPlayer != null)
            {
                Debug.Log("Nuevo jugador encontrado en escena 2.");

                // Reasignar cámara al nuevo jugador
                if (cameraFollow != null)
                {
                    cameraFollow.target = newPlayer.transform;
                    Debug.Log("Cámara asignada al nuevo jugador.");
                }
                else
                {
                    Debug.LogWarning("cameraFollow es null, no se pudo asignar cámara.");
                }

                // Reactivar movimiento en el jugador o sus hijos
                PlayerMovement newMovement = newPlayer.GetComponent<PlayerMovement>();
                if (newMovement == null)
                {
                    newMovement = newPlayer.GetComponentInChildren<PlayerMovement>();
                }

                if (newMovement != null)
                {
                    newMovement.enabled = true;
                    Debug.Log("Movimiento del nuevo jugador activado.");
                }
                else
                {
                    Debug.LogWarning("No se encontró PlayerMovement en el jugador ni en sus hijos.");
                }
            }
            else
            {
                Debug.LogError("No se encontró un objeto con tag 'Player' en la escena 2.");
            }
        }
        else if (player != null)
        {
            player.enabled = true;
            Debug.Log("Movimiento reactivado sin cambiar de escena.");
        }
    }
}
