using UnityEngine;

public class EnemyCore : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRange = 5f;

    private IMovementBehavior movement;
    private IAttackBehavior attack;
    private Transform player;

    void Awake()
    {
        movement = GetComponent<IMovementBehavior>();
        attack = GetComponent<IAttackBehavior>();
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        movement?.Move(transform);

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= detectionRange)
            attack?.Attack(transform, player);
    }
}
