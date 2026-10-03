using System;
using System.Collections;
using UnityEngine;

public class PlatingCutscene : MonoBehaviour
{
    [Header("Scene Actors (Visuals)")]
    public Transform wajan;
    public Transform piring;

    [Header("Food Drop Visibility")]
    public GameObject[] objectsToHideOnDrop;
    public GameObject[] objectsToShowOnDrop;

    [Header("Entry Animation")]
    public Transform piringStartPoint;
    public Transform piringMeetPoint;
    public Transform wajanStartPoint;
    public Transform wajanMeetPoint;
    public Vector3 piringStartOffset = new Vector3(0f, 12f, 0f);
    public Vector3 wajanStartOffset = new Vector3(-12f, 0f, 0f);
    public float entryDuration = 0.8f;
    // Jika true, piring diam di meja (stasioner) sejak awal dan hanya wajan yang meluncur masuk
    public bool keepPlateStationary = true;

    [Header("Post-Drop Effects")]
    public ParticleSystem dropEffect;
    public float shakeIntensity = 0.2f;
    public float shakeDuration = 0.3f;
    public float postDropDelay = 0.5f;

    [Header("Exit Animation")]
    public Vector3 wajanExitOffset = new Vector3(10f, 0f, 0f);
    public float exitDuration = 1.5f;
    public Vector3 plateTargetScale = new Vector3(1.5f, 1.5f, 1f);
    public Transform plateCenterPoint;
    public float zoomDuration = 1.0f;

    [Header("Final Presentation")]
    public GameObject[] finalUIObjects;
    public GameObject nextButton;
    public Transform finalAlasObject;
    public Transform finalAlasTargetPos;
    public Vector3 finalAlasStartOffset = new Vector3(0f, 10f, 0f);
    public float uiBounceDuration = 0.45f;
    public float uiStaggerDelay = 0.12f;

    [Header("Audio")]
    public AudioClip finalScreenSfx;
    public AudioClip dropToPlateSfx;
    [SerializeField] private AudioSource sfxAudioSource;

    private Camera mainCamera;
    private System.Collections.Generic.Dictionary<Transform, Vector3> initialScales = new System.Collections.Generic.Dictionary<Transform, Vector3>();

    private void Awake()
    {
        mainCamera = Camera.main;
        CacheInitialScales();
        HideFinalUI();
        SnapPlateIfStationary();
    }

    private void Start()
    {
        CacheInitialScales();
        HideFinalUI();
        SnapPlateIfStationary();
    }

    // Simpan skala asli elemen UI dan judul agar saat bounce kembali ke ukuran yang tepat
    private void CacheInitialScales()
    {
        if (finalUIObjects != null)
        {
            foreach (var obj in finalUIObjects)
            {
                if (obj != null && !initialScales.ContainsKey(obj.transform))
                {
                    initialScales[obj.transform] = obj.transform.localScale;
                }
            }
        }
        if (nextButton != null && !initialScales.ContainsKey(nextButton.transform))
        {
            initialScales[nextButton.transform] = nextButton.transform.localScale;
        }
    }

    // Pastikan posisi piring langsung berada di titik temu meja jika diset stasioner
    private void SnapPlateIfStationary()
    {
        if (keepPlateStationary && piring != null && piringMeetPoint != null)
        {
            Vector3 pTarget = piringMeetPoint.position;
            pTarget.z = 0f;
            piring.position = pTarget;
        }
    }

