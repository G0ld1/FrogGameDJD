using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Alvo e Suavização")]
    [Tooltip("O Transform do jogador a seguir.")]
    public Transform target; 
    
    [Tooltip("Controla a suavidade do seguimento. Valores baixos = mais lag.")]
    [Range(0.01f, 1f)]
    public float smoothSpeed = 0.125f; 

    [Header("Configuração 2.5D")]
    [Tooltip("Deslocamento da câmara em relação ao alvo (X, Y, Z).")]
    public Vector3 offset = new Vector3(0f, 1.5f, -10f); 
    
    [Tooltip("Opcional: Limita a posição Z da câmara. Manter o Z fixo é vital para 2.5D.")]
    public bool lockZ = true; 

    private void FixedUpdate()
    {
        // Verificar se o alvo foi definido
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: Alvo não definido. Por favor, arraste o jogador para o campo 'Target' no Inspector.");
            return;
        }

        FollowTarget();
    }

    private void FollowTarget()
    {
        // 1. Calcular a posição desejada
        // A posição desejada é a posição do alvo + o offset
        Vector3 desiredPosition = target.position + offset;
        
        // 2. Aplicar Restrição Z (Essencial para 2.5D)
        if (lockZ)
        {
            // Fixa a posição Z da câmara (usando o Z do offset)
            desiredPosition.z = offset.z;
        }

        // 3. Aplicar a Suavização (Lag Behind)
        // Lerp move a câmara da posição atual para a posição desejada de forma suave.
        // O valor 'smoothSpeed' controla a rapidez (lag)
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position, 
            desiredPosition, 
            smoothSpeed * Time.fixedDeltaTime
        );

        // 4. Aplicar a nova posição da câmara
        transform.position = smoothedPosition;

        // Opcional: A câmara pode "olhar" para o alvo, mas para um platformer 2.5D, 
        // muitas vezes a rotação é mantida estática.
        // transform.LookAt(target); 
    }
}
