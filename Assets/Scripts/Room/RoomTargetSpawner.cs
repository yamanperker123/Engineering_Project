using UnityEngine;
using UnityEngine.AI;

public class RoomTargetSpawner : MonoBehaviour
{
    public Transform markerPrefab;

void Start()
{
    if (SceneData.Rooms == null || SceneData.Plan == null)
        return;

    // 1) Plan merkezini bul
    Vector2 min = new(+9e9f, +9e9f), max = new(-9e9f, -9e9f);
    foreach (var s in SceneData.Plan.lines)
    {
        min = Vector2.Min(min, s.p1);  min = Vector2.Min(min, s.p2);
        max = Vector2.Max(max, s.p1);  max = Vector2.Max(max, s.p2);
    }
    Vector2 centerPx = (max + min) * 0.5f;
    float   ppu      = SceneData.pixelsPerUnit;

    // 2) Her oda için hedef oluştur
    foreach (var r in SceneData.Rooms)
    {
        Vector3 w = PxToWorld(r.centerPx, ppu, centerPx);
        if (NavMesh.SamplePosition(w, out var hit, 3f, NavMesh.AllAreas))
            w = hit.position;

        Transform t = markerPrefab
                    ? Instantiate(markerPrefab, w, Quaternion.identity).transform
                    : new GameObject($"RoomTarget_{r.label}").transform;

        t.position = w;
        t.name     = r.label;
    }
}

    /* px → dünya  */
    static Vector3 PxToWorld(Vector2 px, float ppu, Vector2 centerPx)
    {
        Vector2 v = (px - centerPx) / ppu;
        return new Vector3(v.x, 0f, -v.y);
    }
}
