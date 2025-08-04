using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class PlanEditor : MonoBehaviour
{
    public enum Mode
    {
        AddWall = 1,
        Delete = 2,
        MarkDoor = 3,
        MarkWindow = 4,
        PlacePrefab = 5,
        SetSpawn = 6,
        LabelRoom = 7

    }

    [Header("References")]
    [SerializeField] Camera editCam;
    [SerializeField] SegmentDrawer drawer;
    [SerializeField] TextMeshProUGUI infoText;

    [Header("Save")]
    [SerializeField] string saveFileName = "plan.json";


    Mode current = Mode.AddWall;
    readonly List<Segment> segs = new();
    readonly Stack<List<Segment>> undo = new();
    readonly Stack<List<Segment>> redo = new();

    [Header("UI")]
    public TMP_InputField roomNoField;      
    public GameObject     roomMarkerPrefab; 

    readonly List<SceneData.RoomInfo> rooms = new();

        private readonly List<SceneData.FurniturePlacement> furnitureList
        = new List<SceneData.FurniturePlacement>();

    [Header("Prefab Placement")]
    [Tooltip("PlanEditor sahnesinde yerleştireceğiniz prefab")]
    public GameObject prefabToPlace;

    bool awaiting2nd;
    Vector3 firstWorld;

/* ───────── Prefab Yerleşim Göstergesi ───────── */
[Header("Placement Feedback")]
[SerializeField] Vector3 markerScale = new(0.20f, 0.05f, 0.20f);   
[SerializeField] Color   markerColor = new(1f, 0f, 0f, 0.60f);   

readonly List<GameObject> markers = new();                       

/* ───────── Spawn Yerleşim Göstergesi ───────── */
[Header("Spawn Feedback")]
[SerializeField] Vector3 spawnMarkerScale = new(0.25f, 0.05f, 0.25f);   
[SerializeField] Color   spawnMarkerColor = new(0f, 0.4f, 1f, 0.60f);  

GameObject spawnMarker;   

    void Start()
    {
        if (SceneData.Plan == null && !string.IsNullOrEmpty(SceneData.PlanJson))
            SceneData.Plan = JsonUtility.FromJson<PlanRoot>(SceneData.PlanJson);

        if (SceneData.Plan == null)
        {
            Debug.LogError("PlanEditor: SceneData.Plan null!");
            return;
        }

        segs.AddRange(SceneData.Plan.lines);
        drawer.Draw(segs);
        CenterCam();
        infoText.text = "Mode: AddWall";
    }

    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;
        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

void HandleClick()
{
    switch (current)
    {
        /*--------------------------------------------------------------*/
        case Mode.AddWall:
        {
            Vector3 worldPoint = RayToXZ();

            /* başlangıç noktasını tut */
            if (!awaiting2nd)
            {
                firstWorld = worldPoint;
                awaiting2nd = true;
                infoText.text = "Select end point for Wall";
            }
            /* bitiş noktasını eksene hizala */
            else
            {
                /* ---  Hizalama  ------------------------- */
                Vector3 snapped = worldPoint;    
                float   dx      = Mathf.Abs(worldPoint.x - firstWorld.x);
                float   dz      = Mathf.Abs(worldPoint.z - firstWorld.z);

                if (dx > dz)        
                    snapped.z = firstWorld.z;     
                else                
                    snapped.x = firstWorld.x;    

                /* --- control -- */
                if (Vector3.Distance(firstWorld, snapped) < 0.25f)
                {
                    infoText.text = "Too short for a wall";
                    awaiting2nd = false;
                    return;
                }

                /* ---Segmenti kaydet & çiz------- */
                PushUndo();
                segs.Add(new Segment
                {
                    id   = segs.Count,
                    type = "wall",
                    p1   = ToPixels(firstWorld),
                    p2   = ToPixels(snapped)       
                });

                awaiting2nd = false;
                drawer.Draw(segs);
                CenterCam();
                infoText.text = "Wall added";
            }
        }
        break;

        case Mode.Delete:
        {
            Vector3 worldPoint = RayToXZ();
            int idx = PickSeg(worldPoint);
            if (idx < 0)
            {
                infoText.text = "Nothing to delete here";
                return;
            }
            PushUndo();
            segs.RemoveAt(idx);
            drawer.Draw(segs);
            CenterCam();
            infoText.text = "Segment deleted";
        }
        break;

        case Mode.MarkDoor:
        {
            Vector3 worldPoint = RayToXZ();
            int idx = PickSeg(worldPoint);
            if (idx < 0)
            {
                infoText.text = "No segment to mark as Door";
                return;
            }
            PushUndo();
            segs[idx].type = "door";
            drawer.Draw(segs);
            infoText.text = "Segment marked as Door";
        }
        break;

        case Mode.MarkWindow:
        {
            Vector3 worldPoint = RayToXZ();
            int idx = PickSeg(worldPoint);
            if (idx < 0)
            {
                infoText.text = "No segment to mark as Window";
                return;
            }
            PushUndo();
            segs[idx].type = "window";
            drawer.Draw(segs);
            infoText.text = "Segment marked as Window";
        }
        break;

        case Mode.PlacePrefab:
        {
            Vector3 worldClick = RayToXZ();
            if (prefabToPlace == null)
            {
                infoText.text = "No prefab assigned!";
                return;
            }

            /* JSON kaydı  */
            Vector2 p = ToPixels(worldClick);
            var fp = new SceneData.FurniturePlacement
            {
                prefabName = prefabToPlace.name,
                x    = p.x,
                z    = p.y,
                rotY = 0f
            };

            var existing = SceneData.Furnitures;
            if (existing == null || existing.Length == 0)
                SceneData.Furnitures = new SceneData.FurniturePlacement[] { fp };
            else
            {
                var arr = new SceneData.FurniturePlacement[existing.Length + 1];
                existing.CopyTo(arr, 0);
                arr[^1] = fp;
                SceneData.Furnitures = arr;
            }
            SceneData.FurnituresJson = JsonUtility.ToJson(
                new FurniturePlacementListWrapper { furn = SceneData.Furnitures }, false);

            
            GameObject m = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            m.transform.position   = worldClick + Vector3.up * 0.02f;   
            m.transform.localScale = markerScale;
            m.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"))
            {
                color = markerColor
            };
            m.name = $"Marker_{markers.Count}";
            markers.Add(m);

            infoText.text = $"Prefab  ({p.x:F0}, {p.y:F0})";
        }
        break;


        case Mode.SetSpawn:
        {
            /* Dünya → px dönüşümü */
            Vector3 world  = RayToXZ();
            Vector2 pixels = ToPixels(world);        

            SceneData.spawnPixels = pixels;
            SceneData.hasSpawn    = true;
            infoText.text = $"Spawn set (px): {pixels.x:F0}, {pixels.y:F0}";

            /*  Sahnede tek mavi işaret tut */
            if (spawnMarker) Destroy(spawnMarker);

            spawnMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spawnMarker.transform.position   = world + Vector3.up * 0.02f;
            spawnMarker.transform.localScale = spawnMarkerScale;
            spawnMarker.GetComponent<Renderer>().material =
                new Material(Shader.Find("Standard")) { color = spawnMarkerColor };
            spawnMarker.name = "SpawnMarker";
        }
        break;

       
case Mode.LabelRoom:
{
    
    if (roomNoField == null || string.IsNullOrWhiteSpace(roomNoField.text))
    { infoText.text = "Enter room label first"; return; }

    string label = roomNoField.text.Trim();

    //  Tıklanan konum
    Vector3 world = RayToXZ();
    Vector2 px    = ToPixels(world);

    //  Çift kayıt kontrolü
    if (rooms.Exists(r => r.label == label))
    { infoText.text = $"Room '{label}' already exists"; return; }

    rooms.Add(new SceneData.RoomInfo { label = label, centerPx = px });

    //  Görsel 
    if (roomMarkerPrefab)
        Instantiate(roomMarkerPrefab, world + Vector3.up * 0.02f, Quaternion.identity);

    var textObj = new GameObject($"RoomLabel_{label}");
    var tmp     = textObj.AddComponent<TextMeshPro>();
    tmp.text = label;
    tmp.fontSize = 2.5f;
    textObj.transform.position = world + Vector3.up * 0.05f;

    infoText.text  = $"Room '{label}' placed";
    roomNoField.text = "";
}
break;
        /* --------------------------- */
    }
}


    #region 2D <-> 3D Yardımcı Metotlar

    Vector2 ToPixels(Vector3 world)
    {
        return new Vector2(world.x, -world.z) * drawer.pixelsPerUnit;
    }

    Vector3 ToWorld(Vector2 px)
    {
        return new Vector3(px.x / drawer.pixelsPerUnit, 0f, -px.y / drawer.pixelsPerUnit);
    }

    Vector3 RayToXZ()
    {
        Ray r = editCam.ScreenPointToRay(Input.mousePosition);
        new Plane(Vector3.up, Vector3.zero).Raycast(r, out float enter);
        return r.GetPoint(enter);
    }

    int PickSeg(Vector3 w)
    {
        float best = 0.35f;
        int idx = -1;
        for (int i = 0; i < segs.Count; i++)
        {
            Vector3 a = ToWorld(segs[i].p1);
            Vector3 b = ToWorld(segs[i].p2);
            float d = DistPointSeg(w, a, b);
            if (d < best)
            {
                best = d;
                idx = i;
            }
        }
        return idx;
    }

    static float DistPointSeg(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude);
        return Vector3.Distance(p, a + t * ab);
    }

    void CenterCam()
    {
        Vector2 min = drawer.Min, max = drawer.Max;
        Vector3 pos = new Vector3((min.x + max.x) * .5f, 10f, (min.y + max.y) * .5f);
        editCam.transform.position = pos;
        float ext = Mathf.Max(max.x - min.x, max.y - min.y) * .55f;
        editCam.orthographicSize = Mathf.Max(ext, 5f);
    }

    #endregion

    #region UNDO / REDO

    void PushUndo()
    {
        undo.Push(Clone(segs));
        redo.Clear();
    }

    static List<Segment> Clone(List<Segment> src)
    {
        var r = new List<Segment>(src.Count);
        foreach (var s in src)
        {
            r.Add(new Segment { id = s.id, type = s.type, p1 = s.p1, p2 = s.p2 });
        }
        return r;
    }

    public void Undo()
    {
        if (undo.Count == 0) return;
        redo.Push(Clone(segs));
        segs.Clear();
        segs.AddRange(undo.Pop());
        drawer.Draw(segs);
        CenterCam();
        infoText.text = "Undo";
    }

    public void Redo()
    {
        if (redo.Count == 0) return;
        undo.Push(Clone(segs));
        segs.Clear();
        segs.AddRange(redo.Pop());
        drawer.Draw(segs);
        CenterCam();
        infoText.text = "Redo";
    }

    #endregion

    #region UI HOOKS

    public void SetMode(int m)
    {
        current = (Mode)m;
        awaiting2nd = false;
        infoText.text = $"Mode: {current}";
    }

