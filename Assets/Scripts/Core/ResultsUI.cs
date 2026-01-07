using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;


public class ResultsUI : MonoBehaviour
{
    [Header("Core Results")]
    public TMP_Text totalText;
    public TMP_Text compareText;

    [Header("Left: Breakdown by Category")]
    public Transform breakdownContent;        // Panel_LeftBreakdown/Content
    public GameObject breakdownRowPrefab;     // the BreakdownRow prefab
    public Sprite foodIcon, transportIcon, energyIcon, shoppingIcon, wasteIcon, otherIcon;

    [Header("Right Column: Achievements")]
    public Transform achievementsContent;     // ScrollView/Viewport/Content
    public GameObject achievementItemPrefab;  // Prefab for each achievement item
    public TMP_Text achievementsText;         // Fallback for simple text list

    public Sprite defaultIcon, bikeIcon, leafIcon, recycleIcon, heartIcon;
    public Sprite performanceIcon, challengeIcon;

    [Header("Tips")]
    public GameObject tipsPanel;
    public TMP_Text tipsBody;

    [Header("Export")]
    public TMP_Text exportHint;

    [Header("End State")]
    public TMP_Text endMessageText;

    private readonly string[] tips = new string[] {
        "Use public transport or carpool for commute.",
        "Choose plant-forward meals more often.",
        "Set AC moderately and limit run time.",
        "Switch to LEDs; unplug idle devices.",
        "Buy durable/second-hand instead of fast fashion.",
        "Sort waste, recycle, and compost organics."
    };

    private void Start()
    {
        UpdateResultsText();
        
        // Numbers
        if (GameManager.I != null)
        {
            float total = GameManager.I.totalCarbon;
            if (totalText) totalText.text = $"Total: {total:0.0} kg CO₂";
            float pct = (GameManager.I.nationalAverage > 0f) ? (total / GameManager.I.nationalAverage * 100f) : 0f;
            if (compareText) compareText.text = $"You are {pct:0}% of national average ({GameManager.I.nationalAverage:0} kg)";
        }

        if (endMessageText != null && GameManager.I != null)
        {
            if (GameManager.I.deathByNeglect)
            {
                endMessageText.text =
                    "You kept your carbon low by skipping meals and working nonstop.\n" +
                    "In real life, this would seriously harm your health.\n" +
                    "Sustainable living must balance the planet and your body.";
            }
            else
            {
                endMessageText.text =
                    "Simulation complete! This is your 30-day carbon footprint and lifestyle summary.";
            }
        }

        BuildAchievementsList();
        BuildBreakdownList(); // ← NEW: build left panel
    }

    // ---------- Results numbers ----------
    private void UpdateResultsText()
    {
        if (GameManager.I == null) return;

        float total = GameManager.I.totalCarbon;
        if (totalText) totalText.text = $"Total: {total:0.0} kg CO₂";

        float pct = (GameManager.I.nationalAverage > 0f)
            ? (total / GameManager.I.nationalAverage * 100f)
            : 0f;
        if (compareText) compareText.text =
            $"You are {pct:0}% of national average ({GameManager.I.nationalAverage:0} kg)";
    }

    // ================== BREAKDOWN (LEFT PANEL) ==================
    private void BuildBreakdownList()
    {
        // if (breakdownContent == null || breakdownRowPrefab == null) return;
        if (!breakdownContent || !breakdownRowPrefab) return;

        // clear previous rows
        for (int i = breakdownContent.childCount - 1; i >= 0; i--)
            Destroy(breakdownContent.GetChild(i).gameObject);

        // var totals = (DataManager.I != null) ? DataManager.I.current.totalsByCategory : null;

        if (DataManager.I == null || DataManager.I.current == null)
        {
            // show empty set just so panel isn't blank
            SpawnRow("Food", 0f, 0f);
            SpawnRow("Transport", 0f, 0f);
            SpawnRow("Energy", 0f, 0f);
            SpawnRow("Shopping", 0f, 0f);
            SpawnRow("Waste", 0f, 0f);
            return;
        }

        Dictionary<string, float> totals = DataManager.I.current.totalsByCategory;

        // if there is no data yet, still show basic categories
        if (totals == null || totals.Count == 0)
        {
            SpawnRow("Food", 0f, 0f);
            SpawnRow("Transport", 0f, 0f);
            SpawnRow("Energy", 0f, 0f);
            SpawnRow("Shopping", 0f, 0f);
            SpawnRow("Waste", 0f, 0f);
            return;
        }

        // desired order of main categories
        string[] order = { "Food", "Transport", "Energy", "Shopping", "Waste" };
        var ordered = new List<(string cat, float kg)>();

        // foreach (var cat in order)
        //     if (totals.TryGetValue(cat, out var v))
        //         ordered.Add((cat, v));

        // // add any extra categories at the end, ordered by magnitude
        // foreach (var kv in totals.Where(kv => !order.Contains(kv.Key))
        //                          .OrderByDescending(kv => Mathf.Abs(kv.Value)))
        //     ordered.Add((kv.Key, kv.Value));

        if (totals != null && totals.Count > 0)
        {
            foreach (var cat in order)
                if (totals.TryGetValue(cat, out var v)) ordered.Add((cat, v));

            foreach (var kv in totals.Where(kv => !order.Contains(kv.Key))
                                     .OrderByDescending(kv => Mathf.Abs(kv.Value)))
                ordered.Add((kv.Key, kv.Value));
        }
        else
        {
            foreach (var cat in order) ordered.Add((cat, 0f));
        }

        // for bar length normalization
        float maxAbs = Mathf.Max(0.01f,
            ordered.Select(x => Mathf.Abs(x.kg)).DefaultIfEmpty(0f).Max());

        foreach (var (cat, kg) in ordered)
        {
            // float norm = Mathf.Clamp01(Mathf.Abs(kg) / maxAbs);
            // SpawnRow(cat, kg, norm);
            float norm = Mathf.Clamp01(Mathf.Abs(kg) / maxAbs);
            var go = Instantiate(breakdownRowPrefab, breakdownContent);
            var ui = go.GetComponent<BreakdownRowUI>();
            string val = $"{kg:+0.0;-0.0;0} kg";
            ui.Set(GetCatIcon(cat), cat, val, norm);
        }
    }

