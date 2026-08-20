using UnityEngine;

public class CloudParallax : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Camera")]
    public Camera cam;

    [Header("Movement")]
    [Tooltip("Pengaruh gerakan player terhadap awan.")]
    public float parallaxStrength = 0.3f;

    [Tooltip("Kecepatan awan saat player diam.")]
    public float cloudSpeed = 0.2f;

    [Header("Infinite")]
    [Tooltip("Jarak awan muncul di luar sisi kanan kamera.")]
    public float spawnDistance = 2f;

    private float lastPlayerX;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
                player = playerObject.transform;
        }

        if (cam == null)
            cam = Camera.main;

        if (player != null)
            lastPlayerX = player.position.x;
    }

    void Update()
    {
        if (player == null || cam == null)
            return;

        // Hitung pergerakan player
        float playerMovement =
            player.position.x - lastPlayerX;

        // Player bergerak ke kanan
        // Awan bergerak ke kiri
        float parallaxMovement =
            -playerMovement * parallaxStrength;

        // Awan tetap bergerak perlahan
        // walaupun player sedang diam
        float naturalMovement =
            -cloudSpeed * Time.deltaTime;

        transform.position += new Vector3(
            parallaxMovement + naturalMovement,
            0f,
            0f
        );

        lastPlayerX = player.position.x;

        CheckCameraBounds();
    }

    void CheckCameraBounds()
    {
        Vector3 viewportPosition =
            cam.WorldToViewportPoint(transform.position);

        // Kalau awan sudah keluar dari sisi kiri kamera
        if (viewportPosition.x < 0f)
        {
            MoveToRightSide();
        }
    }

    void MoveToRightSide()
    {
        float cameraHeight = cam.orthographicSize;

        float cameraWidth =
            cameraHeight * cam.aspect;

        // Pindahkan awan ke sisi kanan kamera
        float newX =
            cam.transform.position.x
            + cameraWidth
            + spawnDistance;

        transform.position = new Vector3(
            newX,
            transform.position.y,
            transform.position.z
        );
    }
}