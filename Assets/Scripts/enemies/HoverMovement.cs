using UnityEngine;

public class HoverMovement : MonoBehaviour, IMovementBehavior
{
    public float amplitude = 0.2f;
    public float speed = 2f;

    private float baseY;
    private bool isPaused = false;

    void Start()
    {
        baseY = transform.position.y;
    }

    public void Move(Transform enemy)
    {
        if (isPaused) return;

        enemy.position = new Vector3(
            enemy.position.x,
            baseY + Mathf.Sin(Time.time * speed) * amplitude,
            enemy.position.z
        );
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("Player"))
        {
            isPaused = true;
        }
    }

    private void OnCollisionExit(Collision col)
    {
        if (col.collider.CompareTag("Player"))
        {
            isPaused = false;
        }
    }
}

