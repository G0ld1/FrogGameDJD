using System.Collections;
using UnityEngine;

public class ProjectileAttack : MonoBehaviour, IAttackBehavior
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float cooldown = 1f;
    private bool canAttack = true;

    public void Attack(Transform enemy, Transform player)
    {
        if (!canAttack) return;

        Vector3 dirToPlayer = (player.position - firePoint.position).normalized;
        
            Quaternion spreadRotation = Quaternion.LookRotation(dirToPlayer) * Quaternion.Euler(0, 0, 0);
        
            Instantiate(projectilePrefab, firePoint.position, spreadRotation);
        

        enemy.GetComponent<MonoBehaviour>().StartCoroutine(CD());
    }

    IEnumerator CD()
    {
        canAttack = false;
        yield return new WaitForSeconds(cooldown);
        canAttack = true;
    }
}
