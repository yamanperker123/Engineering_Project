// Assets/Scripts/Generation/PlanBuilder.cs
using UnityEngine;
using Unity.AI.Navigation;          // NavMeshSurface
using UnityEngine.AI;               // NavMesh*, NavMeshAgent
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshSurface))]
public class PlanBuilder : MonoBehaviour
{
    /* ────────── Inspector ────────── */
    [Header("Scene Refs")]
    [SerializeField] Transform ground;           // Zemin objesi (Plane / Cube)

    [Header("Wall Settings")]
    [SerializeField] float    wallThickness   = .20f;
    [SerializeField] float    wallHeightRatio = .10f;   // zemine göre yükseklik
    [SerializeField] Material wallMat;


[SerializeField] GameObject doorPrefab;  // Kapı modeli
[SerializeField] float      doorWidth   = 1.0f; // metre
[SerializeField] float      doorHeight  = 2.1f; // metre
[SerializeField] GameObject wallPrefab;   // sahnede hazırladığın Cube  prefabı


    /* ────────── Private ────────── */
    NavMeshSurface surface;
    Transform      wallParent;
    const string   WALL_ROOT = "Walls";

Transform furnitureParent;                 
const string FURN_ROOT = "Furniture";


    /*──────────────────────────────────────────────────────────────────────────*/
void Awake()
{
    // 0) Gerekli referansları bul
    surface = GetComponent<NavMeshSurface>();

    // 1) Hızlı güvenlik kontrolleri
    if (!ground)
    {
        Debug.LogError("PlanBuilder -> ground object empty!");
        return;
    }
    if (SceneData.Plan == null)
    {
        Debug.LogError("PlanBuilder -> SceneData.Plan empty!");
        return;
    }

    // 2) konteynerler
    wallParent      = new GameObject(WALL_ROOT ).transform;  // "Walls"
    furnitureParent = new GameObject(FURN_ROOT).transform;   // "Furniture"

    // 3) Planı zemine sığdır; piksel-to-world dönüşümü için ppu & merkez hesapla
    FitPlanToGround(out float ppu, out Vector2 centerPx);

    // 4) Duvarları inşa et
    BuildWalls(ppu, centerPx);

    // 5) Mobilyaları yerleştir
    PlaceFurniture(ppu, centerPx);

    // 6) NavMesh’i bake et (duvar + mobilya)
    surface.BuildNavMesh();
    Debug.Log("PlanBuilder ► NavMesh bake tamam!");

    // 7) Sahnedeki mevcut player’ı yeni koordinata taşı
    MoveExistingPlayer(ppu, centerPx);
}


