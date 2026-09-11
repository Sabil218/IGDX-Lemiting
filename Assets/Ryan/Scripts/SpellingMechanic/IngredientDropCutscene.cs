using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientDropCutscene : MonoBehaviour
{
    [Header("Drop Animation")]
    [Tooltip("Waktu jatuh tiap potongan (detik).")]
    [SerializeField] private float dropDuration = 0.4f;
    [Tooltip("Jarak vertikal di atas wajan tempat bahan mulai jatuh.")]
    [SerializeField] private float spawnOffsetY = 6f;
    [Tooltip("Jeda acak antar jatuhnya potongan.")]
    [SerializeField] private float maxStaggerDelay = 0.08f;

    [Header("Scale & Spread Animation")]
    [Tooltip("Target skala objek saat masuk wajan.")]
    [SerializeField] private float targetScale = 0.4f;
    [Tooltip("Seberapa jauh potongan terlempar menyebar dari titik tengah.")]
    [SerializeField] private float spreadRadius = 0.8f;
    [Tooltip("Maksimal rotasi acak saat potongan jatuh.")]
    [SerializeField] private float maxRotation = 120f;

    [Header("Visual Effects")]
    [Tooltip("Memutar efek cipratan saat objek mendarat (Opsional).")]
    [SerializeField] private ParticleSystem splashEffect;

    [Header("Post-Impact")]
    [Tooltip("Waktu tunggu sesudah jatuh sebelum memanggil onComplete (detik).")]
    [SerializeField] private float postImpactDelay = 0.7f;

    public void Play(Transform ingredient, Collider2D targetArea, Transform panContentContainer, Action onComplete)
    {
        if (ingredient == null)
        {
            Debug.LogWarning("[IngredientDropCutscene] Ingredient is null");
            onComplete?.Invoke();
            return;
        }

        if (targetArea == null)
        {
            Debug.LogWarning("[IngredientDropCutscene] Target Area (Collider2D) is null!");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(DropSequence(ingredient, targetArea, panContentContainer, onComplete));
    }

    private IEnumerator DropSequence(Transform ingredient, Collider2D targetArea, Transform panContentContainer, Action onComplete)
    {
        Vector3 panCenter = targetArea.bounds.center;
        float maxPanRadius = targetArea.bounds.extents.x;
        float maxPanRadiusY = targetArea.bounds.extents.y;
        float isometricYSquash = maxPanRadius == 0 ? 1f : maxPanRadiusY / maxPanRadius;

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

        for (int i = 0; i < totalPieces; i++)
        {
            Vector2 spread = UnityEngine.Random.insideUnitCircle * spreadRadius;
            Vector3 targetGlobalPos = panCenter + new Vector3(spread.x, spread.y * isometricYSquash, 0f);

            float randomAngle = UnityEngine.Random.Range(-maxRotation, maxRotation);
            Quaternion targetRot = pieces[i].localRotation * Quaternion.Euler(60f, 0f, randomAngle);

            StartCoroutine(AnimateSinglePiece(pieces[i], targetGlobalPos, targetRot, splashEffect, () => completedPieces++));

            yield return new WaitForSeconds(UnityEngine.Random.Range(0.05f, maxStaggerDelay));
        }

        while (completedPieces < totalPieces)
        {
            yield return null;
        }

        yield return new WaitForSeconds(postImpactDelay);
        onComplete?.Invoke();
    }

    private IEnumerator AnimateSinglePiece(Transform piece, Vector3 endPos, Quaternion endRot, ParticleSystem splash, Action onComplete)
    {
        piece.gameObject.SetActive(true);

        Vector3 startPos = piece.position;
        Quaternion startRot = piece.rotation;
        float elapsed = 0f;

        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dropDuration);

            float arcHeight = 2.0f;
            float arc = Mathf.Sin(t * Mathf.PI) * arcHeight;

            float x = Mathf.Lerp(startPos.x, endPos.x, t);
            float y = Mathf.Lerp(startPos.y, endPos.y, t) + arc;
            float z = Mathf.Lerp(startPos.z, endPos.z, t);

            piece.position = new Vector3(x, y, z);
            piece.rotation = Quaternion.Lerp(startRot, endRot, t);

            yield return null;
        }

        piece.position = endPos;
        piece.rotation = endRot;

        StartCoroutine(SquashAndStretch(piece, piece.localScale.x, 0.2f));

        if (splash != null)
        {
            splash.transform.position = endPos;
            splash.Play();
        }

        onComplete?.Invoke();
    }

    private IEnumerator SquashAndStretch(Transform target, float originalScale, float totalDuration)
    {
        float halfDuration = totalDuration / 2f;
        Vector3 startScale = new Vector3(originalScale, originalScale, originalScale);
        Vector3 squashedScale = new Vector3(originalScale * 1.2f, originalScale * 0.8f, originalScale);

        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(startScale, squashedScale, elapsed / halfDuration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            target.localScale = Vector3.Lerp(squashedScale, startScale, elapsed / halfDuration);
            yield return null;
        }

        target.localScale = startScale;
    }
}