using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class PlayerNetworkMovement : NetworkBehaviour
{
    [Header("PlayerData")] [SerializeField]
    private NetworkVariable<float> playerHealth = new NetworkVariable<float>(100,NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

    private NetworkVariable<CustomPlayerData> playerData = new NetworkVariable<CustomPlayerData>(
        new CustomPlayerData
        {
            playerNumber = 12,
            playerHealth = 100,
            playerName = "Player"
        }
    );
    
    public struct CustomPlayerData
    {
        public int playerNumber;
        public int playerHealth;
        public string playerName;

        public void NetworkSerialise<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref playerNumber);
            serializer.SerializeValue(ref playerHealth);
            serializer.SerializeValue(ref playerName);
        }
    }
    
    [Header ("Movement")]

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

        playerHealth.Value = Random.Range(0, 100);
    }
    

    private void OnDisable()
    {
        inputMap.PlayerActionMap.Jump.performed -= OnJump;
        inputMap.PlayerActionMap.Pause.performed -= OnPause;
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
    
    private void OnPause(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        
        Debug.Log(OwnerClientId + "; Player Health: " + playerHealth.Value);
    }
}
