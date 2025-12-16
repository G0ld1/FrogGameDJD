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
    
    [Header("Free Look (Exploração)")]
    public float freeLookSpeed = 15f; 
    public float maxFreeLookDistance = 5f; 
    
    [Header("Fall Look-Ahead")]
    [Tooltip("A velocidade vertical mínima para ativar o offset de queda (valor negativo).")]
    public float fallThreshold = -8f; 
    
    [Tooltip("O offset Y máximo a aplicar quando a cair.")]
    public float maxFallOffset = -3f; 
    
    [Tooltip("A suavização para aplicar/remover o offset de queda.")]
    public float fallSmoothTime = 0.5f;
    
    // Variáveis internas para Free Look
    private Vector2 _freeLookInput = Vector2.zero; 
    private Vector3 _currentFreeLookOffset = Vector3.zero; 
    private Vector3 _freeLookVelocity = Vector3.zero; 

    
    private float _currentFallOffset = 0f;
    // Variáveis internas para rastreamento
    private Vector3 _velocity = Vector3.zero; 
    private Vector3 currentCameraPosition;
    
    // Variáveis de estado
    private float directionChangeTimer;
    private float currentFacingDirection = 1f; 

    private void Update()
    {
        // Capturar Input de Free Look
        float lookX = Input.GetAxis("Mouse X"); 
        float lookY = Input.GetAxis("Mouse Y");
    
        _freeLookInput = new Vector2(lookX, lookY);

        // Parar o Free Look se o input for pequeno
        if (_freeLookInput.magnitude < 0.1f)
        {
            _freeLookInput = Vector2.zero;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;
    
        // A. LÓGICA DE FREE LOOK (CLAMP)
        Vector3 desiredInputVector = new Vector3(_freeLookInput.x, _freeLookInput.y, 0f);

        if (desiredInputVector.magnitude > 0.1f)
        {
            // Aplica o movimento e limita (Clamp)
            Vector3 frameMovement = desiredInputVector * freeLookSpeed * Time.deltaTime;
            _currentFreeLookOffset += frameMovement;
            _currentFreeLookOffset = Vector3.ClampMagnitude(_currentFreeLookOffset, maxFreeLookDistance);
            _freeLookVelocity = Vector3.zero; 
        }
        else
        {
            // Suaviza o retorno para o centro
            _currentFreeLookOffset = Vector3.SmoothDamp(
                _currentFreeLookOffset, 
                Vector3.zero, 
                ref _freeLookVelocity, 
                smoothTime 
            );
        }
        
        // B. LÓGICA DE FALL LOOK-AHEAD
        
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

        // C. DETERMINAR O ALVO FINAL
        Vector3 effectiveTargetPosition = target.position + _currentFreeLookOffset;
        effectiveTargetPosition.y += _currentFallOffset;

        // D. EXECUÇÃO DO FOLLOW
        HandleDirectionDelay(); 
        CalculateDeadZoneFollow(effectiveTargetPosition); 
    }
    
    // CRÍTICO PARA O RESPAWN
    public void ResetCameraState()
    {
      
        
        // 2. Zera todas as velocidades de SmoothDamp
        _velocity = Vector3.zero; 
        _freeLookVelocity = Vector3.zero; 
        
        // 3. Zera todos os offsets
        _currentFreeLookOffset = Vector3.zero;
        _currentFallOffset = 0f;
    
        // 4. Teletransporta a câmara para a posição de spawn instantaneamente
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