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
    
    [Header("Impulso Ori (Bash)")]
    [SerializeField] private float bashForce = 50f; // Força do lançamento direcional
    [SerializeField] private float bashMaxDistance = 1.5f; // Distância do pull inicial (opcional)
    private bool isTargeting; // Novo estado para saber se está fixo e a apontar
    
    // Configurações do Spring Joint (para o balanço)
    [SerializeField] private float jointSpring = 4.5f;
    [SerializeField] private float jointDamper = 7f;
    [SerializeField] private float jointMassScale = 4.5f;
    [SerializeField] private float distanceReduction = 0.8f; // Puxa a distância para 80%

    private Vector3 grapplePoint;
    private SpringJoint joint; // 🚀 MUDANÇA 1: Componente 3D (SpringJoint)

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
    }

    //---------------------------------------------------------

    private void StopGrapple()
    {
        // Apenas se o gancho estava ativo (fixo ou a apontar)
        if (joint != null)
        {
            // 1. APLICAR O IMPULSO (BASH) - SOMENTE SE ESTAVA NO ESTADO DE APONTAR
            if (isTargeting)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Vetor do PONTO DE FIXAÇÃO para o JOGADOR
                    Vector3 launchDirection = (transform.position - grapplePoint).normalized;
                
                    // O Impulso é na DIREÇÃO OPOSTA ao ponto fixo.
                    // Usamos o Vector3.up como um vetor base para garantir o impulso para fora.
                
                    // Aplicar força. Use Vector3.up como o vetor para o 'Ori Bash'
                    // O Impulso do Ori é sempre na direção OPOSTA ao ponto de ancoragem,
                    // para impulsionar o jogador para longe.
                
                    rb.linearVelocity = Vector3.zero; // Resetar a velocidade antes do impulso
                    rb.AddForce(launchDirection * bashForce, ForceMode.VelocityChange); // 🚀 APLICAR FORÇA MÁXIMA
                }
            }
        
            // 2. Limpeza
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
    // 1. Verificar se já existe um joint ativo.
    if (joint != null) return;
    
    // 2. Procurar alvos Grappable na esfera de alcance.
    // Usamos OverlapSphere para detetar todos os coliders 3D na área.
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
    
    // 4. Congelar a Velocidade (Importante para a sensação de Ori Bash)
    Rigidbody rb = GetComponent<Rigidbody>();
    if (rb != null)
    {
        rb.linearVelocity = Vector3.zero; // Congela o movimento no ar
        rb.angularVelocity = Vector3.zero;
    }
    
    // 5. Adicionar e configurar o SpringJoint (Para fixar o jogador)
    joint = gameObject.AddComponent<SpringJoint>();
    joint.autoConfigureConnectedAnchor = false;
    joint.connectedAnchor = grapplePoint; 
    
    // Mola muito rígida para fixar o jogador quase instantaneamente no ponto
    joint.maxDistance = closestDistance; 
    joint.minDistance = closestDistance; 
    joint.spring = 1000f; 
    joint.damper = 10f; 
    joint.massScale = 1f;
    
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
}