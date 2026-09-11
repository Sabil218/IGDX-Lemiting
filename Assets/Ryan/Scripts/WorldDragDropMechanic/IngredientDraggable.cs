using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class IngredientDraggable : MonoBehaviour
{
    [Header("Hierarchy Options")]
    [Tooltip("Jika diisi, objek ini yang akan pindah posisinya (berguna jika script ada di child). Jika kosong, ia menggerakkan dirinya sendiri.")]
    public Transform rootToDrag;

    [Header("Drop Options")]
    [Tooltip("Jika false, barang tidak akan di-hide (hilang) saat berhasil masuk wajan/piring. Berguna untuk wajan yang akan dipakai cutscene.")]
    public bool hideOnConsume = true;

    [Tooltip("Jika true, objek kembali ke posisi awal setelah drop berhasil (tidak di-consume).")]
    public bool returnAfterDrop = false;

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
    private Vector3 dragOffset;
    
    private Coroutine activeMoveCoroutine;
    private bool isReturning = false;

    //Initialize components
    private void Awake()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
        if (spriteRenderer != null) originalSortingOrder = spriteRenderer.sortingOrder;

        if (rootToDrag == null) rootToDrag = transform; // Auto assign if empty
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
                
                if (activeMoveCoroutine != null)
                {
                    StopCoroutine(activeMoveCoroutine);
                    activeMoveCoroutine = null;
                }

                if (!isReturning)
                {
                    originalPosition = rootToDrag.position;
                }
                isReturning = false;

                zOffset = rootToDrag.position.z;
                dragOffset = rootToDrag.position - (Vector3)mouseWorldPos;

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
                CookingDropZone dropZone = hit.GetComponent<CookingDropZone>();
                if (dropZone != null)
                {
                    if (returnAfterDrop)
                    {
                        dropZone.HandleDropNoConsume(this);
                        if (activeMoveCoroutine != null) StopCoroutine(activeMoveCoroutine);
                        activeMoveCoroutine = StartCoroutine(SmoothMoveBack(rootToDrag, originalPosition, 0.2f));
                        droppedInZone = true;
                    }
                    else
                    {
                        dropZone.HandleDrop(this); // Lapor ke wajan
                        droppedInZone = true;
                    }
                    break;
                }
            }

            if (!droppedInZone && !isConsumed)
            {
                if (activeMoveCoroutine != null) StopCoroutine(activeMoveCoroutine);
                activeMoveCoroutine = StartCoroutine(SmoothMoveBack(rootToDrag, originalPosition, 0.2f));
            }
        }

        if (isDragging)
        {
            Vector3 newPos = (Vector3)mouseWorldPos + dragOffset;
            newPos.z = zOffset; // Kunci sumbu Z agar tidak hilang
            rootToDrag.position = newPos;
        }
    }

    //Hide item on valid drop
    public void Consume(Transform dropZoneCenter)
    {
        isConsumed = true;
        isDragging = false;
        
        if (activeMoveCoroutine != null)
        {
            StopCoroutine(activeMoveCoroutine);
            activeMoveCoroutine = null;
        }

        // Jika wajan global aktif, jadikan item ini anak dari wadah bahan mentah
        if (CookingVisualController.Instance != null && CookingVisualController.Instance.rawIngredientsContainer != null)
        {
            rootToDrag.SetParent(CookingVisualController.Instance.rawIngredientsContainer, true);
            
            // Tambahkan sedikit random offset agar tumpukan tidak persis di satu titik
            Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
            Vector3 targetPos = new Vector3(dropZoneCenter.position.x + randomOffset.x, dropZoneCenter.position.y + randomOffset.y, zOffset);
            
            StartCoroutine(SmoothConsume(rootToDrag, targetPos, 0.15f));
        }
        else
        {
            Vector3 targetPos = new Vector3(dropZoneCenter.position.x, dropZoneCenter.position.y, zOffset);
            StartCoroutine(SmoothConsume(rootToDrag, targetPos, 0.15f));
        }
    }

    private System.Collections.IEnumerator SmoothMoveBack(Transform target, Vector3 targetPosition, float duration)
    {
        isReturning = true;
        Vector3 startPosition = target.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            float easedT = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f); // EaseOutBack

            target.position = Vector3.LerpUnclamped(startPosition, targetPosition, easedT);
            yield return null;
        }
        target.position = targetPosition;
        isReturning = false;
        activeMoveCoroutine = null;
    }

    private System.Collections.IEnumerator SmoothConsume(Transform target, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = target.position;
        Vector3 startScale = target.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            target.position = Vector3.Lerp(startPosition, targetPosition, t);
            
            if (hideOnConsume)
            {
                target.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            }

            yield return null;
        }

        target.position = targetPosition;
        if (hideOnConsume)
        {
            target.localScale = Vector3.zero;
            target.gameObject.SetActive(false);
            target.localScale = startScale;
        }
    }
}