    /*──────────────────────────────────────────────────────────────────────────*/
    void FitPlanToGround(out float ppu, out Vector2 centerPx)
    {
        Vector2 min = new(+9e9f, +9e9f), max = new(-9e9f, -9e9f);
        foreach (var s in SceneData.Plan.lines)
        {
            min = Vector2.Min(min, s.p1);  min = Vector2.Min(min, s.p2);
            max = Vector2.Max(max, s.p1);  max = Vector2.Max(max, s.p2);
        }
        Vector2 sizePx = max - min;

        Bounds gb = ground.GetComponent<Renderer>() ?
                    ground.GetComponent<Renderer>().bounds :
                    ground.GetComponent<Collider>().bounds;

        Vector2 sizeW  = new(gb.size.x, gb.size.z);
        float   scale  = Mathf.Min(sizeW.x/sizePx.x, sizeW.y/sizePx.y) * .95f;

        ppu      = 1f / scale;
        centerPx = (max + min) * .5f;
        SceneData.pixelsPerUnit = ppu;
    }

    


void BuildWalls(float ppu, Vector2 centerPx)
{
    float wallHeight = ground.lossyScale.y * wallHeightRatio + 2f;

    /*────────────────────  Yerel yardımcı ────────────────────*/
    void SpawnWallStub(Vector3 center, Vector3 dir, float len, float h = -1f)
    {
        float hUse = (h > 0f) ? h : wallHeight;

        GameObject cube;
        if (wallPrefab != null)
        {
            cube = Instantiate(wallPrefab, wallParent);
            cube.transform.position   = center + Vector3.up * (hUse * 0.5f);
            cube.transform.rotation   = Quaternion.LookRotation(dir, Vector3.up);
            cube.transform.localScale = new Vector3(wallThickness, hUse, len);
        }
        else
        {
            cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.SetParent(wallParent, true);
            cube.transform.position   = center + Vector3.up * (hUse * 0.5f);
            cube.transform.rotation   = Quaternion.LookRotation(dir, Vector3.up);
            cube.transform.localScale = new Vector3(wallThickness, hUse, len);

            /* Z cephede UV tersliyse ölçeği çevir */
            if (Mathf.Abs(dir.z) > Mathf.Abs(dir.x))
            {
                Vector3 sc = cube.transform.localScale;
                sc.z = -sc.z;
                cube.transform.localScale = sc;
            }
            if (wallMat) cube.GetComponent<Renderer>().sharedMaterial = wallMat;
        }
        cube.isStatic = true;        // NavMesh engeli
    }

    /*──────── 1) Uç noktaları say ────────*/
    var endpointCount = new Dictionary<Vector3Int, int>();

    Vector3Int Quant(Vector3 v)
    {
        const float grid = 0.05f;     //  hassasiyet
        return new Vector3Int(
            Mathf.RoundToInt(v.x / grid),
            Mathf.RoundToInt(v.y / grid),
            Mathf.RoundToInt(v.z / grid));
    }

    foreach (var seg in SceneData.Plan.lines)
    {
        if (seg.type != "wall" && seg.type != "door") continue;

        Vector3 p1 = PxToWorld(seg.p1, ppu, centerPx);
        Vector3 p2 = PxToWorld(seg.p2, ppu, centerPx);

        var q1 = Quant(p1);
        var q2 = Quant(p2);

        if (!endpointCount.ContainsKey(q1)) endpointCount[q1] = 0;
        if (!endpointCount.ContainsKey(q2)) endpointCount[q2] = 0;

        endpointCount[q1] += 1;
        endpointCount[q2] += 1;
    }

    /*──────── 2) Geometriyi inşa et ────────*/
    foreach (var seg in SceneData.Plan.lines)
    {
        string kind = seg.type?.ToLowerInvariant();
        if (kind != "wall" && kind != "door") continue;

        Vector3 a   = PxToWorld(seg.p1, ppu, centerPx);
        Vector3 b   = PxToWorld(seg.p2, ppu, centerPx);
        Vector3 dir = (b - a).normalized;
        float   len = Vector3.Distance(a, b);


        float ext   = wallThickness * 0.5f;      
        Vector3 newA   = a - dir * ext;
        Vector3 newB   = b + dir * ext;
        float   newLen = Vector3.Distance(newA, newB);
       

        if (kind == "wall")
        {
            SpawnWallStub((newA + newB) * .5f, dir, newLen + 0.0001f);
            continue;
        }

        if (kind == "door")
        {
            float dw      = Mathf.Min(doorWidth, newLen);
            float sideLen = Mathf.Max(0f, (newLen - dw) * 0.5f);

            // Kapıya bitişik yan duvarlar
            if (sideLen > 0.01f)
            {
                Vector3 cL = newA + dir * (sideLen * 0.5f);
                Vector3 cR = newB - dir * (sideLen * 0.5f);
                SpawnWallStub(cL, dir, sideLen);
                SpawnWallStub(cR, dir, sideLen);
            }

            // Kapı prefabı
            if (doorPrefab)
            {
                Vector3 doorPos = (newA + newB) * .5f + Vector3.up * (doorHeight * .5f);
                Vector3 doorFwd = Vector3.Cross(Vector3.up, dir);      // iç-dış yön
                Quaternion rot  = Quaternion.LookRotation(doorFwd, Vector3.up);

                GameObject door = Instantiate(doorPrefab, doorPos, rot, wallParent);
                Vector3 sc      = door.transform.localScale;
                door.transform.localScale = new Vector3(dw, doorHeight, sc.z);
            }
        }
    }
}

/// <summary>
/// SceneData.Furnitures içindeki kayıtları sahneye yerleştirir.
/// </summary>
void PlaceFurniture(float ppu, Vector2 centerPx)
{
    if (SceneData.Plan.furn == null) return;

    foreach (var fp in SceneData.Plan.furn)
    {
        Vector3 pos = PxToWorld(new Vector2(fp.x, fp.z), ppu, centerPx);
        Quaternion rot = Quaternion.Euler(0, fp.rotY, 0);

        var prefab = Resources.Load<GameObject>("Furniture/" + fp.prefabName);
        if (!prefab) { Debug.LogWarning($"Prefab yok: {fp.prefabName}"); continue; }

        var go = Instantiate(prefab, pos, rot, furnitureParent);
        go.name = fp.prefabName;
        go.isStatic = true;                         

        // Collider yoksa otomatik ekleyebilirsin:
        if (!go.TryGetComponent<Collider>(out _))
            _ = go.AddComponent<BoxCollider>();
    }
    Debug.Log($"PlanBuilder ► {SceneData.Plan.furn.Length} mobilya yerleştirildi.");
}


