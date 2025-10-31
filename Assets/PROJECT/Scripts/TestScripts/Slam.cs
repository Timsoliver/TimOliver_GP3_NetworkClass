using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Slam : MonoBehaviour
{
    
    [Header("Slam Settings")]
    
    [SerializeField] private float dropForce = 30f;
    [SerializeField] private float stopTime = 0.5f;
    [SerializeField] private float gravityScale = 1f;
    
    [Header("Ground Check")]
    
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayers;
    
    private Rigidbody rb;

    private bool doSlam = false;
    private bool isSlamming = false;

    private void Awake()
    { 
        rb = GetComponent<Rigidbody>();
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
            if (contacts[i].normal.y >= 0.5)
            {
                CompleteSlam();
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
        StartCoroutine("DropAndSmash"); 
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
}
