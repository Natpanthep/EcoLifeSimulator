using UnityEngine;

[CreateAssetMenu(fileName = "ScenarioData_", menuName = "EcoLife/ScenarioData", order = 1)]
public class ScenarioData : ScriptableObject
{
    [Tooltip("Pool of decisions for this object/time-of-day")]
    public Decision[] decisions;
}
