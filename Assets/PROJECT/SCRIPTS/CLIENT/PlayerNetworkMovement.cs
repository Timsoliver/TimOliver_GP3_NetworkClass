using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerNetworkMovement : NetworkBehaviour
{
    
    [Header("PlayerData")] [SerializeField]
    private NetworkVariable<float> playerHealth = new NetworkVariable<float>(100,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

    private MultiplayerInputMap inputMap;

    private Vector2 playerMovementDirection;

    [SerializeField] private float movementSpeed;

    [SerializeField] private Transform playerParentTransform;
    
    [SerializeField] private GameObject spawnedItemPrefab;
    
    
    
    private void Awake()
    {
        inputMap = new MultiplayerInputMap();
        inputMap.Enable();

        inputMap.PlayerActionMap.Jump.performed += OnJump;
        inputMap.PlayerActionMap.Interact.performed += OnInteract;
        inputMap.PlayerActionMap.Movement.performed += OnMove;
        inputMap.PlayerActionMap.Movement.canceled += OnResetMove;

        playerHealth.Value = Random.Range(0, 100);
    }
    

    private void OnDisable()
    {
        inputMap.PlayerActionMap.Jump.performed -= OnJump;
        inputMap.PlayerActionMap.Interact.performed -= OnInteract;
        inputMap.PlayerActionMap.Movement.performed -= OnMove;
        inputMap.PlayerActionMap.Movement.canceled -= OnResetMove;
    }

    public override void OnNetworkSpawn()
    {
        
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
   
    void OnInteract(InputAction.CallbackContext context)
    {
        GameObject tempHolder = Instantiate(spawnedItemPrefab);
        tempHolder.GetComponent<NetworkObject>().Spawn(true);

    }

    [ServerRpc]
    private void TestServerRpc()
    {
        
    }

    [ClientRpc]
    void TestClientRpc()
    {
        
    }
}
