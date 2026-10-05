using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientDropCutscene : MonoBehaviour
{
    [Header("Spawn Reference")]
    // Titik awal di mana bahan makanan muncul (kotak rak kayu).
    [SerializeField] private Transform customSpawnPoint;

    [Header("Drop Movement")]
    // Kecepatan Drop
    [SerializeField] private float dropDuration = 0.65f;
    public float DropDuration => Mathf.Max(0.05f, dropDuration);
    // Ingredient Delay (0 = Immedieate Spawn)
    public float delayBeforeDrop = 0.35f;
    // Ketinggian busur lempar/tumpah (parabola) sebelum jatuh ke wajan
    [SerializeField] private float tossArcHeight = 0.55f;

    [Header("Sequential Drop Settings")]
    [Tooltip("Durasi lempar parabola untuk tiap potongan saat dropSequentially aktif.")]
    [SerializeField] private float sequentialPieceDuration = 0.45f;
    public float SequentialPieceDuration => Mathf.Max(0.05f, sequentialPieceDuration);
    [Tooltip("Jeda waktu antar potongan sebelum potongan berikutnya dilempar.")]
    [SerializeField] private float sequentialPieceDelay = 0.10f;

    [Header("Scale & Wajan Basin Bounds")]
    // Faktor skala bahan makanan saat berada di wajan.
    [SerializeField] private float targetScale = 0.65f;
    // Batas radius horizontal di dalam wajan.
    [SerializeField] private float spreadRadiusX = 2.0f;
    // Batas radius vertikal atas di dalam wajan.
    [SerializeField] private float spreadRadiusYUp = 0.65f;
    // Batas radius vertikal bawah di dalam wajan.
    [SerializeField] private float spreadRadiusYDown = 0.40f;
    //Z Rotation
    [SerializeField] private float maxRandomRotationZ = 12f;

    [Header("Visual Effects & Juice")]
    [SerializeField] private ParticleSystem splashEffect;
    [SerializeField] private AudioClip popSfx;
    [SerializeField] private AudioClip landingSfx;
    // Hentakan mendarat (micro-squash pada skala tanpa mengubah koordinat posisi)
    [SerializeField] private float impactSquashIntensity = 0.18f;
    [SerializeField] private float impactSquashDuration = 0.09f;

    [Header("Camera")]
    [SerializeField] private float postImpactDelay = 0.45f;

    [Header("Debug")]
    [SerializeField] private Transform debugDropArea;

    // Pocket tracking untuk bahan-bahan sekunder di sekitar ayam
    private int currentPocketIndex = 0;
    private Dictionary<Transform, Vector3> originalScales = new Dictionary<Transform, Vector3>();

    // Pocket posisi default (-1 s/d 1 relatif terhadap radius wajan) dengan alur sebaran alami
    private static readonly Vector2[] DefaultBrothPockets = new Vector2[]
    {
        new Vector2(-0.70f,  0.20f),  // 1. Kiri tengah
        new Vector2(-0.50f,  0.60f),  // 2. Kiri atas
        new Vector2( 0.05f,  0.75f),  // 3. Atas tengah
        new Vector2( 0.55f,  0.55f),  // 4. Kanan atas
        new Vector2( 0.70f, -0.05f),  // 5. Kanan tengah
        new Vector2( 0.45f, -0.60f),  // 6. Kanan bawah
        new Vector2(-0.15f, -0.75f),  // 7. Bawah tengah
        new Vector2(-0.55f, -0.45f)   // 8. Kiri bawah
    };

    [Header("Drop Pockets (Relative -1 to 1)")]
    // Titik sebaran relatif (-1 sampai 1). Jika kosong, otomatis menggunakan DefaultBrothPockets
    [SerializeField] private Vector2[] customPockets;

    private Vector2[] GetPockets()
    {
        if (customPockets != null && customPockets.Length > 0)
        {
            return customPockets;
        }
        return DefaultBrothPockets;
    }

    public void ResetDropHistory()
    {
        currentPocketIndex = 0;
        originalScales.Clear();
    }

    public void ResetClusterMemory()
    {
        ResetDropHistory();
    }

    private void Awake()
    {
        EnsureSpawnPoint();
    }

    public void EnsureSpawnPoint()
    {
        if (customSpawnPoint == null)
        {
            Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t != null && t.gameObject.scene.IsValid() && t.name == "IngredientSpawnPoint")
                {
                    customSpawnPoint = t;
                    break;
                }
            }
        }
    }

    // Menampilkan dan mengaktifkan bahan makanan tepat di rak kayu (IngredientSpawnPoint).
    public IEnumerator RevealAtSpawnPoint(Transform ingredient, Transform spawnPoint = null)
    {
        if (ingredient == null) yield break;

        EnsureSpawnPoint();
        Transform targetSpawn = spawnPoint != null ? spawnPoint : customSpawnPoint;

        Vector3 spawnPos = targetSpawn != null ? targetSpawn.position : transform.position;
        spawnPos.z = 0f;

        // Catat skala dasar awal agar tidak mengecil berulang kali
        if (!originalScales.ContainsKey(ingredient))
        {
            originalScales[ingredient] = ingredient.localScale;
        }
        Vector3 baseScale = originalScales[ingredient];

        // Tempatkan di spawn point dan aktifkan visualnya
        ingredient.position = spawnPos;
        ingredient.rotation = Quaternion.identity;
        ingredient.gameObject.SetActive(true);

        // Pastikan SpriteRenderer yang aktif tetap aktif tanpa mengubah sorting order
        SpriteRenderer[] renderers = ingredient.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var sr in renderers)
        {
            // PENTING: Jangan flip/toggle status enabled jika sprite tersebut memang sengaja dimatikan
            // (misalnya badan ikan utuh yang sudah di-disable saat ForceSlicedState)
            if (sr.enabled)
            {
                sr.gameObject.SetActive(true);
            }
        }

        // Jika objek adalah bahan yang dipotong (FoodSlicer), pastikan sprite induk utuh dimatikan permanen
        if (ingredient.GetComponent<FoodSlicer>() != null)
        {
            SpriteRenderer rootSr = ingredient.GetComponent<SpriteRenderer>();
            if (rootSr != null) rootSr.enabled = false;
        }

        if (popSfx != null)
        {
            AudioSource.PlayClipAtPoint(popSfx, spawnPos);
        }

        // Animasi pop-in halus dari 0.1x ke skala dasar
        float popDuration = 0.22f;
        float elapsed = 0f;
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popDuration);
            // Smooth ease-out dengan sedikit elastisitas manis
            float ease = Mathf.Sin(t * Mathf.PI * 0.5f);
            ingredient.localScale = Vector3.LerpUnclamped(baseScale * 0.1f, baseScale, ease);
            yield return null;
        }

        ingredient.localScale = baseScale;
    }

    // Menjatuhkan bahan makanan dari rak kayu langsung ke wajan secara mulus.
    public void DropToPan(Transform ingredient, Transform spawnPoint, Transform targetArea, Transform panContentContainer, bool disableRotation, bool dropInCenter, Action onComplete)
    {
        DropToPan(ingredient, spawnPoint, targetArea, panContentContainer, disableRotation, dropInCenter, false, onComplete, null);
    }

    public void DropToPan(Transform ingredient, Transform spawnPoint, Transform targetArea, Transform panContentContainer, bool disableRotation, bool dropInCenter, bool dropSequentially, Action onComplete)
    {
        DropToPan(ingredient, spawnPoint, targetArea, panContentContainer, disableRotation, dropInCenter, dropSequentially, onComplete, null);
    }

    public void DropToPan(Transform ingredient, Transform spawnPoint, Transform targetArea, Transform panContentContainer, bool disableRotation, bool dropInCenter, bool dropSequentially, Action onComplete, Action onCameraPanTrigger)
    {
        if (ingredient == null || targetArea == null)
        {
            onCameraPanTrigger?.Invoke();
            onComplete?.Invoke();
            return;
        }

        // Cek apakah ingredient memiliki child pieces yang aktif (hasil potongan/slicing)
        List<Transform> activePieces = new List<Transform>();
        SpriteRenderer[] renderers = ingredient.GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
        {
            if (sr.enabled && sr.gameObject.activeInHierarchy && sr.transform != ingredient)
            {
                activePieces.Add(sr.transform);
            }
        }

        // Jika opsi dropSequentially aktif dan terdapat lebih dari 1 potongan (misal Bawang Merah 3 potongan):
        // Munculkan semua potongan di rak, lalu lempar 1 per 1 secara bergantian ke wajan dalam loop.
        // Jika false (default):
        // Jatuhkan seluruh objek/grup langsung bersamaan (seperti contoh pada Ikan).
        if (dropSequentially && activePieces.Count > 1)
        {
            StartCoroutine(DropPiecesSequentiallyRoutine(ingredient, activePieces, spawnPoint, targetArea, panContentContainer, disableRotation, dropInCenter, onComplete, onCameraPanTrigger));
        }
        else
        {
            StartCoroutine(DropToPanRoutine(ingredient, spawnPoint, targetArea, panContentContainer, disableRotation, dropInCenter, onComplete, onCameraPanTrigger));
        }
    }

    private IEnumerator DropPiecesSequentiallyRoutine(Transform ingredient, List<Transform> pieces, Transform spawnPoint, Transform targetArea, Transform panContentContainer, bool disableRotation, bool dropInCenter, Action onComplete, Action onCameraPanTrigger = null)
    {
        EnsureSpawnPoint();
        Transform targetSpawn = spawnPoint != null ? spawnPoint : customSpawnPoint;

        Vector3 startPos = (targetSpawn != null) ? targetSpawn.position : ingredient.position;
        startPos.z = 0f;
        ingredient.position = startPos;

        Vector3 panCenter = targetArea.position;
        panCenter.z = 0f;

        float pieceDuration = Mathf.Max(0.05f, sequentialPieceDuration);
        float finalScaleFactor = (targetScale > 0f) ? targetScale : 0.65f;
        int totalPieces = pieces.Count;

        // Urutkan potongan dari kiri ke kanan agar animasi teratur dan estetik
        pieces.Sort((a, b) => a.position.x.CompareTo(b.position.x));
        int lastValidIndex = pieces.FindLastIndex(p => p != null);

        Vector3 groupCenter = Vector3.zero;
        if (dropInCenter)
        {
            foreach (var p in pieces)
            {
                if (p != null) groupCenter += p.position;
            }
            if (totalPieces > 0) groupCenter /= totalPieces;
        }

        Vector2[] pockets = GetPockets();

        for (int i = 0; i < totalPieces; i++)
        {
            Transform piece = pieces[i];
            if (piece == null) continue;

            // Kamera baru mulai bergerak ke wajan tepat saat potongan TERAKHIR mulai dilempar
            if (i == lastValidIndex)
            {
                onCameraPanTrigger?.Invoke();
            }

            Vector3 pieceLandingPos;
            Quaternion targetRot;

            if (dropInCenter)
            {
                Vector3 offsetFromGroup = piece.position - groupCenter;
                pieceLandingPos = panCenter + (offsetFromGroup * finalScaleFactor);
                pieceLandingPos.z = 0f;
                targetRot = piece.rotation;
            }
            else
            {
                Vector2 pocket = pockets[currentPocketIndex % pockets.Length];
                currentPocketIndex++;

                float limitY = pocket.y >= 0f ? spreadRadiusYUp : spreadRadiusYDown;
                float jitterX = UnityEngine.Random.Range(-0.06f, 0.06f) * spreadRadiusX;
                float jitterY = UnityEngine.Random.Range(-0.05f, 0.05f) * limitY;

                float offsetX = (pocket.x * spreadRadiusX) + jitterX;
                float offsetY = (pocket.y * limitY) + jitterY;

                float normX = offsetX / spreadRadiusX;
                float normY = offsetY / limitY;
                float distSq = normX * normX + normY * normY;
                if (distSq > 1f)
                {
                    float scale = 0.95f / Mathf.Sqrt(distSq);
                    offsetX *= scale;
                    offsetY *= scale;
                }

                pieceLandingPos = panCenter + new Vector3(offsetX, offsetY, 0f);

                float rotZVariation = disableRotation ? 0f : UnityEngine.Random.Range(-maxRandomRotationZ, maxRandomRotationZ);
                targetRot = disableRotation ? Quaternion.identity : piece.rotation * Quaternion.Euler(0f, 0f, rotZVariation);
            }

            // Animasikan potongan ini melayang dalam parabola secara mandiri menuju target wajan
            yield return StartCoroutine(AnimatePieceParabolaRoutine(piece, pieceLandingPos, targetRot, finalScaleFactor, pieceDuration, panContentContainer));

            if (i < totalPieces - 1 && sequentialPieceDelay > 0f)
            {
                yield return new WaitForSeconds(sequentialPieceDelay);
            }
        }

        // Pastikan transform induk juga diparent ke container wajan agar tersinkronisasi
        if (ingredient != null && panContentContainer != null)
        {
            ingredient.SetParent(panContentContainer, true);
        }

        if (postImpactDelay > 0f)
        {
            yield return new WaitForSeconds(postImpactDelay);
        }

        onComplete?.Invoke();
    }

    private IEnumerator AnimatePieceParabolaRoutine(Transform piece, Vector3 targetLandingPos, Quaternion targetRot, float scaleFactor, float duration, Transform panContentContainer = null)
    {
        Vector3 startPos = piece.position;
        Quaternion startRot = piece.rotation;
        Vector3 startScale = piece.localScale;
        Vector3 finalScale = startScale * scaleFactor;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float tX = Mathf.SmoothStep(0f, 1f, t);
            float arc = 4f * t * (1f - t) * tossArcHeight;
            float gravityFall = t * t;

            float curX = Mathf.Lerp(startPos.x, targetLandingPos.x, tX);
            float curY = Mathf.Lerp(startPos.y, targetLandingPos.y, gravityFall) + arc;
            float curZ = Mathf.Lerp(startPos.z, targetLandingPos.z, t);

            piece.position = new Vector3(curX, curY, curZ);
            piece.rotation = Quaternion.Lerp(startRot, targetRot, t);
            piece.localScale = Vector3.Lerp(startScale, finalScale, t);

            yield return null;
        }

        piece.position = targetLandingPos;
        piece.rotation = targetRot;
        piece.localScale = finalScale;

        // Efek splash & SFX saat potongan mendarat
        if (splashEffect != null)
        {
            splashEffect.transform.position = targetLandingPos;
            splashEffect.Play();
        }

        if (landingSfx != null)
        {
            AudioSource.PlayClipAtPoint(landingSfx, targetLandingPos);
        }

        // Micro squash on landing
        if (impactSquashIntensity > 0f && impactSquashDuration > 0f)
        {
            float impactElapsed = 0f;
            while (impactElapsed < impactSquashDuration)
            {
                impactElapsed += Time.deltaTime;
                float it = Mathf.Clamp01(impactElapsed / impactSquashDuration);
                float decay = 1f - it;
                float squashX = 1f + (impactSquashIntensity * 0.75f * decay);
                float squashY = 1f - (impactSquashIntensity * decay);

                piece.localScale = new Vector3(finalScale.x * squashX, finalScale.y * squashY, finalScale.z);
                yield return null;
            }
            piece.localScale = finalScale;
        }

        // Langsung jangkar piece ke wajan agar aman
        if (panContentContainer != null)
        {
            piece.SetParent(panContentContainer, true);
        }
    }

    private IEnumerator DropToPanRoutine(Transform ingredient, Transform spawnPoint, Transform targetArea, Transform panContentContainer, bool disableRotation, bool dropInCenter, Action onComplete, Action onCameraPanTrigger = null)
    {
        EnsureSpawnPoint();
        Transform targetSpawn = spawnPoint != null ? spawnPoint : customSpawnPoint;

        Vector3 startPos = (targetSpawn != null) ? targetSpawn.position : ingredient.position;
        startPos.z = 0f;
        ingredient.position = startPos;

        Vector3 panCenter = targetArea.position;
        panCenter.z = 0f;

        // Pemicu kamera untuk drop langsung
        onCameraPanTrigger?.Invoke();

        // 1. Tentukan target akhir di wajan SEJAK AWAL
        Vector3 targetLandingPos;
        if (dropInCenter)
        {
            // Bahan utama seperti ayam mendarat pas di tengah wajan
            targetLandingPos = panCenter;
        }
        else
        {
            // Bahan pendukung mendarat di pocket wajan dengan variasi alami
            Vector2[] pockets = GetPockets();
            Vector2 pocket = pockets[currentPocketIndex % pockets.Length];
            currentPocketIndex++;

            // Skala offset berdasarkan radius wajan agar menyebar proporsional mengisi ruang kosong
            float limitY = pocket.y >= 0f ? spreadRadiusYUp : spreadRadiusYDown;
            float jitterX = UnityEngine.Random.Range(-0.08f, 0.08f) * spreadRadiusX;
            float jitterY = UnityEngine.Random.Range(-0.06f, 0.06f) * limitY;

            float offsetX = (pocket.x * spreadRadiusX) + jitterX;
            float offsetY = (pocket.y * limitY) + jitterY;

            // Clamp batas oval wajan agar tidak keluar dari bibir wajan
            float normX = offsetX / spreadRadiusX;
            float normY = offsetY / limitY;
            float distSq = normX * normX + normY * normY;
            if (distSq > 1f)
            {
                float scale = 0.95f / Mathf.Sqrt(distSq);
                offsetX *= scale;
                offsetY *= scale;
            }

            targetLandingPos = panCenter + new Vector3(offsetX, offsetY, 0f);
        }

        // Tentukan rotasi akhir yang lembut (tanpa putaran berlebihan)
        Quaternion startRot = ingredient.rotation;
        float targetRotZ = disableRotation ? 0f : UnityEngine.Random.Range(-maxRandomRotationZ, maxRandomRotationZ);
        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetRotZ);

        // Tentukan skala target di wajan
        if (!originalScales.ContainsKey(ingredient))
        {
            originalScales[ingredient] = ingredient.localScale;
        }
        Vector3 baseScale = originalScales[ingredient];
        Vector3 finalScale = (targetScale > 0f) ? (baseScale * targetScale) : baseScale;
        Vector3 currentScale = ingredient.localScale;

        // 2. Animasi meluncur/jatuh ke wajan dengan fisika gravitasi & arc lemparan
        float duration = Mathf.Max(0.05f, dropDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Gerakan horizontal menyeberang secara smooth
            float tX = Mathf.SmoothStep(0f, 1f, t);

            // Gerakan vertikal: Arc parabola (tumpah) + percepatan gravitasi (Ease-In Quad)
            // Menghasilkan kecepatan jatuh yang berakselerasi dan menumbuk wajan tanpa mengerem
            float arc = 4f * t * (1f - t) * tossArcHeight;
            float gravityFall = t * t;

            float curX = Mathf.Lerp(startPos.x, targetLandingPos.x, tX);
            float curY = Mathf.Lerp(startPos.y, targetLandingPos.y, gravityFall) + arc;
            float curZ = Mathf.Lerp(startPos.z, targetLandingPos.z, t);

            ingredient.position = new Vector3(curX, curY, curZ);
            ingredient.rotation = Quaternion.Lerp(startRot, targetRot, t);
            ingredient.localScale = Vector3.Lerp(currentScale, finalScale, t);

            yield return null;
        }

        // 3. Touchdown TEPAT di targetLandingPos: 100% terkunci dan tidak pernah bergeser
        ingredient.position = targetLandingPos;
        ingredient.rotation = targetRot;

        // Efek cipratan/splash di titik pendaratan
        if (splashEffect != null)
        {
            splashEffect.transform.position = targetLandingPos;
            splashEffect.Play();
        }

        if (landingSfx != null)
        {
            AudioSource.PlayClipAtPoint(landingSfx, targetLandingPos);
        }

        // 4. Micro Squash & Stretch pada skala (Bukan posisi!) untuk sensasi benturan fisik wajan
        if (impactSquashIntensity > 0f && impactSquashDuration > 0f)
        {
            float impactElapsed = 0f;
            while (impactElapsed < impactSquashDuration)
            {
                impactElapsed += Time.deltaTime;
                float it = Mathf.Clamp01(impactElapsed / impactSquashDuration);
                // Sumbu Y memipih (squash), sumbu X melebar (stretch), lalu membal kembali ke normal (decay)
                float decay = 1f - it;
                float squashX = 1f + (impactSquashIntensity * 0.75f * decay);
                float squashY = 1f - (impactSquashIntensity * decay);

                ingredient.localScale = new Vector3(finalScale.x * squashX, finalScale.y * squashY, finalScale.z);
                yield return null;
            }
        }
        ingredient.localScale = finalScale;

        // 5. Kaitkan ke container wajan (panContentContainer) dengan worldPositionStays = true
        // Posisi world ingredient dijamin tetap persis di targetLandingPos tanpa bergeser
        if (panContentContainer != null)
        {
            ingredient.SetParent(panContentContainer, true);
        }

        // Jeda tenang setelah mendarat
        if (postImpactDelay > 0f)
        {
            yield return new WaitForSeconds(postImpactDelay);
        }

        onComplete?.Invoke();
    }

    // Overload untuk kompatibilitas
    public void Play(Transform ingredient, Transform targetArea, Transform panContentContainer, Action onComplete)
    {
        Play(ingredient, targetArea, panContentContainer, false, false, onComplete);
    }

    public void Play(Transform ingredient, Transform targetArea, Transform panContentContainer, bool disableRotation, bool dropInCenter, Action onComplete)
    {
        EnsureSpawnPoint();
        DropToPan(ingredient, customSpawnPoint, targetArea, panContentContainer, disableRotation, dropInCenter, onComplete);
    }

    public void Play(Transform ingredient, Transform spawnPoint, Transform targetArea, Transform panContentContainer, bool disableRotation, bool dropInCenter, Action onComplete)
    {
        DropToPan(ingredient, spawnPoint, targetArea, panContentContainer, disableRotation, dropInCenter, onComplete);
    }

    public IEnumerator PopAtSpawnPoint(Transform ingredient, Transform targetArea, Transform panContentContainer)
    {
        yield return StartCoroutine(RevealAtSpawnPoint(ingredient, customSpawnPoint));
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
        if (area == null)
        {
            WordMatchingManager wmm = FindObjectOfType<WordMatchingManager>();
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

        // Gambar titik-titik pendaratan bahan (broth pockets) di dalam wajan
        Gizmos.color = Color.yellow;
        Vector2[] pockets = GetPockets();
        if (pockets != null)
        {
            for (int i = 0; i < pockets.Length; i++)
            {
                float limitY = pockets[i].y >= 0f ? spreadRadiusYUp : spreadRadiusYDown;
                Vector3 pocketPos = c + new Vector3(pockets[i].x * spreadRadiusX, pockets[i].y * limitY, 0f);
                Gizmos.DrawWireSphere(pocketPos, 0.12f);
#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.yellow;
                UnityEditor.Handles.Label(pocketPos + Vector3.up * 0.12f, $"Spot {i + 1}");
#endif
            }
        }
    }
}