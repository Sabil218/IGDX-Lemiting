using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WorldObjectDraggable : MonoBehaviour
{
    [Header("Hierarchy Options")]
    public Transform rootToDrag;

    [Header("Drop Options")]
    public bool hideOnConsume = true;
    public bool returnAfterDrop = false;
    public bool isCookingIngredient = false;

    [Header("Drag Visuals")]
    [SerializeField] private int sortingOrderBoost = 10;

    private SpriteRenderer[] allRenderers;
    private Collider2D myCollider;
    private Vector3 originalPosition;

    public bool isConsumed { get; private set; }

    private Camera mainCamera;
    private float zOffset;
    private bool isDragging = false;
    private Vector3 dragOffset;
    
    private Coroutine activeMoveCoroutine;
    private bool isReturning = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        myCollider = GetComponent<Collider2D>();
        
        if (rootToDrag == null) rootToDrag = transform;
        
        allRenderers = rootToDrag.GetComponentsInChildren<SpriteRenderer>(true);
    }

    // Brings the dragged item to the front so it visually overlaps everything else.
    private void BoostSortingOrder(bool boost)
    {
        int offset = boost ? sortingOrderBoost : -sortingOrderBoost;
        foreach (var sr in allRenderers)
        {
            if (sr != null)
                sr.sortingOrder += offset;
        }
    }

    private bool IsPointerDown() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Began : Input.GetMouseButtonDown(0);
    private bool IsPointerHeld() => Input.touchCount > 0 ? (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary) : Input.GetMouseButton(0);
    private bool IsPointerUp() => Input.touchCount > 0 ? Input.GetTouch(0).phase == TouchPhase.Ended : Input.GetMouseButtonUp(0);
    private Vector2 GetPointerScreenPos() => Input.touchCount > 0 ? Input.GetTouch(0).position : (Vector2)Input.mousePosition;

    private bool IsPointerOverUI()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;
        if (Input.touchCount > 0)
            return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
    }

    // Core logic for picking up, moving, and dropping the object using mouse/touch.
    private void Update()
    {
        if (isConsumed) return;

        if (Time.timeScale <= 0.0001f)
        {
            if (isDragging)
            {
                isDragging = false;
                BoostSortingOrder(false);
                if (returnAfterDrop)
                {
                    rootToDrag.position = originalPosition;
                }
            }
            return;
        }

        Vector2 pointerWorldPos = mainCamera.ScreenToWorldPoint(GetPointerScreenPos());

        if (IsPointerDown())
        {
            if (IsPointerOverUI())
                return;

            if (myCollider.OverlapPoint(pointerWorldPos))
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
                dragOffset = rootToDrag.position - (Vector3)pointerWorldPos;

                BoostSortingOrder(true);
            }
        }
        else if (isDragging && IsPointerOverUI())
        {
            isDragging = false;
            BoostSortingOrder(false);
            if (returnAfterDrop)
            {
                rootToDrag.position = originalPosition;
            }
            return;
        }

        if (IsPointerUp() && isDragging)
        {
            isDragging = false;
            BoostSortingOrder(false);

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
                        dropZone.HandleDrop(this);
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
            Vector3 newPos = (Vector3)pointerWorldPos + dragOffset;
            newPos.z = zOffset;
            rootToDrag.position = newPos;
        }
    }

    // Triggered when the object lands in a valid drop zone.
    public void Consume(Transform dropZoneCenter)
    {
        isConsumed = true;
        isDragging = false;
        
        if (activeMoveCoroutine != null)
        {
            StopCoroutine(activeMoveCoroutine);
            activeMoveCoroutine = null;
        }

        if (isCookingIngredient && CookingVisualController.Instance != null && CookingVisualController.Instance.rawIngredientsContainer != null)
        {
            rootToDrag.SetParent(CookingVisualController.Instance.rawIngredientsContainer, true);
            
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

    // Animates the object snapping back to its original spot if dropped incorrectly.
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
            float easedT = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

            target.position = Vector3.LerpUnclamped(startPosition, targetPosition, easedT);
            yield return null;
        }
        target.position = targetPosition;
        isReturning = false;
        activeMoveCoroutine = null;
    }

    // Visually shrinks and absorbs the item into the target drop zone.
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
