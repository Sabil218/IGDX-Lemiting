using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAspectController : MonoBehaviour
{
    [SerializeField] private float referenceAspect = 16f / 9f;
    [SerializeField] private float referenceOrthoSize = 5f;

    private Camera cam;
    private float lastAspect;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        lastAspect = GetAspectRatio();

        UpdateCamera();
    }

    private void Update()
    {
        float currentAspect = GetAspectRatio();

        if (!Mathf.Approximately(currentAspect, lastAspect))
        {
            lastAspect = currentAspect;
            UpdateCamera();
        }
    }

    private float GetAspectRatio()
    {
        return (float)Screen.width / Screen.height;
    }

    private void UpdateCamera()
    {
        if (!cam.orthographic)
            return;

        float currentAspect = GetAspectRatio();

        cam.orthographicSize =
            referenceOrthoSize * (referenceAspect / currentAspect);
    }
}