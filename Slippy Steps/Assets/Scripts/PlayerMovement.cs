using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds1 = new(1f);
    
    [Header("Movement")]
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float maxSpeed = 10.0f;
    [SerializeField] private float jumpSpeed = 8.0f;
    [SerializeField] private bool isRagdolled = false;
    [SerializeField] private bool isWakingUp = false;

    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private InputActionReference xAxisAction;
    [SerializeField] private InputActionReference yAxisAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ArduinoReceiver arduinoController;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask groundLayer;

    private Vector2 moveInput;
    public Vector2 MoveInput { get => moveInput; }
    public bool JumpHeld { get; private set; }
    public bool JumpPressed { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        xAxisAction.action.performed += OnXMoveEnter;
        xAxisAction.action.canceled += OnXMoveExit;
        yAxisAction.action.performed += OnYMoveEnter;
        yAxisAction.action.canceled += OnYMoveExit;
        jumpAction.action.performed += OnJumpEnter;
        jumpAction.action.canceled += OnJumpExit;
    }

    void OnEnable()
    {
        xAxisAction.action.Enable();
        yAxisAction.action.Enable();
        jumpAction.action.Enable();
    }
    
    void OnDisable()
    {
        xAxisAction.action.Disable();
        yAxisAction.action.Disable();
        jumpAction.action.Disable();
    }

    private void OnXMoveEnter(InputAction.CallbackContext ctx)
    {
        if (isRagdolled) return;
        moveInput.x = ctx.ReadValue<float>();
    }

    private void OnXMoveExit(InputAction.CallbackContext ctx)
    {
        moveInput.x = 0f;
    }

    private void OnYMoveEnter(InputAction.CallbackContext ctx)
    {
        if (isRagdolled) return;
        moveInput.y = ctx.ReadValue<float>();
    }

    private void OnYMoveExit(InputAction.CallbackContext ctx)
    {
        moveInput.y = 0f;
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

        if(isRagdolled && GroundCheck() && !isWakingUp)
        {
            StartCoroutine(RagdollWakeup());
        }

        if(isRagdolled) return;

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
            isRagdolled = true;
        }
    }

    public bool GetJumpPressed()
    {
        if(isRagdolled) return false;
        return JumpPressed;
    }

    public bool GroundCheck()
    {
        Debug.DrawRay(transform.position + Vector3.down, Vector3.down * 0.01f, Color.red, .016f);
        return Physics.SphereCast(transform.position + Vector3.down * 1.5f, 0.1f, Vector3.down, out _, 0.05f, groundLayer);
    }

    private IEnumerator RagdollWakeup()
    {
        Debug.Log("Ragdoll waking up");
        isWakingUp = true;
        yield return _waitForSeconds1;
        isRagdolled = false;
        isWakingUp = false;
        JumpPressed = false;
    }
}
