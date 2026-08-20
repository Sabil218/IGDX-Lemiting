using UnityEngine;

public class InfiniteBackground : MonoBehaviour
{
    public Transform cameraTransform;

    public Transform bg1;
    public Transform bg2;

    private float width;

    void Start()
    {
        SpriteRenderer sr = bg1.GetComponent<SpriteRenderer>();
        width = sr.bounds.size.x;
    }

    void Update()
    {
        // BG 1 sudah terlalu jauh ke kiri
        if (bg1.position.x + width < cameraTransform.position.x)
        {
            bg1.position = new Vector3(
                bg2.position.x + width,
                bg1.position.y,
                bg1.position.z
            );
        }

        // BG 2 sudah terlalu jauh ke kiri
        if (bg2.position.x + width < cameraTransform.position.x)
        {
            bg2.position = new Vector3(
                bg1.position.x + width,
                bg2.position.y,
                bg2.position.z
            );
        }
    }
}
