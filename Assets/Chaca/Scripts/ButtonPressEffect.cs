using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Sound")]
    public AudioClip clickSound;

    [Range(0f, 1f)]
    public float soundVolume = 1f;

    [Header("Button Animation")]
    public float pressedScale = 0.9f;
    public float animationDuration = 0.08f;

    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Sound
        if (clickSound != null)
        {
            AudioSource.PlayClipAtPoint(
                clickSound,
                Camera.main.transform.position,
                soundVolume
            );
        }

        // Mengecilkan tombol
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(
            ScaleButton(originalScale * pressedScale)
        );
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(
            ScaleButton(originalScale)
        );
    }

    private IEnumerator ScaleButton(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = timer / animationDuration;

            transform.localScale = Vector3.Lerp(
                startScale,
                targetScale,
                progress
            );

            yield return null;
        }

        transform.localScale = targetScale;
    }
}