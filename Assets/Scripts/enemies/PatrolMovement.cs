using UnityEngine;

public class PatrolMovement : MonoBehaviour, IMovementBehavior
{
    public float speed = 2f;
    public float patrolDistance = 4f; // quanto ele anda para cada lado

    private Rigidbody rb;
    private Vector3 startPos;
    private bool movingRight = true;
    public Animator pedra_animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        pedra_animator = GetComponent<Animator>();
    }

    public void Move(Transform enemy)
    {
        pedra_animator.SetBool("Andar", true);
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
