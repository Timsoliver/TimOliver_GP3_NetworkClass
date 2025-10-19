using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player: NetworkBehaviour
{
    private MultiplayerInputMap inputMap;

    private Vector2 playerMovementDirection;

    [SerializeField] private float movementSpeed;

    [SerializeField] private Transform playerTransform;
    
    [SerializeField] private GameObject spawnedItemPrefab;
    
    

    private void OnDisable()
    {
        inputMap.PlayerActionMap.Jump.performed -= OnJump;
        //inputMap.PlayerActionMap.Interact.performed -= OnInteract;
        inputMap.PlayerActionMap.Movement.performed -= OnMove;
        inputMap.PlayerActionMap.Movement.canceled -= OnResetMove;
        
        
    }

    public override void OnNetworkSpawn() 
    {
        inputMap = new MultiplayerInputMap();
        inputMap.Enable();

        inputMap.PlayerActionMap.Jump.performed += OnJump;
        //inputMap.PlayerActionMap.Interact.performed += OnInteract;
        inputMap.PlayerActionMap.Movement.performed += OnMove;
        inputMap.PlayerActionMap.Movement.canceled += OnResetMove;
    }

    private void Update()
    {
        if (!IsOwner) return;
        
        playerTransform.localPosition = new Vector3(
            playerTransform.localPosition.x + movementSpeed * playerMovementDirection.x * Time.deltaTime,
            playerTransform.localPosition.y,
            playerTransform.localPosition.z + movementSpeed * playerMovementDirection.y * Time.deltaTime);
        
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

    public override void OnNetworkDespawn()
    {
        if (IsOwner && inputMap != null)
        {
            inputMap.Disable();
        }
    }
   
    /*void OnInteract(InputAction.CallbackContext context)
    {
        GameObject tempHolder = Instantiate(spawnedItemPrefab);
        tempHolder.GetComponent<NetworkObject>().Spawn(true);
    }*/
    
}
