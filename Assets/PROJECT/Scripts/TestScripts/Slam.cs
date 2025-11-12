using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Slam : MonoBehaviour
{
    
    [Header("Slam Settings")]
    
    [SerializeField] private float dropForce = 30f;
    [SerializeField] private float stopTime = 0.5f;
    [SerializeField] private float postSlamCooldown = 3f;
    
    [Header("Ground Check")]
    
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayers;
    
    [Header("Slam Image Settings")]
    
    [SerializeField] private GameObject slamImage;
    [SerializeField] private float imageReturnDelay = 3f;
    
    [Header("Player")]
    [SerializeField] private Player player;
    
    [Header("Knockout")] 
    
    [SerializeField] private float slamRadius = 4f;
    [SerializeField] private float slamForce = 12f;
    [SerializeField] private float upwardModifer = 0.25f;
    [SerializeField] private LayerMask affectedLayers;
    
    private Rigidbody rb;
    private bool doSlam = false;
    private bool isSlamming = false;
    
    private void Awake()
    { 
        rb = GetComponent<Rigidbody>();
        if (slamImage != null)
            slamImage.SetActive(false);
    }
    
    void FixedUpdate()
    {
        if (doSlam && !isSlamming)
        {
            SlamAttack();
        }
        doSlam = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!isSlamming) return;
        
        var contacts = other.contacts;
        for (int i = 0; i < contacts.Length; i++)
        {
            if (contacts[i].normal.y >= 0.5f)
            {
                CompleteSlam();
                
                if (player != null && postSlamCooldown > 0f)
                    player.LockControlsFor(postSlamCooldown);
                
                break;
            }
        }
    }

    public void RequestSlam()
    {
        if (!IsGrounded())
        {
            doSlam = true;
        }
    }
    
    private void SlamAttack()
    {
        isSlamming = true;
        Stop();
        StartCoroutine(DropAndSmash()); 
    }

    private void Stop()
    {
        ClearForces();
        rb.useGravity = false;
    }

    private IEnumerator DropAndSmash()
    {
        yield return new WaitForSeconds(stopTime);
        rb.AddForce(Vector3.down * dropForce, ForceMode.Impulse);
    }

    private void CompleteSlam()
    {
        rb.useGravity = true;
        isSlamming = false;

        if (slamImage != null)
        {
            slamImage.SetActive(true);
            StartCoroutine(ImageReturnDelay());
        }

        if (player != null)
        {
            player.DoSlamKnockbackNetwork(transform.position, slamRadius, slamForce, upwardModifer, affectedLayers);
        }
    }

    private IEnumerator ImageReturnDelay()
    {
        yield return new WaitForSeconds(imageReturnDelay);
        if (slamImage != null)
            slamImage.SetActive(false);
    }

    private void ClearForces()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics.CheckSphere( groundCheck.position, groundCheckRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = new Color(1f, 0.6f, 0f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, slamRadius);
    }
    
}
