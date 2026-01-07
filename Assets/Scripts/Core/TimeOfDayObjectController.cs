using UnityEngine;

public class TimeOfDayObjectController : MonoBehaviour
{
    [Header("Objects visible in each time of day")]
    public GameObject[] morningObjects;
    public GameObject[] middayObjects;
    public GameObject[] eveningObjects;
    public GameObject[] nightObjects;

    private TimeOfDay lastTime = (TimeOfDay)(-1);

    private void Start()
    {
        RefreshVisibility(true);
    }

    private void Update()
    {
        if (GameManager.I == null) return;
        if (GameManager.I.currentTimeOfDay != lastTime)
        {
            lastTime = GameManager.I.currentTimeOfDay;
            RefreshVisibility(false);
        }
    }

    private void RefreshVisibility(bool force)
    {
        TimeOfDay t = (GameManager.I != null) ? GameManager.I.currentTimeOfDay : TimeOfDay.Morning;

        SetArrayActive(morningObjects, t == TimeOfDay.Morning);
        SetArrayActive(middayObjects,   t == TimeOfDay.Midday);
        SetArrayActive(eveningObjects,  t == TimeOfDay.Evening);
        SetArrayActive(nightObjects,    t == TimeOfDay.Night);
    }

    private void SetArrayActive(GameObject[] arr, bool active)
    {
        if (arr == null) return;
        foreach (var go in arr)
        {
            if (go != null) go.SetActive(active);
        }
    }
}
