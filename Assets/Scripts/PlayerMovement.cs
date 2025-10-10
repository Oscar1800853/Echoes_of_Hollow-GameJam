using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Rotación")]
    public Camera mainCamera; // Asigna tu cámara en el inspector

    private CharacterController controller;
    private Vector3 moveDirection;
    private bool isDashing = false;
    private float dashTime = 0f;
    private float lastDashTime = -999f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    void Update()
    {
        if (isDashing)
        {
            controller.Move(moveDirection * dashSpeed * Time.deltaTime);
            dashTime += Time.deltaTime;
            if (dashTime >= dashDuration)
                isDashing = false;
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // --- Movimiento relativo a cámara ---
        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        moveDirection = (camForward * vertical + camRight * horizontal);
        if (moveDirection.magnitude > 1f)
            moveDirection.Normalize();

        controller.Move(moveDirection * speed * Time.deltaTime);

        // --- Rotación hacia el ratón ---
        RotateTowardsMouse();

        // --- Dash ---
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= lastDashTime + dashCooldown && moveDirection != Vector3.zero)
        {
            isDashing = true;
            dashTime = 0f;
            lastDashTime = Time.time;
        }
    }

    void RotateTowardsMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
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

    public Vector3 GetMoveDirection()
    {
        return moveDirection;
    }
}
