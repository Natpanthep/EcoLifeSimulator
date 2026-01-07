using UnityEngine;

[CreateAssetMenu(fileName = "Decision_", menuName = "EcoLife/Decision", order = 0)]
public class Decision : ScriptableObject
{
    [Header("Scenario")]
    [TextArea] public string scenarioText;
    
    [Tooltip("When this decision should appear in the day cycle")]
    public TimeOfDay timeOfDay = TimeOfDay.Morning;

    [Tooltip("For breakdown charts: Food, Transport, Energy, Shopping, Waste")]
    public string category = "Other";

    [Tooltip("Optional icon shown in feedback etc.")]
    public Sprite icon;

    [Header("Options")]
    public Choice[] choices;
}
