using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientDropCutscene : MonoBehaviour
{
    [Header("Drop Animation")]
    [Tooltip("How long the ingredient takes to fall into the pan (seconds).")]
    [SerializeField] private float dropDuration = 0.4f;
    [Tooltip("Vertical offset above the pan where the ingredient spawns.")]
    [SerializeField] private float spawnOffsetY = 6f;

    [Header("Scale & Pan Offset (Mencegah Numpuk Besar)")]
    [Tooltip("Target skala objek saat masuk wajan (misal: 0.4 agar mengecil 40%).")]
    [SerializeField] private float targetScale = 0.4f;
    [Tooltip("Radius sebaran acak mendarat di wajan agar antar-bahan tidak bertumpuk di titik persis sama.")]
    [SerializeField] private float panOffsetRadius = 0.5f;

    [Header("Spread Animation")]
    [Tooltip("Durasi animasi menyebar setelah mendarat.")]
    [SerializeField] private float spreadDuration = 0.15f;
    [Tooltip("Seberapa jauh potongan-potongan menyebar saat jatuh.")]
    [SerializeField] private float spreadRadius = 0.8f;
    [Tooltip("Memipihkan sebaran di sumbu Y (0.5 = oval/isometrik).")]
    [SerializeField] private float isometricYSquash = 0.5f; 
    [Tooltip("Maksimal rotasi acak saat potongan jatuh.")]
    [SerializeField] private float maxRotation = 120f;

    [Header("Visual Effects (Tercampur)")]
    [Tooltip("Memutar efek cipratan saat objek mendarat (Opsional). Masukkan prefab/particle system di sini.")]
    [SerializeField] private ParticleSystem splashEffect;
    [Tooltip("Apakah potongan bahan akan memudar/menghilang setelah jatuh seolah larut/tercampur di kuah?")]
    [SerializeField] private bool fadeOutAfterSpread = true;
    [Tooltip("Durasi waktu memudar (detik).")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Post-Impact")]
    [Tooltip("Pause after the impact before calling onComplete (seconds).")]
    [SerializeField] private float postImpactDelay = 0.7f;
    
    //Trigger drop animation
    public void Play(Transform ingredient, Vector3 panCenter, Action onComplete)
    {
        if (ingredient == null)
        {
            Debug.LogWarning("[IngredientDropCutscene] Ingredient is null — skipping cutscene.");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(DropSequence(ingredient, panCenter, onComplete));
    }

    //Execute drop sequence phases
    private IEnumerator DropSequence(Transform ingredient, Vector3 panCenter, Action onComplete)
    {
        Vector3 startPos = panCenter + Vector3.up * spawnOffsetY;
        
        Vector2 randomCirclePan = UnityEngine.Random.insideUnitCircle * panOffsetRadius;
        Vector3 endPos = panCenter + new Vector3(randomCirclePan.x, randomCirclePan.y * isometricYSquash, 0f);

        Vector3 startScale = ingredient.localScale;
        Vector3 finalScale = startScale * targetScale;

        ingredient.localScale = finalScale;

        // Pindahkan parent ke object cutscene ini agar tidak ikut bersembunyi saat container aslinya dimatikan
        ingredient.SetParent(this.transform, true);
        
        ingredient.position = startPos;
        ingredient.gameObject.SetActive(true);

        // Pre-calculate Spread Animation targets
        SpriteRenderer[] renderers = ingredient.GetComponentsInChildren<SpriteRenderer>();
        List<Transform> pieces = new List<Transform>();
        List<Vector3> startLocalPos = new List<Vector3>();
        List<Vector3> targetLocalPos = new List<Vector3>();
        List<Quaternion> startLocalRot = new List<Quaternion>();
        List<Quaternion> targetLocalRot = new List<Quaternion>();

        foreach (var sr in renderers)
        {
            if (sr.transform != ingredient)
            {
                pieces.Add(sr.transform);
                startLocalPos.Add(sr.transform.localPosition);

                Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
                if (direction == Vector2.zero) direction = Vector2.right; // Fallback jika nilai acak tepat (0,0)
                float distance = UnityEngine.Random.Range(spreadRadius * 0.4f, spreadRadius);
                Vector2 randomCircle = direction * distance;
                
                targetLocalPos.Add(sr.transform.localPosition + new Vector3(randomCircle.x, randomCircle.y * isometricYSquash, 0f));

                startLocalRot.Add(sr.transform.localRotation);
                float randomAngle = UnityEngine.Random.Range(-maxRotation, maxRotation);
                targetLocalRot.Add(sr.transform.localRotation * Quaternion.Euler(60f, 0f, randomAngle));
            }
        }

        // FASE 1: JATUH (Hanya mengatur posisi ke bawah)
        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dropDuration);

            float easedT = t * t; // Efek gravitasi (semakin cepat ke bawah)
            ingredient.position = Vector3.Lerp(startPos, endPos, easedT);

            yield return null;
        }
        ingredient.position = endPos;

        // MEMAINKAN EFEK CIPRATAN SAAT MENDARAT
        if (splashEffect != null)
        {
            splashEffect.transform.position = endPos;
            splashEffect.Play();
        }

        // FASE 2: MENYEBAR DAN MIRING SAAT MENDARAT
        elapsed = 0f;
        while (elapsed < spreadDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spreadDuration);

            float easedT = 1f - (1f - t) * (1f - t); // Ease out agar smooth saat menyebar

            // Sebarkan dan putar anak-anaknya (potongan)
            for (int i = 0; i < pieces.Count; i++)
            {
                pieces[i].localPosition = Vector3.Lerp(startLocalPos[i], targetLocalPos[i], easedT);
                pieces[i].localRotation = Quaternion.Lerp(startLocalRot[i], targetLocalRot[i], easedT);
            }

            yield return null;
        }

        // Pastikan semua posisi tepat di akhir animasi
        for (int i = 0; i < pieces.Count; i++)
        {
            pieces[i].localPosition = targetLocalPos[i];
            pieces[i].localRotation = targetLocalRot[i];
        }

        // FASE 3: EFEK TENGGELAM/LARUT (FADE OUT)
        if (fadeOutAfterSpread)
        {
            elapsed = 0f;
            Color[] startColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                startColors[i] = renderers[i].color;
            }

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);

                for (int i = 0; i < renderers.Length; i++)
                {
                    Color c = startColors[i];
                    c.a = Mathf.Lerp(startColors[i].a, 0f, t); // Turunkan Alpha menjadi 0 (transparan)
                    renderers[i].color = c;
                }
                yield return null;
            }

            // Sembunyikan objek setelah benar-benar pudar
            ingredient.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(postImpactDelay);

        onComplete?.Invoke();
    }
}