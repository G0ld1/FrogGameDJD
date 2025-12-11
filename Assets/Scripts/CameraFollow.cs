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
    public float directionChangeDelay = 0.15f; // Ajuste este valor (em segundos)!
    
    [Header("Free Look (Exploração)")]
    public float freeLookSpeed = 15f; 
    public float maxFreeLookDistance = 5f; 
    
    [Header("Fall Look-Ahead")]
    [Tooltip("A velocidade vertical mínima para ativar o offset de queda (valor negativo).")]
    public float fallThreshold = -8f; // Ex: -8 unidades/segundo (ajuste conforme a sua escala)
    
    [Tooltip("O offset Y máximo a aplicar quando a cair.")]
    public float maxFallOffset = -3f; 
    
    [Tooltip("A suavização para aplicar/remover o offset de queda.")]
    public float fallSmoothTime = 0.5f;
    
    // Variáveis internas para Free Look
    private Vector2 _freeLookInput = Vector2.zero; // Input do jogador (eixo do joystick/mouse)
    private Vector3 _currentFreeLookOffset = Vector3.zero; // Offset atual aplicado ao alvo
    private Vector3 _freeLookVelocity = Vector3.zero; // Velocidade de SmoothDamp para o Free Look

    
    private float _currentFallOffset = 0f;
    // Variáveis internas para rastreamento
    private Vector3 _velocity = Vector3.zero; 
    private Vector3 currentCameraPosition;
    
    // Variáveis de estado
    private float directionChangeTimer;
    private float currentFacingDirection = 1f; // Começa por assumir que está a olhar para a direita

    private void Update()
    {
        float lookX = Input.GetAxis("Mouse X"); 
        float lookY = Input.GetAxis("Mouse Y");
    
        _freeLookInput = new Vector2(lookX, lookY);

        // 2. Parar o Free Look se o input for pequeno
        if (_freeLookInput.magnitude < 0.1f)
        {
            _freeLookInput = Vector2.zero;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;
    
        // A. LÓGICA DE FREE LOOK (CLAMP)
    
        // O input desejado, normalizado para Z=0.
        Vector3 desiredInputVector = new Vector3(_freeLookInput.x, _freeLookInput.y, 0f);

        if (desiredInputVector.magnitude > 0.1f)
        {
            // Se há input: Move o offset diretamente na direção do input.
        
            // 1. Calcula o movimento que o input quer aplicar neste frame (Velocidade * Tempo)
            Vector3 frameMovement = desiredInputVector * freeLookSpeed * Time.deltaTime;

            // 2. Aplica o movimento
            _currentFreeLookOffset += frameMovement;
        
            // 3. CLAMP: Limita o deslocamento máximo para a 'maxFreeLookDistance'.
            // O offset pára de crescer assim que atinge este limite.
            _currentFreeLookOffset = Vector3.ClampMagnitude(_currentFreeLookOffset, maxFreeLookDistance);
        
            // Zera a velocidade de SmoothDamp para evitar interferência na próxima vez que retornar.
            _freeLookVelocity = Vector3.zero; 
        }
        else
        {
           
            _currentFreeLookOffset = Vector3.SmoothDamp(
                _currentFreeLookOffset, 
                Vector3.zero, 
                ref _freeLookVelocity, 
                smoothTime 
            );
        }
        
        
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb == null) return; 

        float currentVerticalVelocity = targetRb.linearVelocity.y;
    
      
        float desiredFallOffset = 0f;
    
        if (currentVerticalVelocity < fallThreshold)
        {
          
            float maxFallSpeed = -112f; 
        
            float t = Mathf.InverseLerp(fallThreshold, maxFallSpeed, currentVerticalVelocity);
        
            desiredFallOffset = Mathf.Lerp(0f, maxFallOffset, t);
        }

 
        _currentFallOffset = Mathf.SmoothDamp(
            _currentFallOffset, 
            desiredFallOffset, 
            ref _velocity.z, 
            fallSmoothTime
        );

   
        Vector3 effectiveTargetPosition = target.position + _currentFreeLookOffset;
       
        effectiveTargetPosition.y += _currentFallOffset;

        HandleDirectionDelay(); 
        CalculateDeadZoneFollow(effectiveTargetPosition); 
    }

    private void HandleDirectionDelay()

    {

        // 1. Determinar a Direção Atual do Jogador

                float actualDirection;

        // Se a rotação Y do jogador estiver entre 90º e 270º, está a olhar para a esquerda (-1).

                if (target.localEulerAngles.y > 90f && target.localEulerAngles.y < 270f)

                {

                    actualDirection = -1f;

                }

                else

                {

                    actualDirection = 1f; // Olhando para a direita

                }



        // 2. Verificar se a Direção Mudou

                if (actualDirection != currentFacingDirection)

                {

        // Se a direção atual é diferente da última direção registada, reinicia o timer

                    directionChangeTimer += Time.deltaTime;


        // Se o tempo de espera acabar, atualiza a direção

                    if (directionChangeTimer >= directionChangeDelay)

                    {

                        currentFacingDirection = actualDirection;

                        directionChangeTimer = 0f; // Reset do timer

                    }

                }

                else

                {

        // Se o jogador está a manter a direção, reinicia o timer a zero para a próxima mudança

                    directionChangeTimer = 0f;

                }

    }
    
    public void ResetCameraState()
    {
        _velocity = Vector3.zero; 
        _freeLookVelocity = Vector3.zero; 
    
        _currentFreeLookOffset = Vector3.zero;
        _currentFallOffset = 0f;
    
        Vector3 targetPos = target.position + staticOffset; 
    
        transform.position = targetPos;
        currentCameraPosition = targetPos; 
    }


    private void CalculateDeadZoneFollow(Vector3 effectiveTargetPosition)
    {
        currentCameraPosition = transform.position;
        
  
        float desiredXTarget = effectiveTargetPosition.x + (lookAheadOffset * currentFacingDirection);
        float desiredYTarget = effectiveTargetPosition.y + staticOffset.y; 

        Vector3 deadZoneCenter = new Vector3(currentCameraPosition.x, currentCameraPosition.y, 0);

        // X: SmoothDamp aplicado se o alvo (com look-ahead) sair da Dead Zone
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
        
        // Y: SmoothDamp aplicado se o alvo sair da Dead Zone
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
