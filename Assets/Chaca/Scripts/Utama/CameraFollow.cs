using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float speed = 5f;

    private Vector3 offset;
    private bool isFollowing;

    private void Start()
    {
        isFollowing = false;

        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (!isFollowing)
            return;

        Vector3 targetPosition = new Vector3(
            target.position.x + offset.x,
            transform.position.y,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );
    }

    public void StartFollowing()
    {
        if (target == null)
            return;

        offset = transform.position - target.position;
        isFollowing = true;
    }

    public void StopFollowing()
    {
        isFollowing = false;
    }

    public void ResetFollow()
    {
        isFollowing = false;

        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }
}