    // Sembunyikan elemen UI akhir saat fase memasak/menuang berlangsung
    public void HideFinalUI()
    {
        if (finalUIObjects != null)
        {
            foreach (var obj in finalUIObjects)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
        if (nextButton != null)
        {
            nextButton.SetActive(false);
        }
        if (finalAlasObject != null)
        {
            finalAlasObject.gameObject.SetActive(false);
        }
    }

    public void PlayEntryAnimation(Action onComplete)
    {
        HideFinalUI();
        StartCoroutine(EntrySequence(onComplete));
    }

    // Meluncurkan wajan masuk ke meja sementara piring tetap stasioner di posisinya
    private IEnumerator EntrySequence(Action onComplete)
    {
        if (wajan == null || piring == null)
        {
            Debug.LogWarning("Referensi Wajan atau Piring ada yang kosong di PlatingCutscene!");
            yield break;
        }

        Vector3 wTarget = (wajanMeetPoint != null) ? wajanMeetPoint.position : wajan.position;
        Vector3 pTarget = (piringMeetPoint != null) ? piringMeetPoint.position : piring.position;
        wTarget.z = 0f;
        pTarget.z = 0f;

        Vector3 wStart = (wajanStartPoint != null) ? wajanStartPoint.position : (wTarget + wajanStartOffset);
        wStart.z = 0f;
        wajan.position = wStart;

        // Piring tetap stasioner di meja jika keepPlateStationary bernilai true
        Vector3 pStart = keepPlateStationary ? pTarget : ((piringStartPoint != null) ? piringStartPoint.position : (pTarget + piringStartOffset));
        pStart.z = 0f;
        piring.position = pStart;

        float elapsed = 0f;
        while (elapsed < entryDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / entryDuration);

            Vector3 curW = Vector3.Lerp(wStart, wTarget, t);
            curW.z = 0f;
            wajan.position = curW;

            if (!keepPlateStationary)
            {
                Vector3 curP = Vector3.Lerp(pStart, pTarget, t);
                curP.z = 0f;
                piring.position = curP;
            }

            yield return null;
        }

        wajan.position = wTarget;
        piring.position = pTarget;

        onComplete?.Invoke();
    }

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        if (sfxAudioSource == null)
        {
            sfxAudioSource = GetComponent<AudioSource>();
            if (sfxAudioSource == null) sfxAudioSource = gameObject.AddComponent<AudioSource>();
            SetupAudioMixerGroup(sfxAudioSource);
        }
        sfxAudioSource.PlayOneShot(clip);
    }

    private void SetupAudioMixerGroup(AudioSource source)
    {
        if (AudioManager.instance != null && AudioManager.instance.audioMixer != null)
        {
            UnityEngine.Audio.AudioMixerGroup[] groups = AudioManager.instance.audioMixer.FindMatchingGroups("SFX");
            if (groups != null && groups.Length > 0)
            {
                source.outputAudioMixerGroup = groups[0];
            }
        }
    }

    public void PlayPostDropAndExitAnimation(Action onComplete)
    {
        StartCoroutine(PostDropSequence(onComplete));
    }

    // Efek jatuh makanan, swap sprite piring & wajan, lalu animasi zoom keluar
    private IEnumerator PostDropSequence(Action onComplete)
    {
        if (dropEffect != null) dropEffect.Play();
        if (dropToPlateSfx != null) PlaySFX(dropToPlateSfx);

        if (objectsToHideOnDrop != null)
        {
            foreach (var obj in objectsToHideOnDrop)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        if (objectsToShowOnDrop != null)
        {
            foreach (var obj in objectsToShowOnDrop)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        yield return new WaitForSeconds(postDropDelay);

        yield return StartCoroutine(ExitSequence());

        // Memunculkan UI akhir dengan efek bounce ala NewIngredientPopup
        if (finalUIObjects != null && finalUIObjects.Length > 0)
        {
            if (finalScreenSfx != null) PlaySFX(finalScreenSfx);

            foreach (var obj in finalUIObjects)
            {
                if (obj != null)
                {
                    StartCoroutine(BounceInObject(obj.transform));
                    if (uiStaggerDelay > 0f) yield return new WaitForSeconds(uiStaggerDelay);
                }
            }
        }

        if (nextButton != null)
        {
            StartCoroutine(BounceInObject(nextButton.transform));
        }

        onComplete?.Invoke();
    }

    // Wajan meluncur keluar layar dan piring dizoom ke tengah untuk penyajian
    private IEnumerator ExitSequence()
    {
        float elapsed = 0f;
        Vector3 wajanExitStart = wajan != null ? wajan.position : Vector3.zero;
        Vector3 wajanExitTarget = wajanExitStart + wajanExitOffset;
        wajanExitTarget.z = 0f;

        Vector3 piringStartScale = piring != null ? piring.localScale : Vector3.one;
        Vector3 piringStartPos = piring != null ? piring.position : Vector3.zero;
        
        Vector3 piringTargetPos = plateCenterPoint != null ? plateCenterPoint.position : piringStartPos;
        piringTargetPos.z = 0f;

        Vector3 alasTargetPos = finalAlasTargetPos != null ? finalAlasTargetPos.position : (finalAlasObject != null ? finalAlasObject.position : Vector3.zero);
        alasTargetPos.z = 0f;
        Vector3 alasStartPos = alasTargetPos + finalAlasStartOffset;
        alasStartPos.z = 0f;
        
        if (finalAlasObject != null)
        {
            finalAlasObject.position = alasStartPos;
            finalAlasObject.gameObject.SetActive(true);
        }

        while (elapsed < exitDuration || elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;

            if (elapsed < exitDuration && wajan != null)
            {
                float tWajan = Mathf.SmoothStep(0, 1, elapsed / exitDuration);
                Vector3 wPos = Vector3.Lerp(wajanExitStart, wajanExitTarget, tWajan);
                wPos.z = 0f;
                wajan.position = wPos;
            }

            if (elapsed < zoomDuration)
            {
                float tZoom = Mathf.SmoothStep(0, 1, elapsed / zoomDuration);
                
                if (piring != null)
                {
                    piring.localScale = Vector3.Lerp(piringStartScale, plateTargetScale, tZoom);
                    Vector3 pPos = Vector3.Lerp(piringStartPos, piringTargetPos, tZoom);
                    pPos.z = 0f;
                    piring.position = pPos;
                }

                if (finalAlasObject != null)
                {
                    Vector3 aPos = Vector3.Lerp(alasStartPos, alasTargetPos, tZoom);
                    aPos.z = 0f;
                    finalAlasObject.position = aPos;
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

    // Animasi membal masuk ala NewIngredientPopup
    private IEnumerator BounceInObject(Transform target)
    {
        target.gameObject.SetActive(true);
        Vector3 targetScale = Vector3.one;
        if (initialScales.TryGetValue(target, out Vector3 cached))
        {
            targetScale = cached;
        }
        else if (target.localScale != Vector3.zero)
        {
            targetScale = target.localScale;
        }

        target.localScale = Vector3.zero;

        float timer = 0f;
        float duration = uiBounceDuration > 0f ? uiBounceDuration : 0.45f;
        float[] scales = { 0f, 1.2f, 0.85f, 1.08f, 0.96f, 1f };
        float[] points = { 0f, 0.35f, 0.55f, 0.72f, 0.86f, 1f };

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            float scaleMultiplier = 1f;

            for (int i = 0; i < points.Length - 1; i++)
            {
                if (progress >= points[i] && progress <= points[i + 1])
                {
                    float segProgress = Mathf.InverseLerp(points[i], points[i + 1], progress);
                    scaleMultiplier = Mathf.Lerp(scales[i], scales[i + 1], segProgress);
                    break;
                }
            }

            target.localScale = targetScale * scaleMultiplier;
            yield return null;
        }

        target.localScale = targetScale;
    }
}
