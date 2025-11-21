using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour
{
    [Header("Block Properties")] 
    [SerializeField] private float blockDuration = 1f;
    [SerializeField] private float knockbackMultiplier = 0.5f; //Remember 0 = no knockback and 1 = max knockback
    [SerializeField] private float blockCooldown = 1.5f;
    
    [Header("Visuals")] 
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Color blockColor = Color.green;
    [SerializeField] private GameObject blockObject;

    [Header("Player")] 
    [SerializeField] private Player player;
    
    private Color originalColor;
    private bool isBlocking = false;

    public bool IsBlocking => isBlocking;
    public float KnockbackMultiplier => knockbackMultiplier;
    public float BlockDuration => blockDuration;
    public float BlockCooldown => blockCooldown;

    private void Awake()
    {
        if (bodyRenderer != null)
            originalColor = bodyRenderer.material.color;
        
        if (blockObject != null)
            blockObject.SetActive(false);
    }

    public void RequestBlock()
    {
        if (isBlocking) return;
        StartCoroutine(BlockRoutine());
    }

    private IEnumerator BlockRoutine()
    {
        isBlocking = true;
        
        if (blockObject != null)
            blockObject.SetActive(true);
        
        if (bodyRenderer != null)
            bodyRenderer.material.color = blockColor;

        yield return new WaitForSeconds(blockDuration);

        if (player != null && blockCooldown > 0f)
        {
            player.StartBlockCooldown(blockCooldown);
        }
        
        isBlocking = false;
        
        if (blockObject != null)
            blockObject.SetActive(false);
        
        if (bodyRenderer != null)
            bodyRenderer.material.color = originalColor;
    }
}
