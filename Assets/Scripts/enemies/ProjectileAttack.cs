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

        Vector3 dir = (player.position - firePoint.position).normalized;
        Quaternion rot = Quaternion.LookRotation(dir);

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, rot);

        enemy.GetComponent<MonoBehaviour>().StartCoroutine(CD());
    }

    IEnumerator CD()
    {
        canAttack = false;
        yield return new WaitForSeconds(cooldown);
        canAttack = true;
    }
}
