using UnityEngine;

public class PatrolMovement : MonoBehaviour, IMovementBehavior
{
    public float speed = 2f;
    public float patrolDistance = 4f; // quanto ele anda para cada lado

    private Rigidbody rb;
    private Vector3 startPos;
    private bool movingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
    }

    public void Move(Transform enemy)
    {
        float offset = enemy.position.x - startPos.x;

        // mudou de direção ao atingir a distância limite
        if (offset >= patrolDistance)
            movingRight = false;
        else if (offset <= -patrolDistance)
            movingRight = true;

        // define velocidade no eixo X (com gravidade ativa)
        rb.linearVelocity = new Vector3(movingRight ? speed : -speed, rb.linearVelocity.y, rb.linearVelocity.z);
    }
}
