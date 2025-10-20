using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
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

    [HideInInspector]
    public bool canMove = true; // Bloquea movimiento temporalmente

    private CharacterController controller;
    private PlayerInput playerInput;

    private Vector2 moveInput;
    private Vector3 moveDirection;

    private bool isDashing = false;
    private float dashTime = 0f;
    private float lastDashTime = -999f;

    private InputAction moveAction;
    private InputAction dashAction;

    private static PlayerMovement instance;

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
        if (!canMove) return;

        if (isDashing)
        {
            controller.Move(moveDirection * dashSpeed * Time.deltaTime);
            dashTime += Time.deltaTime;
            if (dashTime >= dashDuration)
                isDashing = false;
            return;
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

        RotateTowardsMouse();
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
        if (mainCamera == null || Mouse.current == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
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
}
