using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Menu, Playing, DaySummary, Results }

// 👇 Add this enum right here
public enum TimeOfDay { Morning, Midday, Evening, Night }

public class GameManager : MonoBehaviour
{
    public static GameManager I;

    [Header("Progress")]
    public int currentDay = 1;
    public int maxDays = 30;

    [Header("Time of Day")]
    public TimeOfDay currentTimeOfDay = TimeOfDay.Morning;

    [Header("Carbon")]
    public float totalCarbon;
    public float dayCarbon;
    public float nationalAverage = 300f; // tweak for your report

    [Header("Flow")]
    public int decisionsPerDay = 4;
    [HideInInspector] public int decisionsTakenToday = 0;

    [Header("Refs")]
    public UIManager ui;

    [Header("Wellbeing")]
    public int consecutiveStarveDays = 0;
    public bool ateToday = false;
    public bool workedToday = false;
    public bool relaxedToday = false;

    [Header("Travel")]
    public int pendingDaysToSkip = 0;   // extra days to skip on NextDay
    public bool deathByNeglect = false; // used to show special message on Results

    public GameState State { get; private set; } = GameState.Playing;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return; 
        }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (ui == null) ui = FindObjectOfType<UIManager>();
        UpdateHUD();
    }

    public void ApplyChoice(Decision decision, Choice choice)
    {
        // Update carbon and counters
        dayCarbon += choice.carbonImpactKg;
        totalCarbon += choice.carbonImpactKg;
        decisionsTakenToday++;

        // --- NEW: update time-of-day based on decision count this day ---
        switch (decisionsTakenToday)
        {
            case 1:
                currentTimeOfDay = TimeOfDay.Morning;
                break;
            case 2:
                currentTimeOfDay = TimeOfDay.Midday;
                break;
            case 3:
                currentTimeOfDay = TimeOfDay.Evening;
                break;
            case 4:
                currentTimeOfDay = TimeOfDay.Night;
                break;
            default:
                currentTimeOfDay = TimeOfDay.Morning;
                break;
        }

        // Tell UI about new time-of-day
        if (ui != null)
            ui.UpdateTimeOfDay(currentTimeOfDay);
        // --- END NEW BLOCK ---

        // --- NEW: wellbeing tracking for anti-cheat ---
        if (choice.countsAsMeal)
            ateToday = true;        // at least one proper meal today

        if (choice.countsAsWork)
            workedToday = true;     // did some serious work today

        if (choice.countsAsRelax)
            relaxedToday = true;    // did at least one relaxing thing
        
        // Long travel: schedule extra days to skip AFTER this day
        if (choice.daysToSkipAfter > 0)
            pendingDaysToSkip = Mathf.Max(pendingDaysToSkip, choice.daysToSkipAfter);
        // ----------------------------------------------

        // 1) Collect category totals (Food/Transport/Energy/Shopping/Waste)
        if (decision != null)
            DataManager.I.AddCategory(decision.category, choice.carbonImpactKg);

        // 2) Simple, transparent achievements
        if (decision.category == "Transport" && choice.carbonImpactKg <= 0f)
            DataManager.I.Unlock("Eco Commuter");           // chose bike/walk
        if (decision.category == "Food" && choice.carbonImpactKg <= 0.6f)
            DataManager.I.Unlock("Plant-Based Hero");       // veggie/leftovers
        if (decision.category == "Waste" && choice.carbonImpactKg < 0f)
            DataManager.I.Unlock("Recycling Champion");
        if (totalCarbon < 150f && currentDay >= maxDays)
            DataManager.I.Unlock("Carbon Conscious");       // final low total

        if (ui != null) ui.ShowFeedback(choice.feedbackText, decision.icon);
        UpdateHUD();

        if (decisionsTakenToday >= decisionsPerDay) 
            GoDaySummary();
    }

    public void NextDay()
    {
        Debug.Log("GameManager.NextDay() called. currentDay = " + currentDay);

        int daysAdvance = 1 + Mathf.Max(0, pendingDaysToSkip);
        pendingDaysToSkip = 0;

        currentDay++;
        decisionsTakenToday = 0;
        dayCarbon = 0;
        currentTimeOfDay = TimeOfDay.Morning;   // 👈 add this

        if (ui != null)
            ui.UpdateTimeOfDay(currentTimeOfDay);

        if (currentDay > maxDays)
        {
            Debug.Log("Reached maxDays -> GoResults()");
            GoResults();
        }
        else
        {
            Debug.Log("Loading GamePlay scene for new day: " + currentDay);
            SceneManager.LoadScene("GamePlay");   // name must match exactly in Build Settings
        }
    }

    private void EvaluateDailyWellbeing()
    {
        // If today you did NOT eat, DID work, and did NOT relax → bad pattern
        if (!ateToday && workedToday && !relaxedToday)
        {
            consecutiveStarveDays++;
        }
        else
        {
            consecutiveStarveDays = 0;
        }

        Debug.Log($"Wellbeing: ateToday={ateToday}, workedToday={workedToday}, relaxedToday={relaxedToday}, consecutiveStarveDays={consecutiveStarveDays}");

        // Reset per-day flags for the NEXT day
        ateToday = false;
        workedToday = false;
        relaxedToday = false;

        // If 4 days in a row like this -> death / burnout
        if (consecutiveStarveDays >= 4)
        {
            deathByNeglect = true;
            GoDeath();
        }
    }

    public void GoDeath()
    {
        State = GameState.Results;
        DataManager.I.FinalizeRun(totalCarbon);
        SceneManager.LoadScene("Results"); // or a separate "GameOver" scene if you want
    }

    public void GoDaySummary()
    {
        State = GameState.DaySummary;

        // Check health/cheat before pushing summary
        EvaluateDailyWellbeing();
        if (State == GameState.Results)
        {
            // We died in EvaluateDailyWellbeing() via GoDeath(), so don't continue
            return;
        }
    
        DataManager.I.PushDaySummary(currentDay, dayCarbon);
        SceneManager.LoadScene("DaySummary");
    }

    public void GoResults()
    {
        State = GameState.Results;
        DataManager.I.FinalizeRun(totalCarbon);
        SceneManager.LoadScene("Results");
    }

    public void ResetRun()
    {
        currentDay = 1;
        totalCarbon = 0;
        dayCarbon = 0;
        decisionsTakenToday = 0;
        consecutiveStarveDays = 0;
        deathByNeglect = false;
        ateToday = workedToday = relaxedToday = false;
        
        DataManager.I.ResetSession();
        SceneManager.LoadScene("GamePlay");
        State = GameState.Playing;
        UpdateHUD();
    }

    public void UpdateHUD()
    {
        if (ui == null) ui = FindObjectOfType<UIManager>();
        if (ui != null) ui.UpdateTopBar(currentDay, maxDays, totalCarbon);
    }
}
