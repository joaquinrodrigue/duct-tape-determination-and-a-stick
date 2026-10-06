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
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ArduinoReceiver arduinoController;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask groundLayer; 

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
        if(isRagdolled) return;
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
