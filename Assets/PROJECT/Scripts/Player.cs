using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class Player: NetworkBehaviour
{
    private MultiplayerInputMap inputMap;
    private Rigidbody rb;
    private Vector2 moveInput;

    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpForce = 5f;
    
    private bool controlsLocked = false;
    
    [Header("Abilities")]
    [SerializeField] private Slam slam;
    [SerializeField] private Block block;
    
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
        inputMap.PlayerActionMap.Block.performed += OnBlock;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (controlsLocked) return;
        
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
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

    private void OnBlock(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        block?.RequestBlock();
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

    [ServerRpc]
    public void ShowSlamImageServerRpc()
    {
        ShowSlamImageClientRpc();
    }

    [ClientRpc]
    private void ShowSlamImageClientRpc()
    {
        if (IsOwner) return;
        
        if (slam != null)
        {
            slam.ShowSlamImage();
        }
    }

    public void StartCooldownColorNetwork(float seconds)
    {
        if (!IsOwner) return;
        StartCooldownColorServerRpc(seconds);
    }

    [ServerRpc]
    private void StartCooldownColorServerRpc(float seconds)
    {
        StartCooldownColorClientRpc(seconds);
    }

    [ClientRpc]
    private void StartCooldownColorClientRpc(float seconds)
    {
        if (IsOwner) return;
        if (bodyRenderer == null) return;
        
        StartCoroutine(CooldownColorRoutine(seconds));
    }

    private IEnumerator CooldownColorRoutine(float seconds)
    {
        bodyRenderer.material.color = cooldownColor;
        yield return new WaitForSeconds(seconds);
        bodyRenderer.material.color = originalColor;
    }

    public void DoSlamKnockbackNetwork(Vector3 origin, float radius, float force, float upward, LayerMask mask)
    {
        if (!IsOwner) return;
        ApplySlamKnockbackServerRpc(origin, radius, force, upward, mask.value);
    }

    [ServerRpc]
    private void ApplySlamKnockbackServerRpc(Vector3 origin, float radius, float force, float upward, int layerMask)
    {
        var hits = Physics.OverlapSphere(origin, radius, layerMask, QueryTriggerInteraction.Ignore);

        foreach (var hit in hits)
        {
            var targetRb = hit.attachedRigidbody;
            if (targetRb == null) continue;
            
            var targetNetworkObject = hit.GetComponentInParent<NetworkObject>();
            if (targetNetworkObject == null) continue;
            
            if (targetNetworkObject == this.NetworkObject) continue;
            
            Vector3 direction = (targetRb.worldCenterOfMass - origin).normalized;
            direction.y = Mathf.Max(direction.y, upward);
            Vector3 impulse = direction * force;
            
            ulong targetClientId = targetNetworkObject.OwnerClientId;
            var sendParams = new ClientRpcParams()
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { targetClientId }
                }
            };
            
            var targetPlayer = targetNetworkObject.GetComponent<Player>();
            if (targetPlayer != null)
            {
                targetPlayer.ApplyKnockbackClientRpc(impulse, sendParams);
            }
        }
    }
    
    [ClientRpc]
    private void ApplyKnockbackClientRpc(Vector3 impulse, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner) return;   
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 finalImpulse = impulse;
        if (block != null && block.IsBlocking)
        {
           finalImpulse *= block.KnockbackMultiplier; 
        }
        
        rb.AddForce(finalImpulse, ForceMode.Impulse);
    }
}
