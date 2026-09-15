using UnityEngine;

public class BawangProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float travelTime = 1f;
    public float arcHeight = 2f;

    [Header("Rotation")]
    public float throwRotation = -45f;
    public float fallRotation = 45f;
    public float rotationSpeed = 8f;

    private Transform target;
    private int damage;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private float timer;
    private bool launched;

    public void SetTarget(
        Transform newTarget,
        int newDamage
    )
    {
        target = newTarget;
        damage = newDamage;

        startPosition = transform.position;
        targetPosition = target.position;

        launched = true;

        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            throwRotation
        );
    }

    private void Update()
    {
        if (!launched)
            return;

        timer += Time.deltaTime;

        float progress = Mathf.Clamp01(
            timer / travelTime
        );

        Vector3 position = Vector3.Lerp(
            startPosition,
            targetPosition,
            progress
        );

        float height = Mathf.Sin(
            progress * Mathf.PI
        ) * arcHeight;

        position.y += height;

        transform.position = position;

        UpdateRotation(progress);

        if (progress >= 1f)
        {
            HitPlayer();
        }
    }

    private void UpdateRotation(float progress)
    {
        float rotationProgress =
            Mathf.Clamp01(
                (progress - 0.35f) / 0.4f
            );

        float angle = Mathf.Lerp(
            throwRotation,
            fallRotation,
            rotationProgress
        );

        Quaternion targetRotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );

        transform.rotation =
            Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
    }

    private void HitPlayer()
    {
        if (target != null)
        {
            Player playerScript =
                target.GetComponent<Player>();

            if (playerScript != null)
            {
                playerScript.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}