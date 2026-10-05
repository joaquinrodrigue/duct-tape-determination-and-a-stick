using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float acceleration = 50f;
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
        jumpAction.action.canceled += OnJumpExit;
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
    }
    
    void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
    }

    private void OnMoveEnter(InputAction.CallbackContext ctx)
    {
        if(GetIsJumping()) enabled = false;
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
        Vector3 moveDirection = new Vector3(MoveInput.x, 0, MoveInput.y).normalized; 
        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if(horizontalVelocity.magnitude < maxSpeed || Vector3.Dot(moveDirection, horizontalVelocity) < 0)
        {
            rb.AddRelativeForce(acceleration * moveDirection, ForceMode.Acceleration);
        }

        if (JumpPressed)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            rb.AddRelativeForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
        }

        if (JumpPressed && Physics.Raycast(transform.position, Vector3.down, 0.1f))
        {
            JumpPressed = false;
        }
    }

    internal bool GetIsJumping()
    {
        return JumpPressed;
    }
}
