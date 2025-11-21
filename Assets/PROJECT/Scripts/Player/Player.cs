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

    [Header("Ability Cooldowns")] 
    [SerializeField] private Image slamIcon;
    [SerializeField] private Image blockIcon;
    [SerializeField] private Color abilityReadyColor = Color.white;
    [SerializeField] private Color abilityCooldownColor = Color.grey;

    [Header("End Round UI")]
    [SerializeField] private GameObject youDiedUI;
    [SerializeField] private GameObject youWinUI;
    
    [Header("Spawn UI")]
    [SerializeField] private GameObject spawnButtonUI;
    
    
    private float slamCooldownTimer = 0f;
    private float blockCooldownTimer = 0f;
    
    public bool IsDead { get; private set; } = false;
    private bool isWaitingToSpawn = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        if(bodyRenderer != null)
            originalColor = bodyRenderer.material.color;
    }
    
    public override void OnNetworkSpawn()
    {
        if (IsServer && RoundManager.Instance != null)
        {
            RoundManager.Instance.RegisterPlayer(this);
        }
        
        if (!IsOwner) return;
        
        inputMap = new MultiplayerInputMap();
        inputMap.Enable();

        inputMap.PlayerActionMap.Jump.performed += OnJump;
        inputMap.PlayerActionMap.Movement.performed += OnMove;
        inputMap.PlayerActionMap.Movement.canceled += OnResetMove;
        inputMap.PlayerActionMap.Slam.performed += OnSlam;
        inputMap.PlayerActionMap.Block.performed += OnBlock;

        if (slamIcon == null)
        {
            GameObject slamIconObj = GameObject.Find("Slam Image");
            if (slamIconObj != null)
                slamIcon = slamIconObj.GetComponent<Image>();
        }

        if (blockIcon == null)
        {
            GameObject blockIconObj = GameObject.Find("Block Image");
            if (blockIconObj != null)
                blockIcon = blockIconObj.GetComponent<Image>();
        }

        if (youDiedUI == null || youWinUI == null || spawnButtonUI == null)
        {
           var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
           foreach (var obj in allObjects)
           {
               if (!obj.scene.IsValid())
                   continue;
               
               if (obj.name == "You Died" && youDiedUI == null)
                   youDiedUI = obj;
               else if (obj.name == "You Win" && youWinUI == null)
                   youWinUI = obj;
               else if (obj.name == "Spawn Button" && spawnButtonUI == null)
                   spawnButtonUI = obj;
           }
        }
        
        
        if (youDiedUI != null)
            youDiedUI.SetActive(false);
        
        if (youWinUI != null)
            youWinUI.SetActive(false);
        
        if (spawnButtonUI != null)
            spawnButtonUI.SetActive(false);
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (slam != null && slam.PostSlamCooldown > 0f)
        {
            if (slamCooldownTimer > 0f)
            {
                slamCooldownTimer -= Time.deltaTime;
                if (slamCooldownTimer < 0f) slamCooldownTimer = 0f;
            }
            
            if (slamIcon != null)
            {
                if (slamCooldownTimer > 0f)
                {
                    float t = slamCooldownTimer / slam.PostSlamCooldown;
                    slamIcon.fillAmount = t;
                    slamIcon.color = abilityCooldownColor;
                }
                else
                {
                    slamIcon.fillAmount = 0f;
                    slamIcon.color = abilityReadyColor;
                }
            }
        }

        if (block != null && block.BlockCooldown > 0f)
        {
            if (blockCooldownTimer > 0f)
            {
                blockCooldownTimer -= Time.deltaTime;
                if (blockCooldownTimer < 0f) blockCooldownTimer = 0f;
            }

            if (blockIcon != null)
            {
                if (blockCooldownTimer > 0f)
                {
                    float t = blockCooldownTimer / block.BlockCooldown;
                    blockIcon.fillAmount = t;
                    blockIcon.color = abilityCooldownColor;
                }
                else
                {
                    blockIcon.fillAmount = 0f;
                    blockIcon.color = abilityReadyColor;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (controlsLocked) return;
        if (IsDead) return;
        if (isWaitingToSpawn) return;
        
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
        if (IsDead) return;
        if (isWaitingToSpawn) return;
        
        if (Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnSlam(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (slam == null) return;
        if (IsDead) return;
        if (isWaitingToSpawn) return;

        if (slamCooldownTimer > 0f) return;
        
        slam.RequestSlam();
    }

    public void StartSlamCooldown(float seconds)
    {
        if (!IsOwner) return;
        slamCooldownTimer = seconds;
    }
    
    private void OnBlock(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        if (block == null) return;
        if (IsDead) return;
        if (isWaitingToSpawn) return;
        
        if (blockCooldownTimer > 0f) return;
        
        block.RequestBlock();

        StartBlockNetwork();
    }

    public void StartBlockCooldown(float seconds)
    {
        if (!IsOwner) return;
        blockCooldownTimer = seconds;
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && RoundManager.Instance != null)
        {
            RoundManager.Instance.UnregisterPlayer(this);
        }
        
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
        inputMap.PlayerActionMap.Block.performed -= OnBlock;
    }

    #region Slam Image Sync Up
    
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
    #endregion
    
    #region Slam Cooldown Color Sync Up
    
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
        
    #endregion

    #region Block Visuals Sync Up
    
        public void StartBlockNetwork()
        {
            if (!IsOwner) return;
            StartBlockServerRpc();
        }

        [ServerRpc]
        private void StartBlockServerRpc()
        {
            StartBlockClientRpc();
        }

        [ClientRpc]
        private void StartBlockClientRpc()
        {
            if (IsOwner) return;
            if (block == null) return;
            
            block.RequestBlock();
        }
    
    #endregion
    
    #region Lava

        [ClientRpc]
        public void KillPlayerClientRpc()
        {
            IsDead = true;
            isWaitingToSpawn = false;
            
            if (IsOwner && youDiedUI != null)
            {
                youDiedUI.SetActive(true);
            }
            
            controlsLocked = true;

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
            }
            
            var col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;
            
            if (bodyRenderer != null)
                bodyRenderer.enabled = false;

            if (IsServer && RoundManager.Instance != null)
            {
                RoundManager.Instance.OnPlayerDied(this);
            }
        }
    
    #endregion
    
    #region Pre-Round/Spawn

        [ClientRpc]
        public void EnterPreRoundStateClientRpc()
        {
                IsDead = false;
                isWaitingToSpawn = true;
                
                controlsLocked = true;
                moveInput = Vector2.zero;
                
                if (rb == null)
                    rb = GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    
                    rb.isKinematic = true;
                }

                var col = GetComponent<Collider>();
                if (col != null)
                    col.enabled = false;
                
                if (bodyRenderer != null)
                    bodyRenderer.enabled = false;
                

                if (IsOwner)
                {
                    if (youDiedUI != null)
                        youDiedUI.SetActive(false);
                    
                    if (youWinUI != null)
                        youWinUI.SetActive(false);
                    
                    if (spawnButtonUI != null)
                        spawnButtonUI.SetActive(true);
                }
        }

        public void RequestSpawn()
        {
            if (!IsOwner) return;
            if (!isWaitingToSpawn) return;

            RequestSpawnServerRpc();
        }

        [ServerRpc]
        private void RequestSpawnServerRpc(ServerRpcParams rpcParams = default)
        {
            if (RoundManager.Instance == null) return;

            int index = RoundManager.Instance.GetPlayerIndex(this);
            if (index < 0) index = 0;

            Vector3 spawnPos = RoundManager.Instance.GetSpawnPosition(index);

            RespawnClientRpc(spawnPos);
        }

        [ClientRpc]
        private void RespawnClientRpc(Vector3 spawnPosition)
        {
            IsDead = false;
            isWaitingToSpawn = false;
                
            if (rb == null)
                rb = GetComponent<Rigidbody>();

            
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            var col = GetComponent<Collider>();
            if (col != null)
                col.enabled = true;
                
            if (bodyRenderer != null)
                bodyRenderer.enabled = true;
                
            transform.position = spawnPosition;
            controlsLocked = false;

            if (IsOwner)
            {
                if (youDiedUI != null)
                    youDiedUI.SetActive(false);
                    
                if (youWinUI != null)
                    youWinUI.SetActive(false);
                    
                if (spawnButtonUI != null)
                    spawnButtonUI.SetActive(false);
            }
        }
    
    #endregion
    
    #region Win UI

        [ClientRpc]
        public void ShowWinClientRpc()
        {
            if (!IsOwner) return;
            
            if (youWinUI != null)
                youWinUI.SetActive(true);
        }
        
    #endregion
    
    #region Knockback 
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
    #endregion    
}
