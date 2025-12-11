using UnityEngine;

public interface IAttackBehavior
{
    void Attack(Transform enemyTransform, Transform playerTransform);
}
