using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class UIManager : MonoBehaviour
{
    [Header("Top Bar")]
    public TMP_Text dayText;
    public TMP_Text timeOfDayText;   // 👈 new
    public Slider carbonMeter;
    public TMP_Text carbonValueText;

    [Header("Decision Panel")]
    public GameObject decisionPanel;
    public TMP_Text decisionTitleText;
    public TMP_Text scenarioText;
    public Button[] choiceButtons;
    public TMP_Text[] choiceLabels;

    [Header("Feedback")]
    public GameObject feedbackPanel;
    public TMP_Text feedbackText;
    public Image feedbackIcon;
    public float feedbackDuration = 1.2f;

    private Decision currentDecision;

    // 1) Add at top of class:
    [Header("Info")]
    public Button infoButton;
    public GameObject factsPanel;
    public CanvasGroup factsGroup;   // optional if you added it
    public TMP_Text factsText;
    public CarbonFactsData factsData;

    private void Start()
    {
        // When a new UIManager spawns (e.g., when GamePlay scene loads),
        // register it with the persistent GameManager and refresh the top bar.
        if (GameManager.I != null)
        {
            GameManager.I.ui = this;
            GameManager.I.UpdateHUD();
            UpdateTimeOfDay(GameManager.I.currentTimeOfDay);
        }
    }

    public void UpdateTopBar(int day, int maxDay, float totalCarbon)
    {
        dayText.text = $"Day {day}/{maxDay}";
        carbonValueText.text = $"{totalCarbon:0.0} kg CO₂";
        
        carbonMeter.minValue = 0;
        carbonMeter.maxValue = 400f; // adjust scale later if needed
        carbonMeter.value = Mathf.Clamp(totalCarbon, 0f, 400f);
    }

    public void ShowDecision(Decision decision, string fromObject)
    {
        currentDecision = decision;
        decisionPanel.SetActive(true);
        decisionTitleText.text = fromObject;
        scenarioText.text = decision.scenarioText;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (i < decision.choices.Length)
            {
                choiceButtons[i].gameObject.SetActive(true);
                choiceLabels[i].text = decision.choices[i].optionText;
                int idx = i;
                choiceButtons[i].onClick.RemoveAllListeners();
                choiceButtons[i].onClick.AddListener(() => OnPickChoice(idx));
            }
            else
            {
                choiceButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnPickChoice(int idx)
    {
        if (currentDecision == null) return;
        var choice = currentDecision.choices[idx];
        decisionPanel.SetActive(false);
        GameManager.I.ApplyChoice(currentDecision, choice);
    }

    public void ShowFeedback(string text, Sprite icon)
    {
        StopAllCoroutines();
        feedbackText.text = text;
        feedbackIcon.sprite = icon;
        feedbackIcon.enabled = icon != null;
        feedbackPanel.SetActive(true);
        StartCoroutine(AutoHideFeedback());
    }

    // Quick demo facts (replace with your own / ScriptableObject)
    private readonly string[] carbonFacts = new string[] {
        "🚲 Cycling produces near-zero direct emissions for short trips.",
        "🍔 Beef typically has a much higher footprint than plant-based meals.",
        "🚌 Public transport can cut per-person CO₂ by >50% vs. solo driving.",
        "❄️ Cooling for a few hours vs. all day can halve daily energy emissions.",
        "♻️ Recycling and composting reduce waste-related CO₂ significantly.",
        "💡 LEDs use ~75% less energy than incandescent bulbs."
    };

    public void UpdateTimeOfDay(TimeOfDay t)
    {
        if (timeOfDayText == null) return;

        // Optional: nicer names instead of raw enum.ToString()
        switch (t)
        {
            case TimeOfDay.Morning:
                timeOfDayText.text = "Morning";
                break;
            case TimeOfDay.Midday:
                timeOfDayText.text = "Midday";
                break;
            case TimeOfDay.Evening:
                timeOfDayText.text = "Evening";
                break;
            case TimeOfDay.Night:
                timeOfDayText.text = "Night";
                break;
        }
    }

    private System.Collections.IEnumerator AutoHideFeedback()
    {
        yield return new WaitForSeconds(feedbackDuration);
        feedbackPanel.SetActive(false);
    }

    // 2) Add these methods:
    public void ToggleFactsPanel(bool show)
    {
        if (factsPanel == null) return;

        if (show)
        {
            if (factsText != null && factsData != null && factsData.facts != null)
            {
                // bullet list
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (var f in factsData.facts)
                    if (!string.IsNullOrWhiteSpace(f)) sb.AppendLine("• " + f.Trim());
                factsText.text = sb.ToString();
            }
            factsPanel.SetActive(true);
        }
        else
        {
            factsPanel.SetActive(false);
        }
    }

    public void OnClickInfo()
    {
        if (factsPanel == null) return;
        ToggleFactsPanel(!factsPanel.activeSelf);
    }

    public void ShowFacts(bool show)
    {
        if (factsPanel == null) return;

        if (show)
        {
            // Build a friendly bullet list
            var sb = new StringBuilder();
            foreach (var f in carbonFacts) sb.AppendLine("• " + f);
            if (factsText != null) factsText.text = sb.ToString();
        }

        factsPanel.SetActive(show);

        if (factsGroup != null)
        {
            factsGroup.alpha = show ? 1f : 0f;
            factsGroup.blocksRaycasts = show;
            factsGroup.interactable = show;
        }
    }
}
