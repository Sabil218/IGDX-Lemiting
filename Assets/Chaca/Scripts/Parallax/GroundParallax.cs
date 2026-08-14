using UnityEngine;

public class GroundParallax : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Ground")]
    public float groundWidth = 20f;
    public int groundCount = 3;

    private Transform[] tiles;

    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player belum diisi!");
            return;
        }

        // Buat tile ground
        tiles = new Transform[groundCount];

        for (int i = 0; i < groundCount; i++)
        {
            GameObject tile = Instantiate(
                gameObject,
                transform.position + Vector3.right * (groundWidth * i),
                transform.rotation,
                transform.parent
            );

            // Jangan biarkan tile membuat tile lagi
            if (i == 0)
            {
                tiles[i] = transform;
            }
            else
            {
                tiles[i] = tile.transform;
            }
        }
    }

    void Update()
    {
        if (player == null || tiles == null)
            return;

        for (int i = 0; i < tiles.Length; i++)
        {
            Transform tile = tiles[i];

            // Kalau tile sudah jauh di belakang player
            if (tile.position.x + groundWidth < player.position.x)
            {
                // Cari tile yang paling depan
                Transform furthestTile = tiles[0];

                for (int j = 1; j < tiles.Length; j++)
                {
                    if (tiles[j].position.x > furthestTile.position.x)
                    {
                        furthestTile = tiles[j];
                    }
                }

                // Pindahkan tile ke paling depan
                tile.position = new Vector3(
                    furthestTile.position.x + groundWidth,
                    tile.position.y,
                    tile.position.z
                );
            }
        }
    }
}