public void GenerateScene()
{
    /*────────────── çizgiler + mobilya + spawn ──────────────*/
    var planRoot = new PlanRoot
    {
        lines = segs.ToArray(),                                     // duvar-kapı-pencere
        furn  = (SceneData.Furnitures != null &&
                 SceneData.Furnitures.Length > 0)
                ? SceneData.Furnitures                              // mobilyalar
                : null,
        spawn = SceneData.hasSpawn
                ? new SpawnPoint {
                      x = SceneData.spawnPixels.x,
                      z = SceneData.spawnPixels.y
                  }
                : null
    };

    SceneData.Plan     = planRoot;
    SceneData.PlanJson = JsonUtility.ToJson(planRoot, true);

    /*──────────────  Oda etiketleri  ───────────────────────────*/
    if (rooms.Count > 0)
    {
        SceneData.Rooms     = rooms.ToArray();                      // RoomInfo[]
        SceneData.RoomsJson = JsonUtility.ToJson(
            new { rooms = SceneData.Rooms }, true);
    }
    else
    {
        SceneData.Rooms     = null;
        SceneData.RoomsJson = "";
    }

    
    SceneData.FurnituresJson = "";      

    /*──────────────  Ölçek & spawn world koordinatı ──────────────────*/
    SceneData.pixelsPerUnit = drawer.pixelsPerUnit;

    if (SceneData.hasSpawn)
    {
        Vector2 sp = SceneData.spawnPixels;
        SceneData.spawnWorld = new Vector3(
            sp.x / SceneData.pixelsPerUnit,
            0f,
           -sp.y / SceneData.pixelsPerUnit);
    }

    /*──────────────  Sahne geçişi ─────────────────────────────────────*/
    SceneManager.LoadScene("OyunSahnesi");      // 3D tur sahnesi
}


    #endregion
    
    public void SetSelectedFurniture(GameObject prefab)
{
    if (prefab == null) { infoText.text = "Prefab null!"; return; }

    prefabToPlace = prefab;         
    current       = Mode.PlacePrefab; 
    awaiting2nd   = false;            

    if (infoText != null)
        infoText.text = $"Mode: PlacePrefab\nSelected: {prefab.name}";
}
    
    
    public void SaveCurrentPlan()
    {
        /*  PlanRoot */
        var planRoot = new PlanRoot
        {
            lines = segs.ToArray(),
            furn = (SceneData.Furnitures != null &&
                     SceneData.Furnitures.Length > 0)
                    ? SceneData.Furnitures
                    : null,
            spawn = SceneData.hasSpawn
                    ? new SpawnPoint
                    {
                        x = SceneData.spawnPixels.x,
                        z = SceneData.spawnPixels.y
                    }
                    : null,
            rooms = rooms.Count > 0 ? rooms.ToArray() : null
        };

        /*  SceneData güncelle ve JSON üret */
        SceneData.Plan = planRoot;
        SceneData.PlanJson = JsonUtility.ToJson(planRoot, true);

        /* Dosyaya yaz */
        SaveUtil.SaveJson(SceneData.PlanJson, saveFileName);

        /* UI bildirimi */
        if (infoText != null)
            infoText.text = $"Plan saved → {saveFileName}";
    }
}
