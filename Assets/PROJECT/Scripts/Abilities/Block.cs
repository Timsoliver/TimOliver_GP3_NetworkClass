using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour
{
    [Header("Block Properties")] 
    [SerializeField] private float blockDuration = 1.5f;
    [SerializeField] private float knockbackMultiplier = 0.5f; //Remember 0 = no knockback and 1 = max knockback

    [Header("Visuals")] 
    [SerializeField] private Renderer bodyRenderer;
    [SerializeField] private Color blockColor = Color.green;
    [SerializeField] private GameObject blockObject;
    
    private Color originalColor;
    private bool isBlocking = false;

    public bool IsBlocking => isBlocking;
    public float KnockbackMultiplier => knockbackMultiplier;
    public float BlockDuration => blockDuration;

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
        
        isBlocking = false;
        
        if (blockObject != null)
            blockObject.SetActive(false);
        
        if (bodyRenderer != null)
            bodyRenderer.material.color = originalColor;
    }
}
