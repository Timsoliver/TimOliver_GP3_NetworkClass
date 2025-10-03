using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetworkMovement : NetworkBehaviour
{

    private MultiplayerInputMap inputMap;

    private Vector2 playerMovementDirection;

    [SerializeField] private float movementSpeed;

    [SerializeField] private Transform playerParentTransform;
    
    private void Awake()
    {
        inputMap = new MultiplayerInputMap();
        inputMap.Enable();

        inputMap.PlayerActionMap.Jump.performed += OnJump;
        inputMap.PlayerActionMap.Pause.performed += OnPause;
        inputMap.PlayerActionMap.Movement.performed += OnMove;
        inputMap.PlayerActionMap.Movement.canceled += OnResetMove;
    }
    

    private void OnDisable()
    {
        inputMap.PlayerActionMap.Jump.performed -= OnJump;
        inputMap.PlayerActionMap.Pause.performed -= OnPause;
        inputMap.PlayerActionMap.Movement.performed -= OnMove;
        inputMap.PlayerActionMap.Movement.canceled -= OnResetMove;
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        playerParentTransform.localPosition = new Vector3(
            playerParentTransform.localPosition.x + movementSpeed * playerMovementDirection.x * Time.deltaTime,
            playerParentTransform.localPosition.y,
            playerParentTransform.localPosition.z + movementSpeed * playerMovementDirection.y * Time.deltaTime);
        
    }

    private void OnResetMove(InputAction.CallbackContext context)
    {
        playerMovementDirection = Vector2.zero;
    }
    private void OnMove(InputAction.CallbackContext context)
    {
        playerMovementDirection = context.ReadValue<Vector2>();
    }
    
    private void OnJump(InputAction.CallbackContext context)
    {
        
    }
    
    private void OnPause(InputAction.CallbackContext context)
    {
        
    }
}
