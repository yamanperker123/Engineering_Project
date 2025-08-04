using System.Collections.Generic;
using UnityEngine;

public class SegmentDrawer : MonoBehaviour
{
    [Header("Scale")]
    public float pixelsPerUnit = 100f;         // 100 px = 1 m

    [Header("Line")]
    [SerializeField] Material lineMat;
    [SerializeField] float    lineWidth = 0.05f;

    public Vector2 Min { get; private set; }
    public Vector2 Max { get; private set; }

    readonly List<LineRenderer> pool = new();

    /* -------------------------------------------------------------- */
    public void Draw(IReadOnlyList<Segment> segs)
    {
        foreach (var lr in pool) lr.gameObject.SetActive(false);

        Min = new Vector2(+99999, +99999);
        Max = new Vector2(-99999, -99999);

        for (int i = 0; i < segs.Count; i++)
        {
            if (i >= pool.Count) pool.Add(CreateLR());
            var lr = pool[i];
            lr.gameObject.SetActive(true);

            Segment s = segs[i];

            // px → world (Y/Z ters çevrilerek)
            Vector2 pa = s.p1 / pixelsPerUnit;
            Vector2 pb = s.p2 / pixelsPerUnit;
            pa.y = -pa.y;   pb.y = -pb.y;

            Min = Vector2.Min(Min, pa);  Min = Vector2.Min(Min, pb);
            Max = Vector2.Max(Max, pa);  Max = Vector2.Max(Max, pb);

            Vector3 a = new Vector3(pa.x, 0.01f, pa.y);
            Vector3 b = new Vector3(pb.x, 0.01f, pb.y);

            lr.startColor = lr.endColor = s.type switch
            {
                "door"   => Color.green,
                "window" => Color.cyan,
                _        => new Color(0.15f,0.15f,0.15f)
            };
            lr.SetPosition(0, a);
            lr.SetPosition(1, b);
        }
    }

    /* -------------------------------------------------------------- */
    LineRenderer CreateLR()
    {
        GameObject go = new("LineSeg");
        go.transform.SetParent(transform, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.material       = lineMat ? lineMat : new Material(Shader.Find("Sprites/Default"));
        lr.useWorldSpace  = true;
        lr.positionCount  = 2;
        lr.widthMultiplier= lineWidth;
        lr.numCapVertices = 0;
        return lr;
    }
}
