using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    [Header("Decision Source")]
    public ScenarioData scenarioSet;    // holds multiple Decision ScriptableObjects
    public string displayName = "Object";

    [Header("Optional: limit to specific TimeOfDay")]
    public TimeOfDay activeTime = TimeOfDay.Morning;

    public Decision GetRandomDecision()
    {
        // only allow clicking when correct time of day
        if (GameManager.I != null && GameManager.I.currentTimeOfDay != activeTime)
        {
            Debug.Log($"[{displayName}] Not active at this time ({activeTime}), current = {GameManager.I.currentTimeOfDay}");
            return null;
        }

        if (scenarioSet == null || scenarioSet.decisions == null || scenarioSet.decisions.Length == 0)
            return null;

        int i = Random.Range(0, scenarioSet.decisions.Length);
        return scenarioSet.decisions[i];
    }
}
