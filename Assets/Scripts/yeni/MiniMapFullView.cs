// Assets/Scripts/UI/MiniMapFullView.cs
using UnityEngine;


[RequireComponent(typeof(Camera))]
public class MiniMapFullView : MonoBehaviour
{
    [Header("Tuning")]
    [Range(0.6f, 1f)]  public float fitPercent = .85f;   
    [Min(0f)]          public float border      = 0f;    
    public bool        applyOnStart  = true;           

    Camera cam;
    void Start()
    {
        if (applyOnStart) Fit();
    }

    /// <summary>Plan boyutuna göre kamerayı ayarlar.</summary>
    [ContextMenu("Fit Now")]
    public void Fit()
    {
        cam = cam ? cam : GetComponent<Camera>();

        /* Control */
        if (SceneData.Plan == null || SceneData.Plan.lines == null)
        {
            Debug.LogError("MiniMapFullView ► SceneData.Plan boş!");
            return;
        }

        /* Piksel alanında sınırlar (min/max) */
        Vector2 minPx = new(+9e9f, +9e9f), maxPx = new(-9e9f, -9e9f);
        foreach (var s in SceneData.Plan.lines)
        {
            minPx = Vector2.Min(minPx, s.p1);
            minPx = Vector2.Min(minPx, s.p2);
            maxPx = Vector2.Max(maxPx, s.p1);
            maxPx = Vector2.Max(maxPx, s.p2);
        }
        Vector2 centerPx = (minPx + maxPx) * .5f;

        /* Piksel -> dünya ölçeği */
        float ppu = SceneData.pixelsPerUnit;          
        if (ppu <= 0f) { Debug.LogError("pixelsPerUnit=0!"); return; }

        Vector2 halfWorld = (maxPx - minPx) / (2f * ppu);  // yarı boyut (X,Z)
        float  halfX = halfWorld.x + border;
        float  halfZ = halfWorld.y + border;

        /*  Kamera konum/ölçek */
        Vector3 pos = cam.transform.position;
        cam.transform.position = new Vector3(0, pos.y, 0)   // Y sabit kalsın
                               + new Vector3(0, 0,
                                             0);            // merkez 0,0 olacak 

        // Dünya merkezini bul 
        Vector3 worldCenter = new Vector3(
            (0f) ,                         // x   = 0  
            pos.y,
            0f);                           // z   = 0
        cam.transform.position = new Vector3(worldCenter.x, pos.y, worldCenter.z);

        float aspect = cam.aspect;
        float size   = Mathf.Max(halfZ, halfX / aspect) * fitPercent;
        cam.orthographic       = true;
        cam.orthographicSize   = size;
        cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}
