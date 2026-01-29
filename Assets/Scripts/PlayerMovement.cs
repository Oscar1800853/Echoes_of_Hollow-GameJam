using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Rotación")]
    public Camera mainCamera;

    [Header("Animación")]
    [Tooltip("Velocidad de transición del blend tree (mayor = más suave)")]
    public float animationSmoothTime = 0.1f;

    [HideInInspector]
    public bool canMove = true; // Bloquea movimiento temporalmente

    private CharacterController controller;
    private PlayerInput playerInput;
    private Animator animator;

    private Vector2 moveInput;
    private Vector3 moveDirection;

    private bool isDashing = false;
    private float dashTime = 0f;
    private float lastDashTime = -999f;

    private InputAction moveAction;
    private InputAction dashAction;

    private static PlayerMovement instance;

    // Variables para suavizar las animaciones
    private float currentSpeed;
    private float speedVelocity;

    void Awake()
    {
        // Instancia única y persistente
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();

        if (mainCamera == null)
            mainCamera = Camera.main;

        var actions = playerInput.actions;
        if (actions != null)
        {
            moveAction = actions.FindAction("Move");
            dashAction = actions.FindAction("Dash");

            if (dashAction == null)
            {
                dashAction = new InputAction("Dash", InputActionType.Button);
                dashAction.AddBinding("<Keyboard>/space");
                dashAction.AddBinding("<Gamepad>/buttonSouth");
                dashAction.Enable();
            }
        }

        playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;
    }

    void OnEnable()
    {
        moveAction?.Enable();
        dashAction?.Enable();

        if (moveAction != null)
        {
            moveAction.performed += OnMove;
            moveAction.canceled += OnMove;
        }

        if (dashAction != null)
            dashAction.performed += OnDash;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        if (moveAction != null)
        {
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMove;
        }

        if (dashAction != null)
            dashAction.performed -= OnDash;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mainCamera = Camera.main;

        if (playerInput != null && !playerInput.enabled)
            playerInput.enabled = true;

        controller.enabled = false;
        controller.enabled = true;
    }

    void Update()
    {
        if (!canMove)
        {
            UpdateAnimator(0f);
            return;
        }

        if (isDashing)
        {
            controller.Move(moveDirection * dashSpeed * Time.deltaTime);
            dashTime += Time.deltaTime;
            
            // Actualizar animación de dash
            UpdateAnimator(1.5f); // Velocidad alta para dash
            animator.SetBool("IsDashing", true);
            
            if (dashTime >= dashDuration)
            {
                isDashing = false;
                animator.SetBool("IsDashing", false);
            }
            return;
        }

        // Verificar que mainCamera existe antes de usarla
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return; // Si aún no hay cámara, esperar al siguiente frame
        }

        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 inputDir = camForward * moveInput.y + camRight * moveInput.x;
        if (inputDir.magnitude > 1f)
            inputDir.Normalize();

        moveDirection = inputDir;
        controller.Move(moveDirection * speed * Time.deltaTime);

        // Actualizar animación de movimiento
        float targetSpeed = moveDirection.magnitude;
        UpdateAnimator(targetSpeed);

        RotateTowardsMouse();
    }

    void UpdateAnimator(float targetSpeed)
    {
        if (animator == null) return;

        // Suavizar el cambio de velocidad para transiciones más fluidas
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedVelocity, animationSmoothTime);
        
        // Actualizar el parámetro Speed del Blend Tree
        animator.SetFloat("Speed", currentSpeed);
        
        // Actualizar parámetro booleano para saber si está en movimiento
        animator.SetBool("IsMoving", currentSpeed > 0.01f);
    }

    void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void OnDash(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time >= lastDashTime + dashCooldown && moveDirection != Vector3.zero)
        {
            isDashing = true;
            dashTime = 0f;
            lastDashTime = Time.time;
        }
    }

    void RotateTowardsMouse()
    {
        // Verificación más robusta antes de acceder a Mouse.current
        if (mainCamera == null) return;
        
        // Verificar que Mouse.current existe y está disponible
        if (Mouse.current == null) return;
        
        // Verificar que la posición del mouse está disponible
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            Vector3 lookDir = hitPoint - transform.position;
            lookDir.y = 0f;

            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
            }
        }
    }

    public Vector3 GetMoveDirection() => moveDirection;
    
    public Animator GetAnimator() => animator;
}