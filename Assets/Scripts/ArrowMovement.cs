using UnityEngine;

public class ArrowMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   

    // Update is called once per frame

    // Update is called once per frame
    private RectTransform rectTransform; 

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // --- PASSO 1: Obter a posição do rato relativa ao mundo ---
        // Isto é necessário porque o seu Canvas pode não ser 'Overlay'.
        Vector3 mouseWorldPosition = Vector3.zero;
    
        // Obter a posição no mundo, convertida do ecrã
        // (Isto depende do seu Canvas estar em Screen Space - Camera ou World Space.
        // Para a maioria dos Canvas, ScreenPointToRay é mais seguro.)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, rectTransform.position); // Usa o plano do elemento
        float distance;

        if (plane.Raycast(ray, out distance))
        {
            mouseWorldPosition = ray.GetPoint(distance);
        }
    
        // --- PASSO 2: Calcular a direção no plano XY ---
        // A direção é do centro do elemento para a posição do rato no mundo.
        Vector3 direction = mouseWorldPosition - transform.position;
    
        // Como estamos em 2.5D, ignoramos a profundidade Z
        direction.z = 0; 

        // --- PASSO 3: Calcular o ângulo em graus (Atan2) ---
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // --- PASSO 4: Aplicar a Rotação ao RectTransform ---
        // A rotação Z (Yaw) é o que faz o elemento girar no plano do ecrã.
        // Subtraímos 90 graus porque o elemento UI padrão aponta para CIMA (+Y) 
        // quando o ângulo é 90º. O Atan2 calcula a partir do eixo X.
    
        // Aplicação Final:
        // **NOTA CRÍTICA:** Se a imagem não for um círculo ou ponto, pode precisar de um offset (-90f).
        rectTransform.localEulerAngles = new Vector3(0, 0, angle); 
    
        // Se o elemento estiver virado para cima por padrão (como a maioria das imagens/sprites):
        // rectTransform.localEulerAngles = new Vector3(0, 0, angle - 90f); 
    }
}
