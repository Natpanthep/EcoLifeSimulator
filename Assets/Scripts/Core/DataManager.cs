using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

[System.Serializable] public class DayRecord { public int day; public float carbonKg; public Dictionary<string,float> byCategory = new(); }

[System.Serializable]
public class SessionSummary
{
    public List<DayRecord> days = new();
    public float total;
    public Dictionary<string, float> totalsByCategory = new(); // "Food","Transport","Energy","Shopping","Waste"
    public List<string> achievements = new();
}

public class DataManager : MonoBehaviour
{
    public static DataManager I;
    public SessionSummary current = new SessionSummary();

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ResetSession() { current = new SessionSummary(); }

    public void AddCategory(string category, float delta)
    {
        if (string.IsNullOrEmpty(category)) category = "Other";
        if (!current.totalsByCategory.ContainsKey(category))
            current.totalsByCategory[category] = 0f;
        current.totalsByCategory[category] += delta;

        // Also append to today's record (created in PushDaySummary)
        if (current.days.Count > 0)
        {
            var dr = current.days[current.days.Count - 1];
            if (!dr.byCategory.ContainsKey(category)) dr.byCategory[category] = 0f;
            dr.byCategory[category] += delta;
        }
    }

    public void PushDaySummary(int day, float carbonKg)
    {
        current.days.Add(new DayRecord { day = day, carbonKg = carbonKg });
        current.total += carbonKg;
    }

    public void FinalizeRun(float finalTotal) { current.total = finalTotal; }

    public void Unlock(string key)
    {
        if (!current.achievements.Contains(key))
            current.achievements.Add(key);
    }

    public string SaveJson()
    {
        string dir = Application.persistentDataPath;
        string path = Path.Combine(dir, $"EcoLifeSession_{System.DateTime.Now:yyyyMMdd_HHmmss}.json");
        string json = JsonUtility.ToJson(current, prettyPrint: true);
        File.WriteAllText(path, json, Encoding.UTF8);
        Debug.Log("Saved JSON: " + path);
        return path;
    }

    public string SaveCsv()
    {
        string dir = Application.persistentDataPath;
        string path = Path.Combine(dir, $"EcoLifeSession_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv");
        var sb = new StringBuilder();
        sb.AppendLine("Day,TotalDayKg,Category,CategoryKg");

        foreach (var d in current.days)
        {
            if (d.byCategory.Count == 0)
            {
                sb.AppendLine($"{d.day},{d.carbonKg},,");
            }
            else
            {
                foreach (var kv in d.byCategory)
                    sb.AppendLine($"{d.day},{d.carbonKg},{kv.Key},{kv.Value}");
            }
        }
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        Debug.Log("Saved CSV: " + path);
        return path;
    }
}
