using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WorldDraggableItem : MonoBehaviour
{
    [Header("Drag Visuals")]
    [SerializeField] private int draggingSortingOrder = 10;

    private SpriteRenderer spriteRenderer;
    private Collider2D myCollider;
    private int originalSortingOrder;
    private Vector3 originalPosition;

    public bool isConsumed { get; private set; }

    private Camera mainCamera;
    private float zOffset;
    private bool isDragging = false;

    //Initialize components
    private void Awake()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
        if (spriteRenderer != null) originalSortingOrder = spriteRenderer.sortingOrder;
    }

    //Handle drag state
    private void Update()
    {
        if (isConsumed) return;

        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            // Cegah klik drag jika kursor berada di atas elemen UI (Canvas)
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;

            if (myCollider.OverlapPoint(mouseWorldPos))
            {
                isDragging = true;
                originalPosition = transform.position;
                zOffset = transform.position.z;

                if (spriteRenderer != null) spriteRenderer.sortingOrder = draggingSortingOrder;
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            if (spriteRenderer != null) spriteRenderer.sortingOrder = originalSortingOrder;

            Collider2D[] hits = Physics2D.OverlapPointAll(transform.position);
            bool droppedInZone = false;

            foreach (var hit in hits)
            {
                WorldDropZone dropZone = hit.GetComponent<WorldDropZone>();
                if (dropZone != null)
                {
                    dropZone.HandleDrop(this); // Lapor ke wajan
                    droppedInZone = true;
                    break;
                }
            }

            if (!droppedInZone && !isConsumed)
            {
                transform.position = originalPosition;
            }
        }

        if (isDragging)
        {
            Vector3 newPos = mouseWorldPos;
            newPos.z = zOffset; // Kunci sumbu Z agar tidak hilang
            transform.position = newPos;
        }
    }

    //Hide item on valid drop
    public void Consume(Transform dropZoneCenter)
    {
        isConsumed = true;
        isDragging = false;
        transform.position = new Vector3(dropZoneCenter.position.x, dropZoneCenter.position.y, zOffset);
        gameObject.SetActive(false);
    }
}