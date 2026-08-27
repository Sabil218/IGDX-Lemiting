using UnityEngine;

public class PlayerBoomerang : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;

    [Header("Rotation")]
    public float rotationSpeed = 720f;

    [Header("Return")]
    public float returnDistance = 0.3f;

    private Transform player;
    private Transform target;
    private IDamageable damageableTarget;

    private int damage;

    private bool goingToTarget = true;
    private bool hasHit = false;

    private float fixedY;
    private float fixedZ;

    public void SetTarget(
        Transform playerTarget,
        Transform enemyTarget,
        int boomerangDamage)
    {
        player = playerTarget;
        target = enemyTarget;
        damage = boomerangDamage;

        if (target != null)
        {
            damageableTarget =
                target.GetComponent<IDamageable>();
        }

        if (damageableTarget == null)
        {
            Debug.LogError(
                "Target tidak memiliki IDamageable!"
            );

            Destroy(gameObject);
            return;
        }

        fixedY = transform.position.y;
        fixedZ = transform.position.z;

        goingToTarget = true;
        hasHit = false;
    }

    private void Update()
    {
        if (player == null)
        {
            Destroy(gameObject);
            return;
        }

        RotateBoomerang();

        if (goingToTarget)
        {
            MoveToTarget();
        }
        else
        {
            MoveBackToPlayer();
        }
    }

    private void RotateBoomerang()
    {
        transform.Rotate(
            0f,
            0f,
            rotationSpeed * Time.deltaTime
        );
    }

    private void MoveToTarget()
    {
        if (target == null)
        {
            goingToTarget = false;
            return;
        }

        float targetX = target.position.x;

        Vector3 targetPosition = new Vector3(
            targetX,
            fixedY,
            fixedZ
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        if (Mathf.Abs(
            transform.position.x - targetX
        ) <= 0.2f)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        if (hasHit)
            return;

        hasHit = true;

        Debug.Log("Boomerang mengenai target.");

        if (damageableTarget != null)
        {
            damageableTarget.TakeDamage(damage);
        }

        goingToTarget = false;
    }

    private void MoveBackToPlayer()
    {
        float playerX = player.position.x;

        Vector3 playerPosition = new Vector3(
            playerX,
            fixedY,
            fixedZ
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            playerPosition,
            speed * Time.deltaTime
        );

        if (Mathf.Abs(
            transform.position.x - playerX
        ) <= returnDistance)
        {
            Debug.Log("Boomerang kembali ke Player.");

            Destroy(gameObject);
        }
    }
}