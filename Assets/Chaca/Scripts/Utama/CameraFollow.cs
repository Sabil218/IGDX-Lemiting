using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow")]
    public float speed = 5f;

    [Header("Camera Bounds")]
    public BoxCollider2D cameraBounds;

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

        float targetX =
            target.position.x + offset.x;

        if (cameraBounds != null)
        {
            Camera cam = GetComponent<Camera>();

            if (cam != null && cam.orthographic)
            {
                float halfWidth =
                    cam.orthographicSize * cam.aspect;

                float minX =
                    cameraBounds.bounds.min.x + halfWidth;

                float maxX =
                    cameraBounds.bounds.max.x - halfWidth;

                targetX = Mathf.Clamp(
                    targetX,
                    minX,
                    maxX
                );
            }
            else
            {
                targetX = Mathf.Clamp(
                    targetX,
                    cameraBounds.bounds.min.x,
                    cameraBounds.bounds.max.x
                );
            }
        }

        Vector3 targetPosition = new Vector3(
            targetX,
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