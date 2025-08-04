using System;
using UnityEngine;

/*────────────────────────── SEGMENT ──────────────────────────*/
[Serializable]
public class Segment
{
    public int      id;
    public Vector2  p1;
    public Vector2  p2;
    public string   type;   // "wall" | "door" | "window"
}

/*────────────────────────── SPAWN ────────────────────────────*/
[Serializable]
public class SpawnPoint
{
    public float x;
    public float z;
}

/*────────────────────────── PLAN ROOT ────────────────────────*/
[Serializable]
public class PlanRoot
{
    public Segment[] lines;   // duvar-kapı-pencere
    public SceneData.FurniturePlacement[] furn;    // furniture
    public SpawnPoint spawn;   // player spawn
    public SceneData.RoomInfo[]            rooms;   // oda numaraları
}
