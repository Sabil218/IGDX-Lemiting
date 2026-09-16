using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientDropCutscene : MonoBehaviour
{
    [Header("Drop Animation")]
    [Tooltip("Waktu jatuh tiap potongan (detik).")]
    [SerializeField] private float dropDuration = 0.4f;
    [Tooltip("Titik awal spesifik bahan muncul sebelum jatuh (opsional). Jika diisi, akan mengabaikan spawnOffsetY.")]
    [SerializeField] private Transform customSpawnPoint;
    [Tooltip("Jarak vertikal di atas wajan tempat bahan mulai jatuh.")]
    [SerializeField] private float spawnOffsetY = 6f;
    [Tooltip("Waktu jeda (detik) sebelum bahan mulai jatuh. Berguna agar kamera bisa bergerak (panning) duluan.")]
    [SerializeField] private float delayBeforeDrop = 0.3f;
    [Tooltip("Jeda acak antar jatuhnya potongan.")]
    [SerializeField] private float maxStaggerDelay = 0.08f;

    [Header("Scale & Spread Animation")]
    [Tooltip("Target skala objek saat masuk wajan.")]
    [SerializeField] private float targetScale = 0.4f;
    [Tooltip("Seberapa jauh potongan terlempar ke samping (kiri-kanan).")]
    [SerializeField] private float spreadRadiusX = 2.5f;
    [Tooltip("Seberapa jauh potongan terlempar ke atas (menjauhi kamera).")]
    [SerializeField] private float spreadRadiusYUp = 0.8f;
    [Tooltip("Seberapa jauh potongan terlempar ke bawah (mendekati kamera). Jika tumpah ke depan wajan, kecilkan nilai ini!")]
    [SerializeField] private float spreadRadiusYDown = 0.4f;
    [Tooltip("Maksimal rotasi acak Z saat jatuh.")]
    [SerializeField] private float maxRandomRotationZ = 120f;

    [Header("Clustering (Controllable RNG)")]
    [Range(0f, 1f)]
    [Tooltip("0 = Pure Random. 1 = Jatuh di tempat yang persis sama dengan potongan sebelumnya.")]
    [SerializeField] private float cohesionWeight = 0.6f;
    [Tooltip("Jika true, bahan baru akan mencoba jatuh di dekat lokasi bahan sebelumnya.")]
    [SerializeField] private bool persistClusterAcrossIngredients = true;

    // State memory for clustering
    private Vector2 lastSpreadOffset = Vector2.zero;
    private bool hasPreviousDrop = false;

    [Header("Visual Effects")]
    [Tooltip("Memutar efek cipratan saat objek mendarat (Opsional).")]
    [SerializeField] private ParticleSystem splashEffect;

    [Header("Post-Impact")]
    [Tooltip("Waktu tunggu sesudah jatuh sebelum memanggil onComplete (detik).")]
    [SerializeField] private float postImpactDelay = 0.7f;

    [Header("Debug")]
    [Tooltip("Isi dengan objek DropArea untuk memunculkan garis panduan sebaran (elips hijau) di Editor.")]
    [SerializeField] private Transform debugDropArea;

    // Backward compatibility for scripts that don't pass disableRotation and dropInCenter
    public void Play(Transform ingredient, Transform targetArea, Transform panContentContainer, Action onComplete)
    {
        Play(ingredient, targetArea, panContentContainer, false, false, onComplete);
    }

    public void Play(Transform ingredient, Transform targetArea, Transform panContentContainer, bool disableRotation, bool dropInCenter, Action onComplete)
    {
        if (ingredient == null)
        {
            Debug.LogWarning("[IngredientDropCutscene] Ingredient is null");
            onComplete?.Invoke();
            return;
        }

        if (targetArea == null)
        {
            Debug.LogWarning("[IngredientDropCutscene] Target Area is null!");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(DropSequence(ingredient, targetArea, panContentContainer, disableRotation, dropInCenter, onComplete));
    }

    private IEnumerator DropSequence(Transform ingredient, Transform targetArea, Transform panContentContainer, bool disableRotation, bool dropInCenter, Action onComplete)
    {
        // Memberi waktu agar Cinemachine mulai bergerak (panning) duluan
        if (delayBeforeDrop > 0f)
        {
            yield return new WaitForSeconds(delayBeforeDrop);
        }

        Vector3 panCenter = targetArea.position;

        if (panContentContainer != null)
        {
            ingredient.SetParent(panContentContainer, true);
        }
        else
        {
            ingredient.SetParent(this.transform, true);
        }
        ingredient.localScale = ingredient.localScale * targetScale;

        Vector3 parentStartPos = panCenter + Vector3.up * spawnOffsetY;

        if (customSpawnPoint != null)
        {
            parentStartPos = customSpawnPoint.position;
        }

        ingredient.position = parentStartPos;
        ingredient.gameObject.SetActive(true);

        SpriteRenderer[] renderers = ingredient.GetComponentsInChildren<SpriteRenderer>();
        List<Transform> pieces = new List<Transform>();

        foreach (var sr in renderers)
        {
            if (sr.transform != ingredient)
            {
                pieces.Add(sr.transform);
                sr.gameObject.SetActive(false);
            }
        }

        if (pieces.Count == 0)
        {
            SpriteRenderer rootSr = ingredient.GetComponent<SpriteRenderer>();
            if (rootSr != null)
            {
                pieces.Add(ingredient);
                ingredient.gameObject.SetActive(false);
            }
        }

        int totalPieces = pieces.Count;
        int completedPieces = 0;

        // Jika tidak persisten antar bahan, reset memory setiap kali Play() dipanggil
        if (!persistClusterAcrossIngredients)
        {
            ResetClusterMemory();
        }

        for (int i = 0; i < totalPieces; i++)
        {
            Vector2 rawSpread = dropInCenter ? Vector2.zero : UnityEngine.Random.insideUnitCircle;
            Vector2 finalSpread;

            if (hasPreviousDrop)
            {
                finalSpread = Vector2.Lerp(rawSpread, lastSpreadOffset, cohesionWeight);
            }
            else
            {
                finalSpread = rawSpread;
            }

            lastSpreadOffset = finalSpread;
            hasPreviousDrop = true;

            float actualRadiusY = finalSpread.y > 0 ? spreadRadiusYUp : spreadRadiusYDown;
            Vector3 targetGlobalPos = panCenter + new Vector3(finalSpread.x * spreadRadiusX, finalSpread.y * actualRadiusY, 0f);

            float randomZ = disableRotation ? 0f : UnityEngine.Random.Range(-maxRandomRotationZ, maxRandomRotationZ);
            Quaternion targetRot = pieces[i].localRotation * Quaternion.Euler(0f, 0f, randomZ);

            StartCoroutine(AnimateSinglePiece(pieces[i], panCenter, targetGlobalPos, targetRot, splashEffect, () => completedPieces++));

            yield return new WaitForSeconds(UnityEngine.Random.Range(0.05f, maxStaggerDelay));
        }

        while (completedPieces < totalPieces)
        {
            yield return null;
        }

        yield return new WaitForSeconds(postImpactDelay);
        onComplete?.Invoke();
    }

    /// <summary>
    /// Panggil ini saat stage memasak baru dimulai agar bahan tidak menumpuk di area stage sebelumnya.
    /// </summary>
    public void ResetClusterMemory()
    {
        hasPreviousDrop = false;
        lastSpreadOffset = Vector2.zero;
    }

    private IEnumerator AnimateSinglePiece(Transform piece, Vector3 endCenterPos, Vector3 endSpreadPos, Quaternion endRot, ParticleSystem splash, Action onComplete)
    {
        piece.gameObject.SetActive(true);

        Vector3 startPos = piece.position;
        Quaternion startRot = piece.rotation;
        float elapsed = 0f;

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dropDuration);

            // Vertikal (Y): Jatuh dengan gravitasi (Ease-In)
            float tFall = t * t;
            float currentY = Mathf.Lerp(startPos.y, endSpreadPos.y, tFall);

            // Horizontal (X, Z): Menyebar dengan Ease-Out agar terlihat terlempar lalu melambat
            float tSpread = 1f - (1f - t) * (1f - t);

            float currentX = Mathf.Lerp(startPos.x, endSpreadPos.x, tSpread);
            float currentZ = Mathf.Lerp(startPos.z, endSpreadPos.z, tSpread);

            piece.position = new Vector3(currentX, currentY, currentZ);
            
            // Rotasi berputar secara halus bersamaan dengan spread
            piece.rotation = Quaternion.Lerp(startRot, endRot, tSpread);

            yield return null;
        }

        piece.position = endSpreadPos;
        piece.rotation = endRot;

        if (splash != null)
        {
            splash.transform.position = endSpreadPos;
            splash.Play();
        }

        onComplete?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        if (debugDropArea == null) return;

        Gizmos.color = Color.green;
        Vector3 center = debugDropArea.position;
        int segments = 36;
        float angle = 0f;
        
        float startRadiusY = Mathf.Sin(0) > 0 ? spreadRadiusYUp : spreadRadiusYDown;
        Vector3 lastPoint = center + new Vector3(Mathf.Cos(0) * spreadRadiusX, Mathf.Sin(0) * startRadiusY, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            angle += (360f / segments) * Mathf.Deg2Rad;
            float currentRadiusY = Mathf.Sin(angle) > 0 ? spreadRadiusYUp : spreadRadiusYDown;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * spreadRadiusX, Mathf.Sin(angle) * currentRadiusY, 0);
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }
}