    /*──────────────────────────────────────────────────────────────────────────*/
void MoveExistingPlayer(float ppu, Vector2 centerPx)
{
    /* 0) Player bul (Tag = "Player") */
    var playerObj = GameObject.FindGameObjectWithTag("Player");
    if (!playerObj)
    { Debug.LogWarning("PlanBuilder ► Player bulunamadı (Tag=Player?)"); return; }

    var agent = playerObj.GetComponent<NavMeshAgent>();
    if (!agent)
    { Debug.LogWarning("PlanBuilder ► Player’da NavMeshAgent yok!"); return; }

    /* 1) Hedef pozisyon (px → world) */
    Vector3 target = SceneData.hasSpawn
        ? PxToWorld(SceneData.spawnPixels, ppu, centerPx)   // PlanRoot.spawn’dan geldi
        : ground.position;                                  // yedek: zeminin ortası

    /* 2) Yüksekliği zemine oturt */
    if (Physics.Raycast(target + Vector3.up * 5f, Vector3.down,
                        out var hit, 20f, LayerMask.GetMask("Ground")))
        target = hit.point + Vector3.up * 0.05f;

    /* 3) Zeminin sınırları içinde kal  */
    Bounds gb = ground.GetComponent<Renderer>() ?
                ground.GetComponent<Renderer>().bounds :
                ground.GetComponent<Collider>().bounds;
    float margin = 0.25f;
    target.x = Mathf.Clamp(target.x, gb.min.x + margin, gb.max.x - margin);
    target.z = Mathf.Clamp(target.z, gb.min.z + margin, gb.max.z - margin);

    /* 4) NavMesh üçgeni bul */
    if (NavMesh.SamplePosition(target, out var navHit, 2f, NavMesh.AllAreas))
        target = navHit.position;
    else
        Debug.LogWarning("PlanBuilder -> Spawn Out of NavMesh , no plane near by.");

    /* 5) Agent’i sorunsuz taşı */
    agent.enabled = false;
    playerObj.transform.position = target;
    agent.enabled = true;
    agent.Warp(target);

    SceneData.spawnWorld = target;
    Debug.Log($"PlanBuilder -> Player moved to {target}");
}


    /*──────────────────────────────────────────────────────────────────────────*/
/// <summary>Plan piksel koordinatını dünya koordinatına çevirir;
/// zeminin (ground) merkezini referans alır.</summary>
Vector3 PxToWorld(Vector2 px, float ppu, Vector2 centerPx)
{
    Vector2 local = (px - centerPx) / ppu;          // plan merkezine göre ofset
    
    return ground.position + new Vector3(local.x, 0f, -local.y);
}

}
