using UnityEngine;

public class ArrowMovement : MonoBehaviour
{
private RectTransform rectTransform; 
    
    // Variável para armazenar a posição do objeto do mundo a seguir
    private Transform targetTransform; 
    
    // Variável opcional para ajustar a altura da seta acima do alvo
    public float verticalOffset = 1.5f; 
    public float rotationOffset = -90f; // Comece com -90f ou 0f

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // --- NOVO: Função para definir o alvo ---
    public void Initialize(Transform target)
    {
        targetTransform = target;
    }

    void Update()
    {
        if (targetTransform == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // --- PASSO 1: Posicionar a Seta UI (Continua a funcionar) ---
        Vector3 worldTargetPosition = targetTransform.position + Vector3.up * verticalOffset;
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldTargetPosition);
        rectTransform.position = screenPosition;


        // --- PASSO 2: Rodar a Seta para Apontar para o Rato ---

        // 2.1. Obter a posição do rato no mundo
        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        
        // 2.2. Calcular a direção: (Mouse Position) - (Arrow Position)
        Vector3 direction = mouseWorldPosition - targetTransform.position;
        direction.z = 0; // Garantir que a direção está no plano 2D

        // 2.3. Calcular o ângulo (em graus)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 2.4. Aplicar a Rotação com o Offset Ajustável
        // O valor 0, 0, angle faria a seta apontar para a direita (0º)
        rectTransform.localEulerAngles = new Vector3(0, 0, angle + rotationOffset);
    }

    // Função auxiliar (pode reutilizar a que tinha no PlayerMovement)
    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane gamePlane = new Plane(Vector3.forward, Vector3.zero); 
        float distance;

        if (gamePlane.Raycast(ray, out distance))
        {
            Vector3 worldPosition = ray.GetPoint(distance);
            worldPosition.z = 0; 
            return worldPosition;
        }
        return Vector3.zero;
    }
}
