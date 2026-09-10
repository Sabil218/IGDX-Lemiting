using System;
using System.Collections;
using UnityEngine;

public class PlatingCutscene : MonoBehaviour
{
    [Header("Scene Actors (Visuals)")]
    public Transform wajan;
    public Transform piring;

    [Header("Sprite Swaps")]
    public SpriteRenderer wajanRenderer;
    public Sprite wajanKosongSprite;
    
    public SpriteRenderer piringRenderer;
    public Sprite piringIsiSprite;

    [Header("Entry Animation")]
    public Vector3 wajanStartOffset = new Vector3(10f, 0f, 0f); // Dari kanan
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

    private Vector3 wajanDefaultPos;
    private Vector3 piringDefaultPos;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (wajan != null) wajanDefaultPos = wajan.position;
        if (piring != null) piringDefaultPos = piring.position;
    }

    public void SetupInitialPositions()
    {
        if (wajan != null) wajan.position = wajanDefaultPos + wajanStartOffset;
        if (piring != null) piring.position = piringDefaultPos + piringStartOffset;
    }

    public void PlayEntryAnimation(Action onComplete)
    {
        StartCoroutine(EntrySequence(onComplete));
    }

    private IEnumerator EntrySequence(Action onComplete)
    {
        Vector3 wStart = wajan != null ? wajan.position : Vector3.zero;
        Vector3 pStart = piring != null ? piring.position : Vector3.zero;

        // 1. Wajan masuk duluan
        if (wajan != null)
        {
            float elapsed = 0f;
            while (elapsed < entryDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / entryDuration);
                wajan.position = Vector3.Lerp(wStart, wajanDefaultPos, t);
                yield return null;
            }
            wajan.position = wajanDefaultPos;
        }

        // 2. Piring menyusul masuk
        if (piring != null)
        {
            float elapsed = 0f;
            while (elapsed < entryDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / entryDuration);
                piring.position = Vector3.Lerp(pStart, piringDefaultPos, t);
                yield return null;
            }
            piring.position = piringDefaultPos;
        }

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
        // StartCoroutine(ScreenShake()); // Dinonaktifkan (terlalu bergetar)

        // 2. Sprite Swaps
        if (wajanRenderer != null && wajanKosongSprite != null)
            wajanRenderer.sprite = wajanKosongSprite;

        if (piringRenderer != null && piringIsiSprite != null)
            piringRenderer.sprite = piringIsiSprite;

        yield return new WaitForSeconds(postDropDelay);

        // 3. Play Exit Animation
        yield return StartCoroutine(ExitSequence());

        onComplete?.Invoke();
    }

    private IEnumerator ScreenShake()
    {
        if (mainCamera == null) yield break;

        Vector3 originalCamPos = mainCamera.transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * shakeIntensity;
            float y = UnityEngine.Random.Range(-1f, 1f) * shakeIntensity;

            mainCamera.transform.position = originalCamPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.position = originalCamPos;
    }

    private IEnumerator ExitSequence()
    {
        float elapsed = 0f;

        // Wajan keluar frame
        if (wajan != null)
        {
            Vector3 wStart = wajan.position;
            Vector3 wExit = wajan.position + wajanExitOffset;

            while (elapsed < exitDuration)
            {
                elapsed += Time.deltaTime;
                wajan.position = Vector3.Lerp(wStart, wExit, Mathf.SmoothStep(0, 1, elapsed / exitDuration));
                yield return null;
            }
            wajan.gameObject.SetActive(false);
        }

        // Piring Zoom & ke Tengah
        elapsed = 0f;
        if (piring != null)
        {
            Vector3 pStartPos = piring.position;
            Vector3 pStartScale = piring.localScale;
            Vector3 pTargetPos = Vector3.zero; // Tengah layar

            while (elapsed < zoomDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / zoomDuration);

                piring.position = Vector3.Lerp(pStartPos, pTargetPos, t);
                piring.localScale = Vector3.Lerp(pStartScale, plateTargetScale, t);

                yield return null;
            }

            piring.position = pTargetPos;
            piring.localScale = plateTargetScale;
        }
    }
}
