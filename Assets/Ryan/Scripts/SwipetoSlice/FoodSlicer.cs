using System.Collections.Generic;
using UnityEngine;

public class FoodSlicer : MonoBehaviour
{
    [System.Serializable]
    public class GuideLine
    {
        public float xPosition;
        public bool isCut;
        public GameObject lineVisual;
    }

    [Header("Referensi")]
    [SerializeField] private SwipeInput swipeInput;
    [SerializeField] private GameObject fullMeatObject;
    [SerializeField] private GameObject slicedObject;
    [SerializeField] private Collider2D foodArea; // Menggantikan variabel Y yang statis

    [Header("Pengaturan Potongan")]
    [SerializeField] private List<GuideLine> guideLines = new();
    [SerializeField] private float cutTolerance = 0.3f;

    //Mencegah memory leak
    private void OnEnable()
    {
        if (swipeInput != null)
            swipeInput.OnSwipeCompleted += HandleSwipeCompleted;
    }

    private void OnDisable()
    {
        if (swipeInput != null)
            swipeInput.OnSwipeCompleted -= HandleSwipeCompleted;
    }

    private void HandleSwipeCompleted(List<Vector2> swipePath)
    {
        foreach (var line in guideLines)
        {
            if (!line.isCut && SwipeCrossesLine(swipePath, line))
                CutLine(line);
        }

        if (AllLinesAreCut())
            RevealFullySlicedFood();
    }

    private bool SwipeCrossesLine(List<Vector2> swipePath, GuideLine line)
    {
        foreach (var point in swipePath)
        {
            // Validasi: Apakah titik usapan dekat dengan garis DAN berada di dalam area makanan?
            if (IsCloseToLine(point, line) && foodArea.OverlapPoint(point))
                return true;
        }
        return false;
    }

    private bool IsCloseToLine(Vector2 point, GuideLine line)
    {
        return Mathf.Abs(point.x - line.xPosition) <= cutTolerance;
    }

    private void CutLine(GuideLine line)
    {
        line.isCut = true;
        if (line.lineVisual != null)
            line.lineVisual.SetActive(false);
    }

    private bool AllLinesAreCut()
    {
        if (guideLines.Count == 0) return false;

        foreach (var line in guideLines)
        {
            if (!line.isCut) return false;
        }
        return true;
    }

    private void RevealFullySlicedFood()
    {
        fullMeatObject.SetActive(false);
        slicedObject.SetActive(true);
    }
}