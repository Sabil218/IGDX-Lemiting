using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StirManager : MonoBehaviour, ICookingPhase
{
    [Header("Input Hook")]
    [SerializeField] private StirInput stirInput;

    [Header("Scene")]
    [SerializeField] private Transform spoonTransform;
    [SerializeField] private GameObject fireObject;

    [Header("Pan Basin Bounds (Green Ellipse)")]
    [Tooltip("Radius horizontal batas area hijau wajan.")]
    [SerializeField] private float spreadRadiusX = 2.0f;
    [Tooltip("Radius vertikal atas batas area hijau wajan.")]
    [SerializeField] private float spreadRadiusYUp = 0.65f;
    [Tooltip("Radius vertikal bawah batas area hijau wajan.")]
    [SerializeField] private float spreadRadiusYDown = 0.40f;
    [Header("Shuffle & Convergence Dynamics (Bergerak ke Arah Satu Sama Lain)")]
    [Tooltip("Kekuatan dorongan adukan saat mengaduk.")]
    [SerializeField] private float stirPushStrength = 2.2f;
    [Tooltip("Daya pegas pengembali bahan ke posisi asalnya saat adukan berhenti (A -> B -> A).")]
    [SerializeField] private float springStrength = 2.4f;
    [Tooltip("Gesekan kuah kental untuk meredam gerakan agar halus dan tidak bergetar.")]
    [SerializeField] private float fluidDamping = 4.2f;

    [Header("Displacement Limits & Exclusion")]
    [Tooltip("Batas maksimal pergeseran rempah-rempah agar leluasa bergerak saling mendekat di wajan.")]
    [SerializeField] private float maxSpiceDisplacement = 1.35f;
    [Tooltip("Batas maksimal goyangan ayam di tengah agar tetap bergoyang lembut tanpa keluar jalur.")]
    [SerializeField] private float maxChickenDisplacement = 0.12f;
    [Tooltip("Radius perlindungan badan ayam agar rempah-rempah mengalir di sekelilingnya, tidak menumpuk di atas ayam.")]
    [SerializeField] private float chickenExclusionRadius = 0.38f;

    [Header("Visuals to Toggle")]
    [SerializeField] private GameObject[] stirVisuals;

    [Header("Phase Specific Visuals")]
    [SerializeField] private LayeredCookingStage[] phaseCookingStages;

    [Header("Animation")]
    [SerializeField] private Animator spoonAnimator;
    private float stirTimeout = 0.15f;
    private float lastStirTime = -1f;

    [Header("Progress")]
    [SerializeField] private Image progressBarFill;
    [SerializeField] private RectTransform progressIcon;
    [SerializeField] private RectTransform progressStartPoint;
    [SerializeField] private RectTransform progressEndPoint;
    [SerializeField] private float completionDelay = 1.0f;

    [Header("Camera Transition")]
    public GameObject stirVirtualCamera;

    [Header("Audio")]
    [SerializeField] private AudioClip stirSfx;
    [SerializeField] private AudioSource sfxAudioSource;

    // Sloshing & Convergence State
    private Vector2 currentStirImpulse = Vector2.zero; // Vektor dorongan adukan kuah
    private float currentAngularVelocity = 0f;         // Kecepatan pusaran melingkar kuah
    private bool initializedItems = false;
    private readonly List<StirFoodItem> stirItems = new List<StirFoodItem>();

    private class StirFoodItem
    {
        public Transform transform;
        public Vector2 homePos;           // Posisi awal mendarat di wajan (A)
        public Vector2 currentPos;        // Posisi dinamis saat diaduk kuah (B)
        public Vector2 velocity;          // Kecepatan gerak
        public float mass;                // Bahan Utama = 2.8f, Rempah = 1.0f
        public bool isMainItem;           // Penanda bahan utama (ayam / ikan / dsb)
        public float maxDisplacement;     // Batas maksimal jangkauan gerak dari homePos
        public float baseRotationZ;       // Rotasi awal
    }

    private Vector3 GetTruePanCenter()
    {
        if (CookingVisualController.Instance != null && CookingVisualController.Instance.rawIngredientsContainer != null)
        {
            return CookingVisualController.Instance.rawIngredientsContainer.position;
        }
        if (CookingVisualController.Instance != null && CookingVisualController.Instance.foodContentRenderer != null)
        {
            return CookingVisualController.Instance.foodContentRenderer.bounds.center;
        }
        else if (CookingVisualController.Instance != null)
        {
            return CookingVisualController.Instance.transform.position;
        }
        return transform.position;
    }

    private void Awake()
    {
        SetVisualsActive(false);
        StopStirAudio();
    }

    private void Start()
    {
    }

    private GameObject GetFireObject()
    {
        if (fireObject != null) return fireObject;

        GameObject stove = GameObject.Find("StoveObject");
        if (stove != null)
        {
            Transform fireTrans = stove.transform.Find("Fire");
            if (fireTrans != null)
            {
                fireObject = fireTrans.gameObject;
                return fireObject;
            }
        }

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject go in allObjects)
        {
            if (go != null && go.name == "Fire" && go.scene.isLoaded)
            {
                if (go.transform.parent != null && go.transform.parent.name.Contains("Stove"))
                {
                    fireObject = go;
                    return fireObject;
                }
            }
        }

        return null;
    }

    // Starts the stirring minigame phase and resets all visuals/positions.
    public void StartPhase()
    {
        if (stirVirtualCamera != null)
        {
            stirVirtualCamera.SetActive(true);
        }

        currentStirImpulse = Vector2.zero;
        currentAngularVelocity = 0f;
        stirItems.Clear();
        initializedItems = false;
        SetVisualsActive(true);

        EnsureItemsInitialized();
    }

    public void SetVisualsActive(bool isActive)
    {
        if (isActive)
        {
            CookingManager.SetSpoonActive(true);
            StartStirAudio();
        }
        else
        {
            HideSpoonVisuals();
            StopStirAudio();
        }

        GameObject fire = GetFireObject();
        if (fire != null && fire.activeSelf != isActive) fire.SetActive(isActive);

        if (stirVisuals != null)
        {
            foreach (GameObject visual in stirVisuals)
            {
                if (visual != null) visual.SetActive(isActive);
            }
        }

        if (stirInput != null && stirInput.StirZoneObject != null && !stirInput.StirZoneObject.CompareTag("Wajan"))
        {
            stirInput.StirZoneObject.SetActive(isActive);
        }

        if (isActive && CookingVisualController.Instance != null && phaseCookingStages != null && phaseCookingStages.Length > 0)
        {
            CookingVisualController.Instance.SetLayeredTransitionStages(phaseCookingStages);
        }
    }

    // Toggles the stirring spoon animation and drives bounded slosh motion.
    private void Update()
    {
        // 1. Update animasi sendok
        if (spoonAnimator != null)
        {
            bool isActivelyStirring = Time.time - lastStirTime < stirTimeout;
            spoonAnimator.SetBool("isStirring", isActivelyStirring);
        }

        // 2. Jalankan simulasi fisika bahan bergoyang & berputar di dalam kuah
        UpdateSloshPhysics(Time.deltaTime);

        // 3. Redakan momentum pusaran & dorongan kuah secara bertahap saat tidak diaduk (inertia cairan)
        currentAngularVelocity = Mathf.MoveTowards(currentAngularVelocity, 0f, 3.5f * Time.deltaTime);
        currentStirImpulse = Vector2.MoveTowards(currentStirImpulse, Vector2.zero, 3.5f * Time.deltaTime);
    }

    private void OnEnable()
    {
        if (stirInput != null)
        {
            stirInput.OnStirred += HandleStirred;
            stirInput.OnStirProgress += HandleStirProgress;
            stirInput.OnStirCompleted += HandleStirCompleted;
        }
        
        if (progressBarFill != null) progressBarFill.fillAmount = 0f;
        if (progressIcon != null && progressStartPoint != null) progressIcon.position = progressStartPoint.position;
    }

    private void OnDisable()
    {
        if (stirInput != null)
        {
            stirInput.OnStirred -= HandleStirred;
            stirInput.OnStirProgress -= HandleStirProgress;
            stirInput.OnStirCompleted -= HandleStirCompleted;
        }
        StopStirAudio();
    }

    // Menerima input adukan dari pemain dan menentukan arah dorongan gelombang kuah
    private void HandleStirred(float angleDelta)
    {
        lastStirTime = Time.time;
        EnsureItemsInitialized();

        Vector2 pushDir = (stirInput != null) ? stirInput.LastPushDirection : Vector2.right;
        if (pushDir.sqrMagnitude < 0.001f)
        {
            pushDir = Vector2.right * Mathf.Sign(angleDelta);
        }

        float effectivePush = Mathf.Max(stirPushStrength, 1.6f);

        // 1. Dorongan pusaran kuah melingkar (swirl vortex)
        float angularKick = Mathf.Sign(angleDelta) * Mathf.Max(Mathf.Abs(angleDelta) * 0.45f, 1.2f) * effectivePush;
        currentAngularVelocity = Mathf.Clamp(currentAngularVelocity + angularKick, -12f, 12f);

        // 2. Dorongan translasi kuah
        float pushMag = Mathf.Clamp(Mathf.Abs(angleDelta) * 0.35f * effectivePush, 0.4f, 3.5f);
        currentStirImpulse = Vector2.ClampMagnitude(currentStirImpulse + pushDir * pushMag, 4.0f);
    }

    private void StartStirAudio()
    {
        if (stirSfx == null) return;
        EnsureAudioSource();
        if (sfxAudioSource != null)
        {
            sfxAudioSource.clip = stirSfx;
            sfxAudioSource.loop = true;
            if (!sfxAudioSource.isPlaying)
            {
                sfxAudioSource.Play();
            }
        }
    }

    private void StopStirAudio()
    {
        if (sfxAudioSource != null && sfxAudioSource.isPlaying)
        {
            sfxAudioSource.Stop();
        }
    }

    private void EnsureAudioSource()
    {
        if (sfxAudioSource == null)
        {
            sfxAudioSource = GetComponent<AudioSource>();
            if (sfxAudioSource == null) sfxAudioSource = gameObject.AddComponent<AudioSource>();
        }
        sfxAudioSource.playOnAwake = false;
        SetupAudioMixerGroup(sfxAudioSource);
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

    // Mengumpulkan bahan utama dan seluruh rempah di wajan berdasarkan posisi pendaratan aslinya (homePos)
    private void EnsureItemsInitialized()
    {
        if (initializedItems && stirItems.Count > 0) return;

        Transform rawContainer = null;
        if (CookingVisualController.Instance != null && CookingVisualController.Instance.rawIngredientsContainer != null)
        {
            rawContainer = CookingVisualController.Instance.rawIngredientsContainer;
        }

        if (rawContainer == null || rawContainer.childCount == 0)
        {
            GameObject dropAreaGo = GameObject.Find("DropArea");
            if (dropAreaGo != null) rawContainer = dropAreaGo.transform;
        }

        if (rawContainer == null) return;

        stirItems.Clear();

        foreach (Transform rootItem in rawContainer)
        {
            if (rootItem == null || !rootItem.gameObject.activeInHierarchy) continue;

            bool isMain = IsMainItem(rootItem);

            StirFoodItem item = new StirFoodItem
            {
                transform = rootItem,
                homePos = new Vector2(rootItem.position.x, rootItem.position.y),
                currentPos = new Vector2(rootItem.position.x, rootItem.position.y),
                velocity = Vector2.zero,
                mass = isMain ? 2.5f : 1.0f,
                isMainItem = isMain,
                maxDisplacement = isMain ? maxChickenDisplacement : maxSpiceDisplacement,
                baseRotationZ = rootItem.eulerAngles.z
            };
            stirItems.Add(item);
        }

        if (stirItems.Count > 0)
        {
            initializedItems = true;
            Debug.Log($"[StirManager] Initialized {stirItems.Count} items to slosh and stir.");
        }
    }

    private bool IsMainItem(Transform item)
    {
        if (item == null) return false;
        string name = item.name.ToLower();
        return name.Contains("ayam") || name.Contains("chicken") || name.Contains("ikan") || name.Contains("fish") || name.Contains("sagu") || name.Contains("daging");
    }

    // Fisika adukan berbatas: Bahan bergoyang dan berputar lembut di sekitar titik pendaratannya (homePos)
    // mengikuti pusaran kuah tanpa menumpuk di DropArea.
    private void UpdateSloshPhysics(float dt)
    {
        if (stirItems.Count == 0 || dt <= 0.0001f) return;

        Vector2 panCenter = GetTruePanCenter();

        // 1. Terapkan gaya pusaran (vortex), gaya translasi, pegas pengembali (spring), dan gesekan kuah
        for (int i = 0; i < stirItems.Count; i++)
        {
            StirFoodItem item = stirItems[i];
            if (item == null || item.transform == null) continue;

            Vector2 relPos = item.currentPos - panCenter;
            float distToCenter = relPos.magnitude;
            Vector2 tangent = distToCenter > 0.01f ? new Vector2(-relPos.y, relPos.x) / distToCenter : Vector2.right;

            // Pusaran melingkar di sekeliling wajan (vortex)
            Vector2 vortexAccel = tangent * (currentAngularVelocity * (item.isMainItem ? 0.35f : 1.25f));

            // Dorongan translasi dari arah kursor
            Vector2 linearAccel = currentStirImpulse * (item.isMainItem ? 0.3f : 0.85f);

            // Pegas pengembali ke posisi pendaratan awal (homePos)
            Vector2 dispFromHome = item.currentPos - item.homePos;
            Vector2 springAccel = -dispFromHome * (item.isMainItem ? springStrength * 1.4f : springStrength);

            // Gesekan zat cair (fluid damping)
            Vector2 dragAccel = -item.velocity * fluidDamping;

            Vector2 totalAccel = vortexAccel + linearAccel + springAccel + dragAccel;
            item.velocity += totalAccel * dt;
            item.currentPos += item.velocity * dt;

            // Batas maksimal goyangan (Displacement clamp yang cukup leluasa agar terlihat jelas)
            float allowedDisplacement = Mathf.Max(item.maxDisplacement, item.isMainItem ? 0.28f : 0.75f);
            Vector2 curDisp = item.currentPos - item.homePos;
            if (curDisp.sqrMagnitude > allowedDisplacement * allowedDisplacement)
            {
                item.currentPos = item.homePos + curDisp.normalized * allowedDisplacement;
                if (Vector2.Dot(item.velocity, curDisp) > 0f)
                {
                    item.velocity = -item.velocity * 0.25f;
                }
            }
        }

        // 2. Eksklusi Bahan Utama di Tengah: Rempah-rempah meluncur di sisi luarnya agar tidak menumpuk di atas bahan utama
        StirFoodItem mainItem = stirItems.Find(x => x != null && x.isMainItem);
        if (mainItem != null)
        {
            float exclusionRad = Mathf.Max(chickenExclusionRadius, 0.35f);
            for (int i = 0; i < stirItems.Count; i++)
            {
                StirFoodItem spice = stirItems[i];
                if (spice == null || spice.isMainItem) continue;

                Vector2 delta = spice.currentPos - mainItem.currentPos;
                float distSq = delta.sqrMagnitude;
                if (distSq < exclusionRad * exclusionRad && distSq > 0.0001f)
                {
                    float dist = Mathf.Sqrt(distSq);
                    Vector2 pushAway = delta / dist;
                    spice.currentPos = mainItem.currentPos + pushAway * exclusionRad;

                    float inwardVel = Vector2.Dot(spice.velocity, -pushAway);
                    if (inwardVel > 0f)
                    {
                        spice.velocity += pushAway * inwardVel * 1.2f;
                    }
                }
            }
        }

        // 3. Spasi Antar Bahan (mencegah saling tumpuk saat sloshing)
        for (int i = 0; i < stirItems.Count; i++)
        {
            StirFoodItem a = stirItems[i];
            if (a == null || a.isMainItem) continue;

            for (int j = i + 1; j < stirItems.Count; j++)
            {
                StirFoodItem b = stirItems[j];
                if (b == null || b.isMainItem) continue;

                Vector2 delta = a.currentPos - b.currentPos;
                float distSq = delta.sqrMagnitude;
                float minDist = 0.25f;

                if (distSq < minDist * minDist && distSq > 0.0001f)
                {
                    float dist = Mathf.Sqrt(distSq);
                    Vector2 sepDir = delta / dist;
                    float overlap = (minDist - dist) * 0.5f;

                    a.currentPos += sepDir * overlap * 0.5f;
                    b.currentPos -= sepDir * overlap * 0.5f;

                    Vector2 relVel = a.velocity - b.velocity;
                    Vector2 contactDamp = relVel * 0.25f;
                    a.velocity -= contactDamp;
                    b.velocity += contactDamp;
                }
            }
        }

        // 4. Sinkronkan posisi akhir dan kemiringan (tilt) dinamis ke Transform objek
        bool isActivelyStirring = Mathf.Abs(currentAngularVelocity) > 0.15f || currentStirImpulse.sqrMagnitude > 0.1f;
        for (int i = 0; i < stirItems.Count; i++)
        {
            StirFoodItem item = stirItems[i];
            if (item == null || item.transform == null) continue;

            Transform t = item.transform;
            t.position = new Vector3(item.currentPos.x, item.currentPos.y, t.position.z);

            // Goyangan mengapung & kemiringan mengikuti gelombang adukan kuah
            float waveBob = isActivelyStirring ? Mathf.Sin(Time.time * 7f + i * 1.2f) * (item.isMainItem ? 2.5f : 6.0f) : 0f;
            float tiltZ = Mathf.Clamp(item.velocity.x * -7.0f + waveBob, -14f, 14f);
            t.rotation = Quaternion.Euler(0f, 0f, item.baseRotationZ + tiltZ);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 c = GetTruePanCenter();
        Gizmos.color = Color.green;
        int segments = 48;
        float angle = 0f;

        float startRadiusY = Mathf.Sin(0) >= 0 ? spreadRadiusYUp : spreadRadiusYDown;
        Vector3 lastPoint = c + new Vector3(Mathf.Cos(0) * spreadRadiusX, Mathf.Sin(0) * startRadiusY, 0);

        for (int i = 1; i <= segments; i++)
        {
            angle += (360f / segments) * Mathf.Deg2Rad;
            float currentRadiusY = Mathf.Sin(angle) >= 0 ? spreadRadiusYUp : spreadRadiusYDown;
            Vector3 nextPoint = c + new Vector3(Mathf.Cos(angle) * spreadRadiusX, Mathf.Sin(angle) * currentRadiusY, 0);
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }

        // Titik pusat DropArea / Wajan
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(c, 0.08f);

        // Lingkaran eksklusi ayam di tengah
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(c, chickenExclusionRadius);
    }

    // Updates the UI progress bar and visually fades between raw/cooked states.
    private void HandleStirProgress(float progress)
    {
        if (progressBarFill != null) progressBarFill.fillAmount = progress;
        
        if (progressIcon != null && progressStartPoint != null && progressEndPoint != null)
        {
            progressIcon.position = Vector3.Lerp(progressStartPoint.position, progressEndPoint.position, progress);
        }

        if (CookingVisualController.Instance != null)
            CookingVisualController.Instance.UpdateStirProgress(progress);
    }

    // Called when the stirring circle is fully complete.
    private void HandleStirCompleted()
    {
        if (progressBarFill != null) progressBarFill.fillAmount = 1f;
        
        if (progressIcon != null && progressEndPoint != null)
        {
            progressIcon.position = progressEndPoint.position;
        }

        // Matikan api kompor seketika saat proses aduk selesai (memberikan sensasi langsung selesai masak)
        GameObject fire = GetFireObject();
        if (fire != null) fire.SetActive(false);

        // Sembunyikan sendok pengaduk seketika agar tidak mengganggu visual transisi ke plating
        HideSpoonVisuals();

        // Hentikan suara adukan seketika
        StopStirAudio();

        // Kembalikan posisi item ke homePos secara rapi sebelum transisi plating
        for (int i = 0; i < stirItems.Count; i++)
        {
            if (stirItems[i] != null && stirItems[i].transform != null)
            {
                stirItems[i].transform.position = new Vector3(stirItems[i].homePos.x, stirItems[i].homePos.y, stirItems[i].transform.position.z);
                stirItems[i].transform.rotation = Quaternion.Euler(0f, 0f, stirItems[i].baseRotationZ);
            }
        }

        StartCoroutine(DelayedTransition());
    }

    private void HideSpoonVisuals()
    {
        CookingManager.SetSpoonActive(false);
    }

    // Delays briefly before telling CookingManager to advance to the next step.
    private IEnumerator DelayedTransition()
    {
        yield return new WaitForSeconds(completionDelay);

        SetVisualsActive(false);
        HideSpoonVisuals();

        CookingManager manager = FindObjectOfType<CookingManager>();
        if (manager != null)
        {
            manager.NextStep();
        }
        else
        {
            Debug.LogWarning("[StirManager] CookingManager not found in scene!");
        }
    }
}