    private void SpawnRow(string category, float kg, float normalized)
    {
        GameObject go = Instantiate(breakdownRowPrefab, breakdownContent);
        BreakdownRowUI ui = go.GetComponent<BreakdownRowUI>();
        if (ui == null) return;

        string valueText = $"{kg:+0.0;-0.0;0} kg"; // +4.2 kg / -2.5 kg / 0 kg
        ui.Set(GetCatIcon(category), category, valueText, normalized);
    }

    private Sprite GetCatIcon(string category)
    {
        switch (category)
        {
            case "Food":      return foodIcon     ? foodIcon     : otherIcon;
            case "Transport": return transportIcon ? transportIcon: otherIcon;
            case "Energy":    return energyIcon   ? energyIcon   : otherIcon;
            case "Shopping":  return shoppingIcon ? shoppingIcon : otherIcon;
            case "Waste":     return wasteIcon    ? wasteIcon    : otherIcon;
            default:          return otherIcon;
        }
    }

    // ================== ACHIEVEMENTS (RIGHT PANEL) ==================
    private void BuildAchievementsList()
    {
        if (DataManager.I == null) return;

        var list = DataManager.I.current.achievements;

        // If using scroll view with prefabs
        if (achievementsContent != null && achievementItemPrefab != null)
        {
            if (!achievementsContent || !achievementItemPrefab) return;

            // Clear old items
            for (int i = achievementsContent.childCount - 1; i >= 0; i--)
                Destroy(achievementsContent.GetChild(i).gameObject);
            /*var */ list = (DataManager.I != null) ? DataManager.I.current.achievements : null;

            if (list == null || list.Count == 0)
            {
                var go = Instantiate(achievementItemPrefab, achievementsContent);
                var ui = go.GetComponent<AchievementItemUI>();
                ui.Set(defaultIcon, "No achievements unlocked yet");
                return;
            }
            else
            {
                foreach (var name in list)
                {
                    var go = Instantiate(achievementItemPrefab, achievementsContent);
                    var ui = go.GetComponent<AchievementItemUI>();
                    ui.Set(ChooseIcon(name), name);
                }
            }
        }

        // Fallback: simple TMP text
        if (achievementsText != null)
        {
            achievementsText.text = (list == null || list.Count == 0)
                ? "—"
                : "• " + string.Join("\n• ", list);
        }
    }

    private Sprite ChooseIcon(string name)
    {
        string n = name.ToLower();
        if (n.Contains("commuter") || n.Contains("bike")) return bikeIcon ? bikeIcon : defaultIcon;
        if (n.Contains("plant"))                           return leafIcon ? leafIcon : defaultIcon;
        if (n.Contains("recycl"))                          return recycleIcon ? recycleIcon : defaultIcon;
        if (n.Contains("carbon") || n.Contains("conscious")) return heartIcon ? heartIcon : defaultIcon;
        return defaultIcon;
    }

    // ================== TIPS ==================
    public void ToggleTips()
    {
        if (!tipsPanel) return;
        if (!tipsPanel.activeSelf && tipsBody != null)
            tipsBody.text = "• " + string.Join("\n• ", tips);
        tipsPanel.SetActive(!tipsPanel.activeSelf);
    }

    public void OnExportJson() { var p = DataManager.I.SaveJson(); if (exportHint) exportHint.text = "Saved JSON → " + p; }
    public void OnExportCsv() { var p = DataManager.I.SaveCsv(); if (exportHint) exportHint.text = "Saved CSV → " + p; }

    public void OnPlayAgain()  { GameManager.I.ResetRun(); }
    
}