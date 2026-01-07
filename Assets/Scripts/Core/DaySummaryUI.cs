using UnityEngine;
using TMPro;

public class DaySummaryUI : MonoBehaviour
{
    public TMP_Text todayText;

    private void Start()
    {
        if (todayText != null && GameManager.I != null)
        {
            todayText.text =
                $"Day {GameManager.I.currentDay} Complete!\nToday's Total: {GameManager.I.dayCarbon:0.0} kg CO₂";
        }
        else
        {
            Debug.LogWarning("todayText or GameManager.I is NULL in DaySummary scene!");
        }
    }

    public void OnContinue()
    {
        Debug.Log("DaySummaryUI.OnContinue clicked");
        if (GameManager.I != null)
            GameManager.I.NextDay();
        else
            Debug.LogWarning("GameManager.I is NULL in DaySummary scene!");
    }
}
