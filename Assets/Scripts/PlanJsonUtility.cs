using UnityEngine;

/// <summary>
/// JsonUtility kısayolları – PlanRoot ileri-geri serileştirme.
///
/// </summary>
public static class PlanJsonUtility
{
    public static PlanRoot FromJson(string json) =>
        JsonUtility.FromJson<PlanRoot>(json);

    public static string ToJson(PlanRoot plan, bool pretty = true) =>
        JsonUtility.ToJson(plan, pretty);
}
