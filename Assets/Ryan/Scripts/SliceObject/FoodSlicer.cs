using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FoodSlicer : MonoBehaviour
{
    [System.Serializable]
    public class SlicePhase
    {
        public Image sliceLine;
        public GameObject sourceObject;
        public GameObject[] resultObjects;
        public Transform ejectedFragment;
        public Vector2 knockbackDirection;

        [Header("Juiceness Effect (Rotating and Movement of Sliced Objects)")]
        public float tiltAngle = 15f;

        [Tooltip("Optional to move non-ejected fragments")]
        public bool moveNonEjected = false;
        public Vector2 secondaryKnockbackDirection = new Vector2(-0.2f, -0.1f);
        public float secondaryTiltAngle = -3f;

        [Header("Sprite Swapping")]
        [Tooltip("Change cut sprite")]
        public Sprite cutSprite;
    }

    [Header("Input Hook")]
    [SerializeField] private SwipeInput inputController;

    [Header("Slicing Mechanics")]
    [SerializeField] private SlicePhase[] slicePhases;

    [Header("Slice Validation Tolerances")]
    [Tooltip("Toleransi jarak menyamping kursor dengan garis potong.")]
    [SerializeField] private float perpDistanceTolerance = 0.3f;

    [Header("Game Feel (Juiciness)")]
    [SerializeField] private float knockbackDistance;
    [SerializeField] private float knockbackDuration;

    private int currentPhaseIndex = 0;
    private float currentSliceProgress = 1f;
    
    private List<GameObject> spawnedFragments = new List<GameObject>();
    private Dictionary<SpriteRenderer, Sprite> originalSprites = new Dictionary<SpriteRenderer, Sprite>();

    //Initialize original sprites
    private void Awake()
    {
        // Simpan sprite asli dari setiap sourceObject agar bisa di-reset nanti
        foreach (var phase in slicePhases)
        {
            if (phase.sourceObject != null)
            {
                SpriteRenderer sr = phase.sourceObject.GetComponent<SpriteRenderer>();
                if (sr != null && !originalSprites.ContainsKey(sr))
                {
                    originalSprites[sr] = sr.sprite;
                }
            }
        }
    }

    //Subscribe to swipe input
    private void OnEnable()
    {
        if (inputController != null)
        {
            inputController.OnSwipeCompleted += HandleSwipeCompleted;
            inputController.OnSwipeMoving += HandleSwipeMoving;
        }
        ResetSlicerState();
    }

    //Reset slice progress and fragments
    private void ResetSlicerState()
    {
        currentPhaseIndex = 0;
        currentSliceProgress = 1f;

        // Clean sliced fragment from prev ingredient
        foreach (var frag in spawnedFragments)
        {
            if (frag != null) Destroy(frag);
        }
        spawnedFragments.Clear();

        for (int i = 0; i < slicePhases.Length; i++)
        {
            if (slicePhases[i].sliceLine != null)
            {
                slicePhases[i].sliceLine.gameObject.SetActive(i == 0);
                slicePhases[i].sliceLine.fillAmount = 1f;
            }

            foreach (GameObject obj in slicePhases[i].resultObjects)
            {
                // Validating object type
                if (obj != null && obj.scene.IsValid()) obj.SetActive(false);
            }
        }

        if (slicePhases.Length > 0 && slicePhases[0].sourceObject != null)
        {
            GameObject src = slicePhases[0].sourceObject;
            
            // Reset sprite for next ingredient
            SpriteRenderer sr = src.GetComponent<SpriteRenderer>();
            if (sr != null && originalSprites.TryGetValue(sr, out Sprite origSprite))
            {
                sr.sprite = origSprite;
            }

            if (src == this.gameObject)
            {
                if (sr != null) sr.enabled = true;

                Collider2D col2d = src.GetComponent<Collider2D>();
                if (col2d != null) col2d.enabled = true;

                Collider col = src.GetComponent<Collider>();
                if (col != null) col.enabled = true;
            }
            else
            {
                src.SetActive(true);
            }
        }
    }

    //Unsubscribe from swipe input
    private void OnDisable()
    {
        if (inputController != null)
        {
            inputController.OnSwipeCompleted -= HandleSwipeCompleted;
            inputController.OnSwipeMoving -= HandleSwipeMoving;
        }
    }


    //Force skip slicing process
    public void ForceSlicedState()
    {
        this.enabled = false;

        for (int i = 0; i < slicePhases.Length; i++)
        {
            if (slicePhases[i].sliceLine != null)
                slicePhases[i].sliceLine.gameObject.SetActive(false);

            Transform sourceT = slicePhases[i].sourceObject != null ? slicePhases[i].sourceObject.transform : transform;

            foreach (GameObject obj in slicePhases[i].resultObjects)
            {
                if (obj != null)
                {
                    if (!obj.scene.IsValid()) // Jika obj adalah Prefab
                    {
                        GameObject spawned = Instantiate(obj, sourceT.position, sourceT.rotation, transform);
                        spawned.SetActive(true);
                        spawnedFragments.Add(spawned);
                    }
                    else
                    {
                        obj.SetActive(true);
                    }
                }
            }
        }

        for (int i = 0; i < slicePhases.Length; i++)
        {
            if (slicePhases[i].sourceObject != null)
            {
                if (slicePhases[i].cutSprite != null)
                {
                    SpriteRenderer sr = slicePhases[i].sourceObject.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.sprite = slicePhases[i].cutSprite;
                }
                else
                {
                    if (slicePhases[i].sourceObject == this.gameObject)
                    {
                        SpriteRenderer sr = GetComponent<SpriteRenderer>();
                        if (sr != null) sr.enabled = false;
                    }
                    else
                    {
                        slicePhases[i].sourceObject.SetActive(false);
                    }
                }
            }
        }
    }

    //Update slice line fill visually
    private void HandleSwipeMoving(Vector2 currentPos)
    {
        if (currentPhaseIndex >= slicePhases.Length) return;

        SlicePhase currentPhase = slicePhases[currentPhaseIndex];
        if (currentPhase.sliceLine == null) return;

        if (inputController.PathPoints.Count > 1) // Cek arah tarikan
        {
            Vector2 startPos = inputController.PathPoints[0];

            Vector2 linePos = currentPhase.sliceLine.transform.position;
            Vector2 lineUp = currentPhase.sliceLine.transform.up;
            Vector2 lineRight = currentPhase.sliceLine.transform.right;

            // 1. Cek akurasi menyamping
            float currentPerpDistance = Mathf.Abs(Vector2.Dot(currentPos - linePos, lineRight));
            float startPerpDistance = Mathf.Abs(Vector2.Dot(startPos - linePos, lineRight));

            if (startPerpDistance > perpDistanceTolerance * 1.5f || currentPerpDistance > perpDistanceTolerance * 1.5f)
            {
                return;
            }

            // Hitung tinggi garis (World Space)
            RectTransform rectT = currentPhase.sliceLine.rectTransform;
            float worldHeight = rectT.rect.height * rectT.lossyScale.y;

            // Proyeksi jari ke vertikal garis
            Vector2 midOffset = currentPos - linePos;
            float alongDistance = Vector2.Dot(midOffset, lineUp); // Posisi jari relatif thd tengah

            // Deteksi arah usapan untuk ubah Fill Origin otomatis
            Vector2 swipeDir = (currentPos - startPos).normalized;
            bool swipingUp = Vector2.Dot(swipeDir, lineUp) > 0;
            
            float newFillAmount = 1f;

            if (swipingUp)
            {
                currentPhase.sliceLine.fillOrigin = (int)UnityEngine.UI.Image.OriginVertical.Top;
                float sisaGaris = (worldHeight / 2f) - alongDistance;
                newFillAmount = Mathf.Clamp01(sisaGaris / worldHeight);
            }
            else // swiping down
            {
                currentPhase.sliceLine.fillOrigin = (int)UnityEngine.UI.Image.OriginVertical.Bottom;
                float sisaGaris = alongDistance + (worldHeight / 2f);
                newFillAmount = Mathf.Clamp01(sisaGaris / worldHeight);
            }

            // Simpan progress terkecil (agar garis tidak pernah mundur)
            currentSliceProgress = Mathf.Min(currentSliceProgress, newFillAmount);
            currentPhase.sliceLine.fillAmount = currentSliceProgress;

            // Pemicu eksekusi saat progres selesai dicicil
            if (currentSliceProgress <= 0.05f)
            {
                ExecuteSlice(currentPhase);
            }
        }
    }

    //Check if slice is completed
    private void HandleSwipeCompleted(List<Vector2> swipePath)
    {
        if (currentPhaseIndex >= slicePhases.Length) return;

        SlicePhase currentPhase = slicePhases[currentPhaseIndex];

        if (currentSliceProgress <= 0.05f)
        {
            ExecuteSlice(currentPhase);
        }
    }

    //Spawn slice fragments and apply knockback
    private void ExecuteSlice(SlicePhase phase)
    {
        if (phase.sliceLine != null)
        {
            phase.sliceLine.gameObject.SetActive(false);
        }

        if (phase.sourceObject != null)
        {
            if (phase.cutSprite != null)
            {
                // Change Sprite
                SpriteRenderer sr = phase.sourceObject.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = phase.cutSprite;
                }
            }
            else
            {
                if (phase.sourceObject == this.gameObject)
                {
                    SpriteRenderer sr = phase.sourceObject.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.enabled = false;

                    Collider2D col2d = phase.sourceObject.GetComponent<Collider2D>();
                    if (col2d != null) col2d.enabled = false;

                    Collider col = phase.sourceObject.GetComponent<Collider>();
                    if (col != null) col.enabled = false;
                }
                else
                {
                    phase.sourceObject.SetActive(false);
                }
            }
        }

        Transform sourceTransform = phase.sourceObject != null ? phase.sourceObject.transform : transform;
        Vector3 spawnPos = sourceTransform.position;
        Quaternion spawnRot = sourceTransform.rotation;

        foreach (GameObject obj in phase.resultObjects)
        {
            if (obj != null) 
            {
                GameObject activeObj = null;

                if (!obj.scene.IsValid()) // Jika obj adalah Prefab
                {
                    activeObj = Instantiate(obj, spawnPos, spawnRot, transform);
                    activeObj.SetActive(true);
                    spawnedFragments.Add(activeObj);
                }
                else // Jika obj sudah ada di Scene
                {
                    activeObj = obj;
                    activeObj.SetActive(true);
                }

                // Cek apakah objek ini adalah pecahan yang harus terlempar (knockback utama)
                if (phase.ejectedFragment != null && obj.transform == phase.ejectedFragment)
                {
                    Vector3 knockbackDir = new Vector3(phase.knockbackDirection.x, phase.knockbackDirection.y, 0).normalized;
                    StartCoroutine(ProcessKnockback(activeObj.transform, knockbackDir * knockbackDistance, knockbackDuration, phase.tiltAngle));
                }
                else if (phase.moveNonEjected)
                {
                    // Secondary jiggle movement untuk objek sisa
                    Vector3 secKnockbackDir = new Vector3(phase.secondaryKnockbackDirection.x, phase.secondaryKnockbackDirection.y, 0);
                    StartCoroutine(ProcessKnockback(activeObj.transform, secKnockbackDir, knockbackDuration * 0.8f, phase.secondaryTiltAngle));
                }
            }
        }

        currentPhaseIndex++;
        currentSliceProgress = 1f; // Reset progress

        if (currentPhaseIndex < slicePhases.Length)
        {
            if (slicePhases[currentPhaseIndex].sliceLine != null)
            {
                slicePhases[currentPhaseIndex].sliceLine.gameObject.SetActive(true);
                slicePhases[currentPhaseIndex].sliceLine.fillAmount = 1f;
            }
        }

        if (currentPhaseIndex >= slicePhases.Length)
        {
            StartCoroutine(DelayNextGameState());
        }
    }

    //Animate knockback fragment
    private IEnumerator ProcessKnockback(Transform target, Vector3 knockbackVector, float duration, float tiltAngle)
    {
        Vector3 startPos = target.localPosition;
        Vector3 endPos = target.localPosition + knockbackVector;

        Quaternion startRot = target.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 0, tiltAngle);

        float elapsedTime = 0f;
        Vector3 startScale = target.localScale;
        target.localScale = startScale * 1.15f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = 1f - (1f - t) * (1f - t);

            target.localPosition = Vector3.Lerp(startPos, endPos, t);
            target.localRotation = Quaternion.Lerp(startRot, endRot, t);
            target.localScale = Vector3.Lerp(target.localScale, startScale, t);
            yield return null;
        }

        target.localPosition = endPos;
        target.localRotation = endRot;
        target.localScale = startScale;
    }

    //Delay before notifying manager
    private IEnumerator DelayNextGameState()
    {
        yield return new WaitForSeconds(1f);

        if (SlicingManager.instance != null)
        {
            SlicingManager.instance.BahanSelesaiDipotong();
        }
    }
}