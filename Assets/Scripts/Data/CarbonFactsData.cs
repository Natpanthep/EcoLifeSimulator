using UnityEngine;

[CreateAssetMenu(fileName = "CarbonFactsData_", menuName = "EcoLife/Carbon Facts", order = 10)]
public class CarbonFactsData : ScriptableObject
{
    [TextArea(2,5)] public string[] facts;   // 5–7 short items
}
