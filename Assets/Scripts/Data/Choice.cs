using UnityEngine;

[System.Serializable]
public class Choice
{
    [TextArea] public string optionText;
    public float carbonImpactKg;
    [TextArea] public string feedbackText;

    [Header("Wellbeing Tags")]
    [Tooltip("Does this choice count as eating a proper meal today?")]
    public bool countsAsMeal;
    
    [Tooltip("Does this choice represent working (job, study, long screen time)?")]
    public bool countsAsWork;
    
    [Tooltip("Does this choice represent relaxing / leisure (travel, reading, walk, etc.)?")]
    public bool countsAsRelax;

    [Header("Travel / Time Skip")]
    [Tooltip("Days to skip after this day when this choice is taken (for long trips). 0 = no extra days.")]
    // [Tooltip("Extra days to skip after this day (long trips)")]
    public int daysToSkipAfter = 0;
}
