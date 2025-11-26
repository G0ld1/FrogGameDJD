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
    
  
    private Vector3 currentLaunchDirection = Vector3.up; 
    private bool drawGizmo = false;
    
    // Configurações do Spring Joint (para o balanço)
    [SerializeField] private float jointSpring = 4.5f;
    [SerializeField] private float jointDamper = 7f;
    [SerializeField] private float jointMassScale = 4.5f;
    [SerializeField] private float distanceReduction = 0.8f; // Puxa a distância para 80%

    public Vector3 grapplePoint;
    private SpringJoint joint; 

    
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
        // 🚀 NÃO ADICIONAMOS A JOINT AQUI. Ela será adicionada em tempo de execução.
        if (rope != null)
        {
            rope.enabled = false;
        }
    }
    


    void Update()
    {
        // --- 1. DETECÇÃO DE INPUT POR TECLA ---
        if (Input.GetKeyDown(grappleKey))
        {
            FindAndStartGrapple();
        }

        // --- 2. TERMINAR O GRAPPLE AO SOLTAR A TECLA ---
        if (Input.GetKeyUp(grappleKey))
        {
            StopGrapple();
        }

        // --- 3. ATUALIZAR A CORDA ---
        if (rope != null && rope.enabled == true)
        {
            rope.SetPosition(1, transform.position);
        }
        
        if (isTargeting)
        {
            // Calcular a direção de lançamento potencial
            Vector3 horizontalDirection = transform.right; 
            Vector3 launchDirection = horizontalDirection + (Vector3.up * launchVerticalBoost);
        
            // Armazenamos a direção normalizada (já que a força está congelada)
            currentLaunchDirection = launchDirection.normalized; 
        }
    }

    //---------------------------------------------------------

    private void StopGrapple()
    {
     
        if (joint != null)
        {
           
            if (isTargeting)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                   
                    Vector3 horizontalDirection = transform.right; 
        
                    
                    Vector3 launchDirection = horizontalDirection + (Vector3.up * launchVerticalBoost);

                    
                    launchDirection = launchDirection.normalized; 

                    
                

              
                    rb.AddForce(launchDirection * bashForce, ForceMode.VelocityChange); 
              
                    
                    playerMovement.InitiateGrappleLaunch(rb.linearVelocity.y);
                }
            }
        
     
            Destroy(joint);
        }
    
        isTargeting = false; // 🚀 RESETAR ESTADO
        if (rope != null)
        {
            rope.enabled = false;
        }
    }

  private void FindAndStartGrapple()
{
 
    if (joint != null) return;
    

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

    // 3. Encontrar o alvo mais próximo
    float closestDistance = float.MaxValue;
    Vector3 closestTargetPoint = Vector3.zero;
    Vector3 playerPos = transform.position;

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

    // --- INICIAR O GRAPPLE E A FASE DE PAUSA/APONTAR ---
    
    grapplePoint = closestTargetPoint;
    
    
    Rigidbody rb = GetComponent<Rigidbody>();
    if (rb != null)
    {
    
        rb.linearVelocity = Vector3.zero; // Congela o movimento no ar
        rb.angularVelocity = Vector3.zero;
    }
    

    joint = gameObject.AddComponent<SpringJoint>();
    joint.autoConfigureConnectedAnchor = false;
    joint.connectedAnchor = grapplePoint; 
    float initialDistance = closestDistance;


    joint.minDistance = initialDistance * distanceReduction; 
    

    joint.maxDistance = initialDistance * 1.1f; 
    joint.spring = jointSpring; 
    joint.damper = jointDamper; 
    joint.massScale = jointMassScale;
    
    // 6. Entrar no estado de Apontar/Pausa
    isTargeting = true;
 
    
    // 7. Configurar a LineRenderer
    if (rope != null)
    {
        rope.SetPosition(0, grapplePoint);
        rope.SetPosition(1, transform.position);
        rope.enabled = true;
    }
    
    
    
}
  
    private void OnDrawGizmos()
    {
        // Desenha SÓ SE estiver em modo de apontar (tempo está parado)
        if (!Application.isPlaying || !isTargeting) 
            return;

        // Define a cor do Gizmo (vermelho)
        Gizmos.color = Color.red;

        Vector3 startPoint = transform.position;
        
        // O comprimento do Gizmo deve ser ajustável ou usar o BashPower
        // Usamos o BashPower para ter a magnitude real da força que será aplicada.
        Vector3 endPoint = startPoint + (currentLaunchDirection * (bashForce / 15f)); // Dividir por 15f para escala visual
        
        // Desenha o vetor do impulso
        Gizmos.DrawLine(startPoint, endPoint);

        // Desenha uma esfera onde o lançamento vai terminar
        Gizmos.DrawWireSphere(endPoint, 0.1f);
        
        // Desenha uma linha para o grapple point (para contexto)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, grapplePoint);
        Gizmos.DrawSphere(grapplePoint, 0.1f);
    }
  
  
}