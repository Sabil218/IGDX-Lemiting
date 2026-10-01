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
    public GameObject[] objectsToHideOnDrop;
    public SpriteRenderer piringRenderer;
    public Sprite piringIsiSprite;
    public GameObject[] objectsToShowOnDrop;

    [Header("Entry Animation")]
    public Vector3 wajanStartOffset = new Vector3(-10f, 0f, 0f);
    public Vector3 piringStartOffset = new Vector3(0f, 8f, 0f);
    public float entryDuration = 0.8f;

    [Header("Post-Drop Effects")]
    public ParticleSystem dropEffect;
    public float shakeIntensity = 0.2f;
    public float shakeDuration = 0.3f;
    public float postDropDelay = 0.5f;

    [Header("Exit Animation")]
    public Vector3 wajanExitOffset = new Vector3(10f, 0f, 0f);
    public float exitDuration = 1.5f;
    public Vector3 plateTargetScale = new Vector3(2f, 2f, 1f);
    public Transform plateCenterPoint;
    
    public float zoomDuration = 1.0f;

    [Header("Final Presentation")]
    public GameObject[] finalUIObjects;
    public Transform finalAlasObject;
    public Transform finalAlasTargetPos;

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

    // Slides the pan and plate into view from off-screen.
    private IEnumerator EntrySequence(Action onComplete)
    {
        if (wajan == null || piring == null || wajanMeetPoint == null || piringMeetPoint == null)
        {
            Debug.LogWarning("Referensi Wajan, Piring, atau Meet Point ada yang kosong di PlatingCutscene!");
            yield break;
        }

        Vector3 wTarget = wajanMeetPoint.position;
        Vector3 pTarget = piringMeetPoint.position;

        Vector3 wStart = wTarget + wajanStartOffset;
        Vector3 pStart = pTarget + piringStartOffset;

        wajan.position = wStart;
        piring.position = pStart;

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

    // Plays the food falling effect and updates the sprites (empty pan, full plate).
    private IEnumerator PostDropSequence(Action onComplete)
    {
        if (dropEffect != null) dropEffect.Play();

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

        yield return StartCoroutine(ExitSequence());

        if (finalUIObjects != null)
        {
            foreach(var obj in finalUIObjects)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        onComplete?.Invoke();
    }

    // Animates the pan leaving the screen and the finished plate zooming in for presentation.
    private IEnumerator ExitSequence()
    {
        float elapsed = 0f;
        Vector3 wajanExitStart = wajan != null ? wajan.position : Vector3.zero;
        Vector3 wajanExitTarget = wajanExitStart + wajanExitOffset;

        Vector3 piringStartScale = piring != null ? piring.localScale : Vector3.one;
        Vector3 piringStartPos = piring != null ? piring.position : Vector3.zero;
        
        Vector3 piringTargetPos = plateCenterPoint != null ? plateCenterPoint.position : piringStartPos;

        Vector3 alasStartPos = finalAlasObject != null ? finalAlasObject.position : Vector3.zero;
        Vector3 alasTargetPos = finalAlasTargetPos != null ? finalAlasTargetPos.position : alasStartPos;
        
        if (finalAlasObject != null)
        {
            finalAlasObject.gameObject.SetActive(true);
        }

        while (elapsed < exitDuration || elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;

            if (elapsed < exitDuration && wajan != null)
            {
                float tWajan = Mathf.SmoothStep(0, 1, elapsed / exitDuration);
                wajan.position = Vector3.Lerp(wajanExitStart, wajanExitTarget, tWajan);
            }

            if (elapsed < zoomDuration)
            {
                float tZoom = Mathf.SmoothStep(0, 1, elapsed / zoomDuration);
                
                if (piring != null)
                {
                    piring.localScale = Vector3.Lerp(piringStartScale, plateTargetScale, tZoom);
                    piring.position = Vector3.Lerp(piringStartPos, piringTargetPos, tZoom);
                }

                if (finalAlasObject != null)
                {
                    finalAlasObject.position = Vector3.Lerp(alasStartPos, alasTargetPos, tZoom);
                }
            }

            yield return null;
        }

        if (wajan != null) wajan.position = wajanExitTarget;
        if (piring != null) 
        {
            piring.localScale = plateTargetScale;
            piring.position = piringTargetPos;
        }
        if (finalAlasObject != null)
        {
            finalAlasObject.position = alasTargetPos;
        }
    }
}
