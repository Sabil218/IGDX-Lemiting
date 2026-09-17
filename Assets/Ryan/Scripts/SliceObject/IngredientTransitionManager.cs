using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientTransitionManager : MonoBehaviour
{
    [Header("Transition Settings")]
    public float exitDuration = 0.35f;
    public float enterDuration = 0.3f;
    public float exitDistance = 5f;
    public Vector3 enterOffset = new Vector3(0, 1.2f, 0);

    public AnimationCurve exitEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve enterEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public bool isTransitioning { get; private set; }

    public void TransitionToNextIngredient(GameObject current, GameObject next, Vector3 boardPos, Action onTransitionComplete = null)
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[IngredientTransitionManager] Transition requested while already transitioning. Forcing callback to prevent hang.");
            onTransitionComplete?.Invoke();
            return;
        }
        StartCoroutine(TransitionRoutine(current, next, boardPos, onTransitionComplete));
    }

    private IEnumerator TransitionRoutine(GameObject current, GameObject next, Vector3 boardPos, Action onTransitionComplete)
    {
        isTransitioning = true;
        Coroutine exitCor = null;

        // 1. Exit current ingredient if it exists (Run in background!)
        if (current != null)
        {
            exitCor = StartCoroutine(ExitRoutine(current));
        }

        // 2. Enter next ingredient if it exists (Run simultaneously!)
        if (next != null)
        {
            yield return StartCoroutine(EnterRoutine(next, boardPos));
        }
        else
        {
            // Give one frame to allow exitCor to actually start if EnterRoutine didn't run
            yield return null;
        }

        // Wait for exit to finish if it takes longer than enter
        if (exitCor != null)
        {
            yield return exitCor;
        }

        isTransitioning = false;
        
        Debug.Log("[IngredientTransitionManager] Transition complete! Invoking callback.");
        onTransitionComplete?.Invoke();
    }

    private IEnumerator ExitRoutine(GameObject currentIngredient)
    {
        Vector3 startPos = currentIngredient.transform.position;
        Vector3 endPos = startPos + Vector3.right * exitDistance;
        
        SpriteRenderer[] spriteRenderers = currentIngredient.GetComponentsInChildren<SpriteRenderer>();
        List<Color> spriteStartColors = new List<Color>();
        foreach (var sr in spriteRenderers)
        {
            spriteStartColors.Add(sr.color);
        }

        UnityEngine.UI.Graphic[] uiGraphics = currentIngredient.GetComponentsInChildren<UnityEngine.UI.Graphic>();
        List<Color> uiStartColors = new List<Color>();
        foreach (var g in uiGraphics)
        {
            uiStartColors.Add(g.color);
        }

        float elapsed = 0f;
        while (elapsed < exitDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / exitDuration);
            float curveT = exitEaseCurve.Evaluate(t);

            // Move
            currentIngredient.transform.position = Vector3.LerpUnclamped(startPos, endPos, curveT);

            // Fade Sprites
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    Color c = spriteStartColors[i];
                    c.a = Mathf.Lerp(spriteStartColors[i].a, 0f, curveT);
                    spriteRenderers[i].color = c;
                }
            }

            // Fade UI Graphics (like Guidelines)
            for (int i = 0; i < uiGraphics.Length; i++)
            {
                if (uiGraphics[i] != null)
                {
                    Color c = uiStartColors[i];
                    c.a = Mathf.Lerp(uiStartColors[i].a, 0f, curveT);
                    uiGraphics[i].color = c;
                }
            }

            yield return null;
        }

        // Clean up
        if (currentIngredient.scene.IsValid())
        {
            // If it's a pre-placed scene object, just hide it
            currentIngredient.SetActive(false);
        }
        else
        {
            // If it's an instantiated prefab, destroy it
            Destroy(currentIngredient);
        }
    }

    private IEnumerator EnterRoutine(GameObject newIngredient, Vector3 targetPosition)
    {
        Vector3 startPos = targetPosition + enterOffset;
        newIngredient.transform.position = startPos;
        newIngredient.SetActive(true);

        SpriteRenderer[] spriteRenderers = newIngredient.GetComponentsInChildren<SpriteRenderer>(true);
        List<Color> spriteTargetColors = new List<Color>();
        foreach (var sr in spriteRenderers)
        {
            spriteTargetColors.Add(sr.color);
            Color c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        UnityEngine.UI.Graphic[] uiGraphics = newIngredient.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
        List<Color> uiTargetColors = new List<Color>();
        foreach (var g in uiGraphics)
        {
            uiTargetColors.Add(g.color);
            Color c = g.color;
            c.a = 0f;
            g.color = c;
        }

        float elapsed = 0f;
        while (elapsed < enterDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / enterDuration);
            float curveT = enterEaseCurve.Evaluate(t);

            // Move
            newIngredient.transform.position = Vector3.LerpUnclamped(startPos, targetPosition, curveT);

            // Fade in Sprites
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    Color c = spriteTargetColors[i];
                    c.a = Mathf.Lerp(0f, spriteTargetColors[i].a, curveT);
                    spriteRenderers[i].color = c;
                }
            }

            // Fade in UI Graphics
            for (int i = 0; i < uiGraphics.Length; i++)
            {
                if (uiGraphics[i] != null)
                {
                    Color c = uiTargetColors[i];
                    c.a = Mathf.Lerp(0f, uiTargetColors[i].a, curveT);
                    uiGraphics[i].color = c;
                }
            }

            yield return null;
        }

        // Ensure exact final state
        newIngredient.transform.position = targetPosition;
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null) spriteRenderers[i].color = spriteTargetColors[i];
        }
        for (int i = 0; i < uiGraphics.Length; i++)
        {
            if (uiGraphics[i] != null) uiGraphics[i].color = uiTargetColors[i];
        }
    }
}
