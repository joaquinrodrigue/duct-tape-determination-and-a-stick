using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float acceleration = 1.5f;
    [SerializeField] private float maxSpeed = 10.0f;
    [SerializeField] private float jumpSpeed = 8.0f;

    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Components")]
    [SerializeField] private Rigidbody rb;

    public Vector2 MoveInput { get; private set; }
    public bool JumpHeld { get; private set; }
    public bool JumpPressed { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        moveAction.action.performed += OnMoveEnter;
        moveAction.action.canceled += OnMoveExit;
        jumpAction.action.performed += OnJumpEnter;
        jumpAction.action.performed += OnJumpExit;
    }

    private void OnMoveEnter(InputAction.CallbackContext ctx)
    {
        MoveInput = ctx.ReadValue<Vector2>();
    }

    private void OnMoveExit(InputAction.CallbackContext ctx)
    {
        MoveInput = Vector2.zero;
    }

    private void OnJumpEnter(InputAction.CallbackContext ctx)
    {
        JumpHeld = true;
        JumpPressed = true;
    }

    private void OnJumpExit(InputAction.CallbackContext ctx)
    {
        JumpHeld = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetVelocity = maxSpeed * (new Vector3(MoveInput.x, 0, MoveInput.y)).normalized;
        rb.AddRelativeForce(acceleration * (new Vector3(MoveInput.x, 0, MoveInput.y)).normalized, ForceMode.Force);
        Vector3 horizontalVelocity = new(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (targetVelocity.magnitude < horizontalVelocity.magnitude)
        {
            Vector3 temp = rb.linearVelocity;
            horizontalVelocity = horizontalVelocity.normalized * targetVelocity.magnitude;
            rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
        }

        if (JumpPressed)
        {
            rb.AddRelativeForce(new Vector3(0, jumpSpeed, 0), ForceMode.Impulse);
        }
        JumpPressed = false;
    }
}
