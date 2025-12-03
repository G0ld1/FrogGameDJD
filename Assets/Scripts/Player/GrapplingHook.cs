using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    [Header("Configuração")]
    [SerializeField] private float grappleRange = 15f; 
    [SerializeField] private LayerMask grappleLayer;
    [SerializeField] private LineRenderer rope;
    [SerializeField] private KeyCode grappleKey = KeyCode.Space; 
    
    [Header("Grapple Launch")]
    [SerializeField] private float bashForce = 50f;
  private bool isTargeting; 
    [SerializeField] private float launchVerticalBoost = 1.2f;
    
  
    private bool drawGizmo = false;
    
public bool isGrapplingActive = false;

   
    public Vector3 grapplePoint;


    
    private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement component not found on this object!");
        }
    }

    void Start()
    {
        if (rope != null)
        {
            rope.enabled = false;
        }
    }
    


    void Update()
    {
        if (Input.GetKeyDown(grappleKey))
        {
            FindAndStartGrapple();
        }

        if (rope != null && rope.enabled == true)
        {
            rope.SetPosition(1, transform.position);
        }
        
        if (isTargeting)
        {
    
            Vector3 horizontalDirection = transform.right; 
            Vector3 launchDirection = horizontalDirection + (Vector3.up * launchVerticalBoost);
        
           
        }
    }

    
    
 

  private void FindAndStartGrapple()
{
  

    float closestDistance = float.MaxValue; 
    Vector3 closestTargetPoint = Vector3.zero;
    Vector3 playerPos = transform.position;

    Collider[] targets = Physics.OverlapSphere(
        transform.position, 
        grappleRange, 
        grappleLayer
    );
 
    if (targets.Length == 0)
    {
        Debug.Log("Grapple falhou: Nenhum alvo encontrado.");
        return;
    }

    foreach (Collider target in targets)
    {
        Vector3 targetPoint = target.bounds.center; 
        float distance = Vector3.Distance(playerPos, targetPoint); 

        if (distance < closestDistance)
        {
            closestDistance = distance;
            closestTargetPoint = targetPoint;
        }
    }

   grapplePoint = closestTargetPoint;
    
    Rigidbody rb = GetComponent<Rigidbody>();
    if (rb != null)
    {
    
        rb.useGravity = false; 
        rb.angularVelocity = Vector3.zero;

     
        Vector3 pullDirection = (grapplePoint - transform.position);
        float distance = pullDirection.magnitude;
        pullDirection.Normalize();

      
        float initialLaunchSpeed = playerMovement.MoveStats.launchForce; 
     
        Vector3 finalVelocity = pullDirection * initialLaunchSpeed;
        
    
        rb.linearVelocity = finalVelocity; 
    }


    isGrapplingActive = true; 
    
    if (rope != null)
    {
        rope.SetPosition(0, grapplePoint);
        rope.SetPosition(1, transform.position);
        rope.enabled = true;
    }
    
   
    StartCoroutine(InstantGrappleRelease(0.1f));
}
  
IEnumerator InstantGrappleRelease(float duration)
{
    
    yield return new WaitForFixedUpdate(); 
    
    Rigidbody rb = GetComponent<Rigidbody>();
    if (rb != null)
    {
    
        rb.useGravity = true; 
        
        playerMovement.InitiateGrappleLaunch(rb.linearVelocity.y);
    }
    
    isGrapplingActive = false;
    if (rope != null)
    {
        rope.enabled = false;
    }
}
    
 
}