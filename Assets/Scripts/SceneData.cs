using UnityEngine;

/// <summary>
/// Sahneler arası statik veri konteyneri.
/// </summary>
public static class SceneData
{
    // ───────── Plan bilgisi ─────────
    public static PlanRoot Plan;
    public static string PlanJson;

    public static RoomInfo[] Rooms;     
    public static string     RoomsJson; 

    // ───────── Mobilya yerleşimleri ─────────
[System.Serializable] public class FurniturePlacement
{
    public string prefabName;  // Resources/Furniture/ içindeki Prefab adı
    public float  x;           // Plan üzerindeki piksel X
    public float  z;           // Plan üzerindeki piksel Z
    public float  rotY;        // Y ekseninde döndürme (derece)
    public float  scale = 1f;  // Opsiyonel ölçek çarpanı (default = 1)
}

[System.Serializable]
public class RoomInfo
{
    public string  label;     // ex "cse 41" , "Lab-A-3"
    public Vector2 centerPx;  // 2D plan pikseli
}

    public static FurniturePlacement[] Furnitures;
    public static string FurnituresJson;

    // ───────── Oyuncu başlangıcı ─────────  // 
    public static bool hasSpawn = false;
    public static Vector3 spawnWorld;   
     public static Vector2 spawnPixels;
    public static float pixelsPerUnit = 100f;  
}

/// <remarks>
/// Wrapper `JsonUtility.ToJson()` için var.
/// </remarks>
[System.Serializable]
public class FurniturePlacementListWrapper
{
    public SceneData.FurniturePlacement[] furn;
}
