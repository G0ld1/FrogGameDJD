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

    // Variáveis internas para rastreamento
    private Vector3 _velocity = Vector3.zero; 
    private Vector3 currentCameraPosition;
    
    // Variáveis de estado
    private float directionChangeTimer;
    private float currentFacingDirection = 1f; // Começa por assumir que está a olhar para a direita

    private void LateUpdate()
    {
        if (target == null) return;
        HandleDirectionDelay(); // Chama primeiro a nova lógica de atraso
        CalculateDeadZoneFollow();
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

    private void CalculateDeadZoneFollow()
    {
        currentCameraPosition = transform.position;
        
        // 1. Calcular a Posição Alvo Desejada no X (Usando a direção com DELAY)
        // O look-ahead só muda quando o currentFacingDirection for atualizado em HandleDirectionDelay()
        float desiredXTarget = target.position.x + (lookAheadOffset * currentFacingDirection);
        float desiredYTarget = target.position.y + staticOffset.y;

        // --- Lógica da Zona Morta (Dead Zone) ---

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
