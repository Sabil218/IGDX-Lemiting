using UnityEngine;

public class JaheProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;

    [Header("Hit")]
    public float hitDistance = 0.5f;
    public int damage = 1;

    private Transform player;
    private float direction;

    public void SetTarget(Transform target, int projectileDamage)
    {
        player = target;
        damage = projectileDamage;

        if (player == null)
        {
            Debug.LogError("Target Player tidak ditemukan!");
            return;
        }

        if (player.position.x > transform.position.x)
        {
            direction = 1f;
        }
        else
        {
            direction = -1f;
        }

        Debug.Log("Projectile mendapatkan target Player.");
    }

    private void Update()
    {
        transform.position +=
            Vector3.right * direction * speed * Time.deltaTime;

        if (player == null)
            return;

        float distance = Mathf.Abs(
            transform.position.x - player.position.x
        );

        if (distance <= hitDistance)
        {
            HitPlayer();
        }
    }

    private void HitPlayer()
    {
        Debug.Log("PROJECTILE KENA PLAYER");

        Player playerScript = player.GetComponent<Player>();

        if (playerScript != null)
        {
            Debug.Log("PLAYER SCRIPT DITEMUKAN");

            playerScript.TakeDamage(damage);

            Debug.Log("DAMAGE DIBERIKAN : " + damage);
        }
        else
        {
            Debug.LogError(
                "Player.cs tidak ditemukan pada target Player!"
            );
        }

        Destroy(gameObject);
    }
}