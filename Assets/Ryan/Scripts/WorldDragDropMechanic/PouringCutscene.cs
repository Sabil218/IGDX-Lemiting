using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PouringCutscene : MonoBehaviour
{
    [Header("Cutscene Actors")]
    public Transform wajan;
    public Transform piring;
    public SpriteRenderer wajanRenderer;
    public Sprite wajanKosongSprite;

    [Header("Events")]
    public UnityEvent OnCutsceneFinished;
    [Header("Animation Settings")]
    [Tooltip("Sudut wajan saat menuang (misal 45 derajat)")]
    public float tiltAngle = 45f;
    [Tooltip("Durasi animasi wajan miring")]
    public float tiltDuration = 0.3f;
    
    [Tooltip("Jeda setelah wajan miring & gambar diganti")]
    public float postPourDelay = 0.5f;

    [Tooltip("Seberapa jauh wajan bergeser keluar frame")]
    public Vector3 panExitOffset = new Vector3(10f, 0f, 0f);
    public float exitDuration = 0.5f;

    [Header("Plate Zoom Settings")]
    [Tooltip("Skala piring setelah di-zoom")]
    public Vector3 plateTargetScale = new Vector3(1.5f, 1.5f, 1f);
    public float zoomDuration = 0.6f;

    //Trigger pouring cutscene
    public void StartCutscene()
    {
        if (wajan == null || piring == null)
        {
            Debug.LogWarning("[PouringCutscene] Aktor wajan/piring belum di-assign!");
            OnCutsceneFinished?.Invoke();
            return;
        }
        StartCoroutine(PouringSequence());
    }

    //Execute pouring animation sequence
    private IEnumerator PouringSequence()
    {
        // Tilt Wajan
        Quaternion startRot = wajan.rotation;
        Quaternion targetRot = wajan.rotation * Quaternion.Euler(0, 0, tiltAngle);
        float elapsed = 0f;

        while (elapsed < tiltDuration)
        {
            elapsed += Time.deltaTime;
            wajan.rotation = Quaternion.Lerp(startRot, targetRot, elapsed / tiltDuration);
            yield return null;
        }
        wajan.rotation = targetRot;

        //Ganti Gambar (Jadi Kosong) & Mungkin munculin partikel makanan jatuh
        if (wajanRenderer != null && wajanKosongSprite != null)
        {
            wajanRenderer.sprite = wajanKosongSprite;
        }
        
        yield return new WaitForSeconds(postPourDelay);

        //Wajan keluar layar (Exit)
        Vector3 wajanStartPos = wajan.position;
        Vector3 wajanExitPos = wajan.position + panExitOffset;
        elapsed = 0f;

        while (elapsed < exitDuration)
        {
            elapsed += Time.deltaTime;
            wajan.position = Vector3.Lerp(wajanStartPos, wajanExitPos, elapsed / exitDuration);
            yield return null;
        }
        wajan.position = wajanExitPos;
        wajan.gameObject.SetActive(false); // Nonaktifkan wajan

        //Piring ke tengah layar (0,0) & Zoom In
        Vector3 piringStartPos = piring.position;
        Vector3 piringTargetPos = Vector3.zero; // Tengah layar
        
        Vector3 piringStartScale = piring.localScale;
        elapsed = 0f;

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;
            float easedT = t * t * (3f - 2f * t); // SmoothStep

            piring.position = Vector3.Lerp(piringStartPos, piringTargetPos, easedT);
            piring.localScale = Vector3.Lerp(piringStartScale, plateTargetScale, easedT);
            
            yield return null;
        }
        
        piring.position = piringTargetPos;
        piring.localScale = plateTargetScale;

        OnCutsceneFinished?.Invoke();
    }
}
