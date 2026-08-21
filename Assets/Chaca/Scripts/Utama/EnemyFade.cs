using System.Collections;
using UnityEngine;

public class EnemyFade : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    private bool fading;

    public void FadeAndDestroy()
    {
        if (fading)
            return;

        fading = true;

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        SpriteRenderer[] renderers =
            GetComponentsInChildren<SpriteRenderer>();

        if (renderers.Length == 0)
        {
            Destroy(gameObject);
            yield break;
        }

        Color[] originalColors =
            new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                originalColors[i] =
                    renderers[i].color;
            }
        }

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / fadeDuration
                );

            float alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    progress
                );

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                    continue;

                Color color =
                    originalColors[i];

                color.a =
                    originalColors[i].a *
                    alpha;

                renderers[i].color =
                    color;
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}