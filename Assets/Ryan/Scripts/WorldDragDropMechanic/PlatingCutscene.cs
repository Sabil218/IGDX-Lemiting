using System;
using System.Collections;
using UnityEngine;

public class PlatingCutscene : MonoBehaviour
{
    [Header("Scene Actors (Visuals)")]
    public Transform wajan;
    public Transform piring;

    [Header("Sprite Swaps")]
    [Tooltip("Target wajan yang sprite-nya akan diganti")]
    public SpriteRenderer wajanRenderer;
    [Tooltip("Gambar wajan kosong/kotor setelah makanan dituang")]
    public Sprite wajanKosongSprite;
    
    [Tooltip("Object-object makanan (seperti Matang/SetengahMatang) di dalam wajan yang harus disembunyikan saat dituang")]
    public GameObject[] objectsToHideOnDrop;

    [Tooltip("Target piring yang sprite-nya akan diganti")]
    public SpriteRenderer piringRenderer;
    [Tooltip("Gambar piring yang sudah ada makanannya")]
    public Sprite piringIsiSprite;

    [Tooltip("Object-object yang harus dimunculkan saat dituang (misalnya ServedFood)")]
    public GameObject[] objectsToShowOnDrop;

    [Header("Entry Animation")]
    public Vector3 wajanStartOffset = new Vector3(-10f, 0f, 0f); // Dari kiri
    public Vector3 piringStartOffset = new Vector3(0f, 8f, 0f); // Dari atas
    public float entryDuration = 0.8f;

    [Header("Post-Drop Effects")]
    public ParticleSystem dropEffect;
    public float shakeIntensity = 0.2f;
    public float shakeDuration = 0.3f;
    public float postDropDelay = 0.5f;

    [Header("Exit Animation")]
    public Vector3 wajanExitOffset = new Vector3(10f, 0f, 0f);
    public float exitDuration = 0.5f;
    public Vector3 plateTargetScale = new Vector3(1.5f, 1.5f, 1f);
    public float zoomDuration = 0.6f;

    public Transform wajanMeetPoint;
    public Transform piringMeetPoint;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void PlayEntryAnimation(Action onComplete)
    {
        StartCoroutine(EntrySequence(onComplete));
    }

    private IEnumerator EntrySequence(Action onComplete)
    {
        if (wajan == null || piring == null || wajanMeetPoint == null || piringMeetPoint == null)
        {
            Debug.LogWarning("Referensi Wajan, Piring, atau Meet Point ada yang kosong di PlatingCutscene!");
            yield break;
        }

        // 1. Set posisi awal (di luar layar / offset berdasarkan titik kumpul)
        Vector3 wTarget = wajanMeetPoint.position;
        Vector3 pTarget = piringMeetPoint.position;

        Vector3 wStart = wTarget + wajanStartOffset;
        Vector3 pStart = pTarget + piringStartOffset;

        wajan.position = wStart;
        piring.position = pStart;

        // 2. Animasi meluncur dari luar layar ke titik kumpul
        float elapsed = 0f;
        while (elapsed < entryDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / entryDuration);

            wajan.position = Vector3.Lerp(wStart, wTarget, t);
            piring.position = Vector3.Lerp(pStart, pTarget, t);

            yield return null;
        }

        wajan.position = wTarget;
        piring.position = pTarget;

        onComplete?.Invoke();
    }

    public void PlayPostDropAndExitAnimation(Action onComplete)
    {
        StartCoroutine(PostDropSequence(onComplete));
    }

    private IEnumerator PostDropSequence(Action onComplete)
    {
        // 1. Play Effects
        if (dropEffect != null) dropEffect.Play();

        // 2. Sprite Swaps
        if (wajanRenderer != null && wajanKosongSprite != null)
            wajanRenderer.sprite = wajanKosongSprite;

        if (piringRenderer != null && piringIsiSprite != null)
            piringRenderer.sprite = piringIsiSprite;
            
        if (objectsToHideOnDrop != null)
        {
            foreach(var obj in objectsToHideOnDrop)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        if (objectsToShowOnDrop != null)
        {
            foreach(var obj in objectsToShowOnDrop)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        yield return new WaitForSeconds(postDropDelay);

        // 3. Play Exit Animation
        yield return StartCoroutine(ExitSequence());

        onComplete?.Invoke();
    }

    private IEnumerator ExitSequence()
    {
        float elapsed = 0f;
        Vector3 wajanExitStart = wajan != null ? wajan.position : Vector3.zero;
        Vector3 wajanExitTarget = wajanExitStart + wajanExitOffset;

        Vector3 piringStartScale = piring != null ? piring.localScale : Vector3.one;

        while (elapsed < exitDuration || elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;

            if (elapsed < exitDuration && wajan != null)
            {
                float tWajan = Mathf.SmoothStep(0, 1, elapsed / exitDuration);
                wajan.position = Vector3.Lerp(wajanExitStart, wajanExitTarget, tWajan);
            }

            if (elapsed < zoomDuration && piring != null)
            {
                float tZoom = Mathf.SmoothStep(0, 1, elapsed / zoomDuration);
                piring.localScale = Vector3.Lerp(piringStartScale, plateTargetScale, tZoom);
            }

            yield return null;
        }

        if (wajan != null) wajan.position = wajanExitTarget;
        if (piring != null) piring.localScale = plateTargetScale;
    }
}
