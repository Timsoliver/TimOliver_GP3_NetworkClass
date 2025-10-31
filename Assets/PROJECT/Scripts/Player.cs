using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Player: NetworkBehaviour
{
    private MultiplayerInputMap inputMap;
    private Rigidbody rb;
    private Vector2 moveInput;

    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpForce = 5f;
    
    private bool controlsLocked = false;
    
    [SerializeField] private Slam slam;
    
    [Header("Color Change")]
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Color cooldownColor = Color.yellow;
    private Color originalColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        if(bodyRenderer != null)
            originalColor = bodyRenderer.material.color;
    }
    
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        
        inputMap = new MultiplayerInputMap();
        inputMap.Enable();

        inputMap.PlayerActionMap.Jump.performed += OnJump;
        inputMap.PlayerActionMap.Movement.performed += OnMove;
        inputMap.PlayerActionMap.Movement.canceled += OnResetMove;
        inputMap.PlayerActionMap.Slam.performed += OnSlam;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (controlsLocked) return;
        
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        rb.MovePosition(rb.position + move * movementSpeed * Time.fixedDeltaTime);
    }
    
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    private void OnResetMove(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }
    
    private void OnJump(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (controlsLocked) return;
        
        if (Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnSlam(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        slam?.RequestSlam();
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner && inputMap != null)
        {
            inputMap.Disable();
        }
    }

    public void LockControlsFor(float seconds)
    {
        StartCoroutine(LockRoutine(seconds));
    }

    private IEnumerator LockRoutine(float seconds)
    {
        controlsLocked = true;
        moveInput = Vector2.zero;
        
        if (bodyRenderer != null)
            bodyRenderer.material.color = cooldownColor;
        
        yield return new WaitForSeconds(seconds);
        controlsLocked = false;
        
        if (bodyRenderer != null)
            bodyRenderer.material.color = originalColor;
    }
    
    private void OnDisable()
    {
        if (inputMap == null) return;
        inputMap.PlayerActionMap.Jump.performed -= OnJump;
        inputMap.PlayerActionMap.Movement.performed -= OnMove;
        inputMap.PlayerActionMap.Movement.canceled -= OnResetMove;
        inputMap.PlayerActionMap.Slam.performed -= OnSlam;
    }
    
}
