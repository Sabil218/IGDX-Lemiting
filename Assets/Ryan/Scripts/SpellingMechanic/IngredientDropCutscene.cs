using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientDropCutscene : MonoBehaviour
{
    [Header("Drop Animation")]
    [Tooltip("Fall duration in seconds for a gentle, caring drop.")]
    [SerializeField] private float dropDuration = 0.65f;
    [SerializeField] private float spawnOffsetY = 3.8f;
    [Tooltip("Small delay so the camera starts blending down before the ingredient falls into view.")]
    [SerializeField] private float delayBeforeDrop = 0.25f;
    [SerializeField] private float maxStaggerDelay = 0.05f;

    [Header("Scale & Wajan Basin Bounds")]
    [SerializeField] private float targetScale = 0.6f;
    [Tooltip("Maximum horizontal spread inside the wajan.")]
    [SerializeField] private float spreadRadiusX = 2.0f;
    [Tooltip("Maximum upper spread inside the wajan.")]
    [SerializeField] private float spreadRadiusYUp = 0.65f;
    [Tooltip("Maximum lower spread inside the wajan.")]
    [SerializeField] private float spreadRadiusYDown = 0.40f;
    [Tooltip("Gentle tilt on landing.")]
    [SerializeField] private float maxRandomRotationZ = 20f;

    [Header("Visual Effects & Juice")]
    [SerializeField] private ParticleSystem splashEffect;
    [SerializeField] private AudioClip popSfx;
    [Tooltip("Gentle settling cushion on landing.")]
    [SerializeField] private bool enableSquashAndStretch = true;
    [SerializeField] private float squashAmount = 0.08f;
    [SerializeField] private float squashDuration = 0.16f;

    [Header("Post-Impact")]
    [SerializeField] private float postImpactDelay = 0.65f;

    [Header("Debug")]
    [SerializeField] private Transform debugDropArea;

    // Sector / Pocket tracking
    private int currentPocketIndex = 0;
    private List<Transform> cachedPieces = new List<Transform>();

    // Natural broth pockets around the chicken inside the wajan oval
    private static readonly Vector2[] BrothPockets = new Vector2[]
    {
        new Vector2(-1.15f,  0.38f),  // Upper-left broth pocket
        new Vector2( 1.15f,  0.38f),  // Upper-right broth pocket
        new Vector2( 1.55f, -0.05f),  // Right edge broth
        new Vector2(-1.55f, -0.05f),  // Left edge broth
        new Vector2( 0.00f,  0.50f),  // Top center broth
        new Vector2( 0.50f, -0.22f),  // Lower-front right
        new Vector2(-0.50f, -0.22f)   // Lower-front left
    };

    public void ResetDropHistory()
    {
        currentPocketIndex = 0;
        cachedPieces.Clear();
    }

    public void ResetClusterMemory()
    {
        ResetDropHistory();
    }

    // Main entry point for gentle, caring drop into the pan
    public void Play(Transform ingredient, Transform targetArea, Transform panContentContainer, Action onComplete)
    {
        Play(ingredient, targetArea, panContentContainer, false, false, onComplete);
    }

    public void Play(Transform ingredient, Transform targetArea, Transform panContentContainer, bool disableRotation, bool dropInCenter, Action onComplete)
    {
        if (ingredient == null || targetArea == null)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(DropSequence(ingredient, targetArea, panContentContainer, disableRotation, dropInCenter, onComplete));
    }

    private IEnumerator DropSequence(Transform ingredient, Transform targetArea, Transform panContentContainer, bool disableRotation, bool dropInCenter, Action onComplete)
    {
        if (delayBeforeDrop > 0f)
        {
            yield return new WaitForSeconds(delayBeforeDrop);
        }

        Vector3 panCenter = targetArea.position;

        // Reparent cleanly so it follows the pan container
        if (panContentContainer != null)
        {
            ingredient.SetParent(panContentContainer, true);
        }
        else
        {
            ingredient.SetParent(this.transform, true);
        }

        // Apply visual scale
        ingredient.localScale = ingredient.localScale * targetScale;

        // Position directly above the pan (NOT across the room at the letter board)
        // This ensures the drop is vertical, gentle, and centered in the pan camera view
        Vector3 parentStartPos = panCenter + Vector3.up * Mathf.Clamp(spawnOffsetY, 3.2f, 4.2f);
        ingredient.position = parentStartPos;
        ingredient.gameObject.SetActive(true);

        if (popSfx != null)
        {
            AudioSource.PlayClipAtPoint(popSfx, parentStartPos);
        }

        // Collect all individual pieces
        SpriteRenderer[] renderers = ingredient.GetComponentsInChildren<SpriteRenderer>(true);
        List<Transform> pieces = new List<Transform>();

        foreach (var sr in renderers)
        {
            if (sr.transform != ingredient)
            {
                pieces.Add(sr.transform);
                sr.gameObject.SetActive(true);
            }
        }

        if (pieces.Count == 0)
        {
            SpriteRenderer rootSr = ingredient.GetComponent<SpriteRenderer>();
            if (rootSr != null)
            {
                pieces.Add(ingredient);
            }
        }

        int totalPieces = pieces.Count;
        int completedPieces = 0;

        // Determine landing position inside the wajan
        Vector3 baseTargetPos;

        if (dropInCenter)
        {
            // Centerpiece (Ayam or Ikan) lands gently right in the center of the wajan
            baseTargetPos = panCenter;
        }
        else
        {
            // Subsequent ingredients cycle through the open broth pockets around the chicken
            Vector2 pocket = BrothPockets[currentPocketIndex % BrothPockets.Length];
            currentPocketIndex++;

            float candX = pocket.x + UnityEngine.Random.Range(-0.15f, 0.15f);
            float candY = pocket.y + UnityEngine.Random.Range(-0.08f, 0.08f);

            // ABSOLUTE CLAMP: Guarantee point is 100% inside the wajan's inner oval
            float limitY = candY >= 0f ? spreadRadiusYUp : spreadRadiusYDown;
            float normX = candX / spreadRadiusX;
            float normY = candY / limitY;
            float distSq = normX * normX + normY * normY;
            if (distSq > 1f)
            {
                float scale = 0.92f / Mathf.Sqrt(distSq);
                candX *= scale;
                candY *= scale;
            }

            baseTargetPos = panCenter + new Vector3(candX, candY, 0f);
        }

        // Animate each piece falling gently into its spot
        float actualDropDuration = Mathf.Max(0.4f, dropDuration);

        for (int i = 0; i < totalPieces; i++)
        {
            Vector3 targetGlobalPos;

            if (totalPieces == 1)
            {
                targetGlobalPos = baseTargetPos;
            }
            else
            {
                // Multi-piece (chopped onion/garlic/cabai) fans out gently around the pocket
                float pieceAngle = (i * (360f / totalPieces) + UnityEngine.Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
                float pieceDist = UnityEngine.Random.Range(0.12f, 0.28f);
                float pOffsetX = Mathf.Cos(pieceAngle) * pieceDist;
                float pOffsetY = Mathf.Sin(pieceAngle) * pieceDist * 0.55f; // isometric squash
                targetGlobalPos = baseTargetPos + new Vector3(pOffsetX, pOffsetY, 0f);
            }

            // Piece starts slightly above its target X for a vertical drop
            Vector3 pieceStartPos = new Vector3(targetGlobalPos.x, parentStartPos.y, targetGlobalPos.z);
            pieces[i].position = pieceStartPos;

            float randomZ = disableRotation ? 0f : UnityEngine.Random.Range(-maxRandomRotationZ, maxRandomRotationZ);
            Quaternion targetRot = pieces[i].localRotation * Quaternion.Euler(0f, 0f, randomZ);

            StartCoroutine(AnimateSinglePiece(pieces[i], pieceStartPos, targetGlobalPos, targetRot, actualDropDuration, splashEffect, () => completedPieces++));

            if (totalPieces > 1 && maxStaggerDelay > 0f)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.02f, maxStaggerDelay));
            }
        }

        while (completedPieces < totalPieces)
        {
            yield return null;
        }

        // Anchor pieces to panContentContainer so they follow during stirring/cooking
        if (panContentContainer != null)
        {
            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] != null && pieces[i] != ingredient)
                {
                    pieces[i].SetParent(panContentContainer, true);
                }
            }
            if (ingredient != null)
            {
                ingredient.position = panCenter;
            }
        }

        float actualWait = Mathf.Max(0.2f, postImpactDelay);
        yield return new WaitForSeconds(actualWait);
        onComplete?.Invoke();
    }

    // Animates a single piece falling gently and settling with care into the wajan
    private IEnumerator AnimateSinglePiece(Transform piece, Vector3 startPos, Vector3 endPos, Quaternion endRot, float duration, ParticleSystem splash, Action onComplete)
    {
        piece.gameObject.SetActive(true);

        Quaternion startRot = piece.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // SmoothStep provides a gentle, natural, caring descent that cushions as it touches down
            float tFall = t * t * (3f - 2f * t);
            float currentY = Mathf.Lerp(startPos.y, endPos.y, tFall);

            // Smooth gentle horizontal drift into final resting spot
            float currentX = Mathf.Lerp(startPos.x, endPos.x, t);
            float currentZ = Mathf.Lerp(startPos.z, endPos.z, t);

            piece.position = new Vector3(currentX, currentY, currentZ);
            piece.rotation = Quaternion.Lerp(startRot, endRot, tFall);

            yield return null;
        }

        piece.position = endPos;
        piece.rotation = endRot;

        // Subtle splash on touchdown
        if (splash != null)
        {
            splash.transform.position = endPos;
            splash.Play();
        }

        // Gentle settling cushion (soft micro-squash)
        if (enableSquashAndStretch && piece != null)
        {
            yield return StartCoroutine(SquashAndStretchRoutine(piece, squashDuration));
        }

        onComplete?.Invoke();
    }

    private IEnumerator SquashAndStretchRoutine(Transform target, float duration)
    {
        if (target == null) yield break;

        Vector3 baseScale = target.localScale;
        Vector3 squashedScale = new Vector3(
            baseScale.x * (1f + squashAmount),
            baseScale.y * (1f - squashAmount),
            baseScale.z
        );

        float halfDuration = duration * 0.5f;
        float elapsed = 0f;

        // Gentle settle
        while (elapsed < halfDuration && target != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            target.localScale = Vector3.Lerp(baseScale, squashedScale, t);
            yield return null;
        }

        elapsed = 0f;
        // Rebound softly to normal
        while (elapsed < halfDuration && target != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            target.localScale = Vector3.Lerp(squashedScale, baseScale, t);
            yield return null;
        }

        if (target != null)
        {
            target.localScale = baseScale;
        }
    }

    // Backwards-compatible methods for other scripts
    public IEnumerator PopAtSpawnPoint(Transform ingredient, Transform targetArea, Transform panContentContainer)
    {
        // No-op or quick reveal (kept for compatibility)
        if (ingredient != null)
        {
            ingredient.gameObject.SetActive(true);
        }
        yield break;
    }

    public void StartDroppingPieces(Transform ingredient, Transform targetArea, Transform panContentContainer, bool disableRotation, bool dropInCenter, Action onComplete)
    {
        Play(ingredient, targetArea, panContentContainer, disableRotation, dropInCenter, onComplete);
    }

    private void OnDrawGizmosSelected()
    {
        Transform area = debugDropArea;
        if (area == null)
        {
            WordMatchingManager wmm = GetComponent<WordMatchingManager>();
            if (wmm != null && wmm.targetDropArea != null) area = wmm.targetDropArea;
        }
        if (area == null) return;

        Gizmos.color = Color.green;
        Vector3 c = area.position;
        int segments = 36;
        float angle = 0f;

        float startRadiusY = Mathf.Sin(0) > 0 ? spreadRadiusYUp : spreadRadiusYDown;
        Vector3 lastPoint = c + new Vector3(Mathf.Cos(0) * spreadRadiusX, Mathf.Sin(0) * startRadiusY, 0);

        for (int i = 1; i <= segments; i++)
        {
            angle += (360f / segments) * Mathf.Deg2Rad;
            float currentRadiusY = Mathf.Sin(angle) > 0 ? spreadRadiusYUp : spreadRadiusYDown;
            Vector3 nextPoint = c + new Vector3(Mathf.Cos(angle) * spreadRadiusX, Mathf.Sin(angle) * currentRadiusY, 0);
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }

        // Draw natural broth pockets
        Gizmos.color = Color.yellow;
        if (BrothPockets != null)
        {
            for (int i = 0; i < BrothPockets.Length; i++)
            {
                Gizmos.DrawWireSphere(c + new Vector3(BrothPockets[i].x, BrothPockets[i].y, 0f), 0.12f);
            }
        }
    }
}