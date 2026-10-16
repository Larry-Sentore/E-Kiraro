using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] public float movementSpeed = 5f;
    [SerializeField] public float mouseSensitivity = 0.1f;
    [SerializeField] public float maximumLookAngle = 85f;
    [SerializeField] private float gravity = -20f;

    private InputAction moveAction;
    private InputAction lookAction;
    private CharacterController characterController;
    private Transform cameraTransform;
    private float yaw;
    private float pitch;
    private float verticalVelocity;
    private bool cursorLocked;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        Camera playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            cameraTransform = playerCamera.transform;
        }

        yaw = transform.eulerAngles.y;
        pitch = cameraTransform != null ? cameraTransform.localEulerAngles.x : 0f;
        if (pitch > 180f)
        {
            pitch -= 360f;
        }

        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        lookAction = new InputAction("Look", InputActionType.Value, "<Mouse>/delta");
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        SetCursorLocked(true);
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        SetCursorLocked(false);
    }

    private void OnDestroy()
    {
        moveAction.Dispose();
        lookAction.Dispose();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetCursorLocked(!cursorLocked);
        }

        if (cursorLocked)
        {
            Vector2 lookInput = lookAction.ReadValue<Vector2>();
            yaw += lookInput.x * mouseSensitivity;
            pitch = Mathf.Clamp(pitch - lookInput.y * mouseSensitivity, -maximumLookAngle, maximumLookAngle);
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            if (cameraTransform != null)
            {
                cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        Vector2 moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        Vector3 velocity = moveDirection * movementSpeed + Vector3.up * verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void SetCursorLocked(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
