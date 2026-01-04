using System;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Alvo e Suavização")]
    public Transform target;
    [Range(0.01f, 1f)]
    public float smoothTime = 0.3f; 

    [Header("Configuração 2.5D")]
    public Vector3 staticOffset = new Vector3(0f, 1.5f, -10f); 
    public float lookAheadOffset = 3.5f; 

    [Header("Zona Morta (Dead Zone)")]
    public float deadZoneWidth = 2f; 
    public float deadZoneHeight = 1f;

    [Header("Atraso de Direção")]
    [Tooltip("Tempo que o jogador tem de manter a nova direção antes da câmara mudar o look-ahead.")]
    public float directionChangeDelay = 0.15f; 
    
    [Header("Fall Look-Ahead")]
    [Tooltip("A velocidade vertical mínima para ativar o offset de queda (valor negativo).")]
    public float fallThreshold = -8f; 
    
    [Tooltip("O offset Y máximo a aplicar quando a cair.")]
    public float maxFallOffset = -3f; 
    
    [Tooltip("A suavização para aplicar/remover o offset de queda.")]
    public float fallSmoothTime = 0.5f;
    
    // Variáveis internas
    private float _currentFallOffset = 0f;
    private Vector3 _velocity = Vector3.zero; 
    private Vector3 currentCameraPosition;
    
    // Variáveis de estado
    private float directionChangeTimer;
    private float currentFacingDirection = 1f; 

    private void LateUpdate()
    {
        if (target == null) return;
        
        // A. LÓGICA DE FALL LOOK-AHEAD
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb == null) return; 

        float currentVerticalVelocity = targetRb.linearVelocity.y;
        float desiredFallOffset = 0f;
    
        // Usa o threshold para ativar o offset de queda
        if (currentVerticalVelocity < fallThreshold)
        {
            float maxFallSpeed = -112f; 
        
            float t = Mathf.InverseLerp(fallThreshold, maxFallSpeed, currentVerticalVelocity);
        
            desiredFallOffset = Mathf.Lerp(0f, maxFallOffset, t);
        }

        // Suaviza a aplicação do Fall Offset
        _currentFallOffset = Mathf.SmoothDamp(
            _currentFallOffset, 
            desiredFallOffset, 
            ref _velocity.z, 
            fallSmoothTime
        );

        // B. DETERMINAR O ALVO FINAL
        Vector3 effectiveTargetPosition = target.position;
        effectiveTargetPosition.y += _currentFallOffset;

        // C. EXECUÇÃO DO FOLLOW
        HandleDirectionDelay(); 
        CalculateDeadZoneFollow(effectiveTargetPosition); 
    }
    
    // CRÍTICO PARA O RESPAWN
    public void ResetCameraState()
    {
        // 1. Zera todas as velocidades de SmoothDamp
        _velocity = Vector3.zero; 
        
        // 2. Zera o offset de queda
        _currentFallOffset = 0f;
    
        // 3. Teletransporta a câmara para a posição de spawn instantaneamente
        Vector3 targetPos = target.position + staticOffset; 
    
        currentCameraPosition = targetPos; 
        transform.position = targetPos;
    }

    private void HandleDirectionDelay()
    {
        float actualDirection;

        if (target.localEulerAngles.y > 90f && target.localEulerAngles.y < 270f)
        {
            actualDirection = -1f;
        }
        else
        {
            actualDirection = 1f; 
        }

        if (actualDirection != currentFacingDirection)
        {
            directionChangeTimer += Time.deltaTime;

            if (directionChangeTimer >= directionChangeDelay)
            {
                currentFacingDirection = actualDirection;
                directionChangeTimer = 0f; 
            }
        }
        else
        {
            directionChangeTimer = 0f;
        }
    }

    private void CalculateDeadZoneFollow(Vector3 effectiveTargetPosition)
    {
        currentCameraPosition = transform.position;
        
        // 1. Calcular Posição Alvo Desejada
        float desiredXTarget = effectiveTargetPosition.x + (lookAheadOffset * currentFacingDirection);
        float desiredYTarget = effectiveTargetPosition.y + staticOffset.y; 

        // --- Lógica da Zona Morta (Dead Zone) ---
        Vector3 deadZoneCenter = new Vector3(currentCameraPosition.x, currentCameraPosition.y, 0);

        // X: SmoothDamp
        float horizontalDistance = desiredXTarget - deadZoneCenter.x;

        if (Mathf.Abs(horizontalDistance) > deadZoneWidth / 2f)
        {
            currentCameraPosition.x = Mathf.SmoothDamp(
                currentCameraPosition.x, 
                desiredXTarget, 
                ref _velocity.x, 
                smoothTime
            );
        }
        
        // Y: SmoothDamp
        float verticalDistance = desiredYTarget - deadZoneCenter.y;

        if (Mathf.Abs(verticalDistance) > deadZoneHeight / 2f)
        {
             currentCameraPosition.y = Mathf.SmoothDamp(
                currentCameraPosition.y, 
                desiredYTarget, 
                ref _velocity.y, 
                smoothTime
            );
        }
        
        // 3. Aplicar a Restrição Z (offset estático)
        currentCameraPosition.z = staticOffset.z;

        // 4. Aplicar a nova posição da câmara
        transform.position = currentCameraPosition;
    }
}