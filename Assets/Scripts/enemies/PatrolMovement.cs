using UnityEngine;

public class PatrolMovement : MonoBehaviour, IMovementBehavior
{
    public enum MovementAxis { Horizontal, Vertical }
    
    [Header("Configuração de Patrulha")]
    public MovementAxis movementType = MovementAxis.Horizontal;
    public float speed = 2f;
    public float patrolDistance = 4f;

    private Rigidbody rb;
    private Vector3 startPos;
    private bool movingForward = true; 
    public Animator pedra_animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        if (pedra_animator == null) pedra_animator = GetComponent<Animator>();
        
      
        if (movementType == MovementAxis.Vertical)
        {
            rb.useGravity = false;
        }
    }

    public void Move(Transform enemy)
    {
        if (pedra_animator != null) pedra_animator.SetBool("Andar", true);

        if (movementType == MovementAxis.Horizontal)
        {
            HandleHorizontal(enemy);
        }
        else
        {
            HandleVertical(enemy);
        }
    }

    private void HandleHorizontal(Transform enemy)
    {
        float offset = enemy.position.x - startPos.x;

        if (offset >= patrolDistance) movingForward = false;
        else if (offset <= -patrolDistance) movingForward = true;

        rb.linearVelocity = new Vector3(movingForward ? speed : -speed, rb.linearVelocity.y, 0f);
    }

    private void HandleVertical(Transform enemy)
    {
        float offset = enemy.position.y - startPos.y;

        if (offset >= patrolDistance) movingForward = false;
        else if (offset <= -patrolDistance) movingForward = true;

  
        rb.linearVelocity = new Vector3(0f, movingForward ? speed : -speed, 0f);
    }
}
