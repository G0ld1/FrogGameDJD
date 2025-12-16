using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    public float knockbackSpeed = 6f;     // velocidade horizontal
    public float upwardSpeed = 2f;         // lift leve
    public float duration = 0.12f;         // MUITO curto

    private Rigidbody rb;
    private float timer;
    private bool isKnocked;
    private Vector3 knockDir;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ApplyKnockback(Vector3 sourcePosition)
    {
        isKnocked = true;
        timer = duration;

        knockDir = (transform.position - sourcePosition);
        knockDir.y = 0f;
        knockDir.Normalize();
    }

    void FixedUpdate()
    {
        if (!isKnocked) return;

        timer -= Time.fixedDeltaTime;

        // VELOCIDADE CONTROLADA
        rb.linearVelocity = new Vector3(
            knockDir.x * knockbackSpeed,
            upwardSpeed,
            0f
        );

        if (timer <= 0f)
        {
            isKnocked = false;
        }
    }

    public bool IsKnocked()
    {
        return isKnocked;
    }
}
