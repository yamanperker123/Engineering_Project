using UnityEngine;

public class PlanLoader : MonoBehaviour
{
    public static PlanLoader Instance;
    void Awake() => Instance = this;

    [System.Serializable] public class Line { public float x1,y1,x2,y2; }
    [System.Serializable] public class Coll { public Line[] lines; }

    public Material wallMat;
    

    public void LoadPlan(string json)
    {
        Coll data = JsonUtility.FromJson<Coll>(json);

        foreach (var l in data.lines)
        {
            Vector3 a = new(l.x1, 0, l.y1);
            Vector3 b = new(l.x2, 0, l.y2);

            var go = new GameObject("Wall");
            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.SetPositions(new[]{a,b});
            lr.startWidth = lr.endWidth = 0.1f;
            lr.material = wallMat;
        }
    }
}
