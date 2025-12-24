using UnityEngine;

public class EnemyCore : MonoBehaviour
{
   [Header("Detection")]
    public float detectionRange = 5f;
    [Tooltip("Define as layers que bloqueiam a visão (ex: Ground, Wall)")]
    public LayerMask obstacleLayers; 

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
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (player == null) return;

        // O movimento continua a processar (patrulha/hover)
        movement?.Move(transform);

        float dist = Vector3.Distance(transform.position, player.position);

        // Só tenta atacar se estiver dentro do range E se tiver visão clara
        if (dist <= detectionRange && HasLineOfSight())
        {
            attack?.Attack(transform, player);
        }
    }

    private bool HasLineOfSight()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Lança um raio do inimigo para o jogador
        // Se o raio bater em algo que esteja nas 'obstacleLayers' antes de chegar ao player, retorna false
        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, distanceToPlayer, obstacleLayers))
        {
            // Se o raio atingiu algo, e esse algo NÃO é o jogador, a visão está bloqueada
            if (!hit.transform.CompareTag("Player"))
            {
                return false;
            }
        }

        // Se o raio não bateu em nada ou bateu direto no Player, a visão está limpa
        return true;
    }

    // Desenha o raio no editor para te ajudar a debugar
    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            bool visible = HasLineOfSight();
            Gizmos.color = visible ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
