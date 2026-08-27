using UnityEngine;

public class SeraiProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("Hit")]
    public float hitDistance = 0.8f;

    [Header("Lifetime")]
    public float lifeTime = 5f;

    [Header("Target")]
    public float targetHeight = 1f;

    private Transform player;
    private Vector3 direction;
    private int damage;
    private bool initialized;
    private bool hasHit;

    public void SetTarget(
        Transform targetPlayer,
        int projectileDamage
    )
    {
        player = targetPlayer;
        damage = projectileDamage;

        if (player == null)
        {
            Debug.LogError(
                "SERAI PROJECTILE: Player tidak ditemukan!"
            );

            return;
        }

        // =========================================
        // ARAHKAN KE BADAN PLAYER
        // BUKAN KE KAKI / PIVOT
        // =========================================

        Vector3 targetPosition =
            player.position +
            Vector3.up * targetHeight;

        direction =
            (
                targetPosition -
                transform.position
            ).normalized;

        initialized = true;

        Debug.Log(
            "SERAI PROJECTILE: Target Player ditemukan."
        );

        Debug.Log(
            "SERAI PROJECTILE: Target diarahkan ke badan Player."
        );

        // Hancur otomatis jika tidak mengenai Player
        Destroy(
            gameObject,
            lifeTime
        );
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (player == null)
        {
            Destroy(gameObject);
            return;
        }

        // Posisi projectile saat ini
        Vector3 currentPosition =
            transform.position;

        // Gerakan projectile
        Vector3 movement =
            direction *
            speed *
            Time.deltaTime;

        // Posisi berikutnya
        Vector3 nextPosition =
            currentPosition +
            movement;

        // =========================================
        // TARGET HIT DI BADAN PLAYER
        // =========================================

        Vector3 targetPosition =
            player.position +
            Vector3.up * targetHeight;

        // =========================================
        // CEK JARAK PROJECTILE KE BADAN PLAYER
        // =========================================

        float distanceCurrent =
            Vector3.Distance(
                currentPosition,
                targetPosition
            );

        float distanceNext =
            Vector3.Distance(
                nextPosition,
                targetPosition
            );

        // Kalau posisi sekarang ATAU posisi berikutnya
        // sudah cukup dekat dengan badan Player
        if (
            distanceCurrent <= hitDistance ||
            distanceNext <= hitDistance
        )
        {
            HitPlayer();
            return;
        }

        // Gerakkan projectile
        transform.position =
            nextPosition;
    }

    private void HitPlayer()
    {
        if (hasHit)
            return;

        hasHit = true;

        Debug.Log(
            "SERAI PROJECTILE KENA PLAYER!"
        );

        Player playerScript =
            player.GetComponent<Player>();

        if (playerScript == null)
        {
            playerScript =
                player.GetComponentInParent<Player>();
        }

        if (playerScript != null)
        {
            Debug.Log(
                "PLAYER SCRIPT DITEMUKAN"
            );

            playerScript.TakeDamage(
                damage
            );

            Debug.Log(
                "DAMAGE DIBERIKAN: " +
                damage
            );
        }
        else
        {
            Debug.LogError(
                "Player.cs tidak ditemukan!"
            );
        }

        // Projectile langsung hilang
        Destroy(gameObject);
    }
}