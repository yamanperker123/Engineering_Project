// Assets/Scripts/UI/DemoPlanLoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class DemoPlanLoader : MonoBehaviour
{
    [Header("JSON Asset")]
    [Tooltip("OkulPlanıYedek.json dosyasını buraya sürükleyin")]
    [SerializeField] private TextAsset planJsonAsset;

    /*──────────────────────────────────────────────────────────────*/
    // UI butonuna bağla
    public void LoadDemoAndGenerate()
    {
        /* 0 ─ Koruma */
        if (!planJsonAsset)
        {
            Debug.LogError($"{nameof(DemoPlanLoader)} ► TextAsset atanmadı!");
            return;
        }

        /* 1 ─ JSON → PlanRoot */
        SceneData.PlanJson = planJsonAsset.text;

        PlanRoot root;
        try
        {
            root = JsonUtility.FromJson<PlanRoot>(SceneData.PlanJson);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"DemoPlanLoader ► JSON parse hatası:\n{ex}");
            return;
        }

        /* 2 ─ SceneData’ya yaz */
        SceneData.Plan        = root;

        // ── Mobilya ───────────────────────────────────────────────
        SceneData.Furnitures  = root.furn;
        if (root.furn != null)
        {
            SceneData.FurnituresJson = JsonUtility.ToJson(
                new FurniturePlacementListWrapper { furn = root.furn });
        }

        // ── Odalar ────────────────────────────────────────────────
        SceneData.Rooms = root.rooms;
        if (root.rooms != null)
        {
            SceneData.RoomsJson = JsonUtility.ToJson(
                new RoomListWrapper { rooms = root.rooms });
        }

        // ── Spawn (isteğe bağlı) ─────────────────────────────────
        if (root.spawn != null)
        {
            SceneData.hasSpawn    = true;
            SceneData.spawnPixels = new Vector2(root.spawn.x, root.spawn.z);
        }
        else
        {
            SceneData.hasSpawn = false;
        }

        /* 3 ─ Oyun sahnesine geç */
        SceneManager.LoadScene("OyunSahnesi");
    }

    /*──────────────────────────────────────────────────────────────*/
    // JsonUtility diziyi doğrudan serileştiremediği için basit wrapper
    [System.Serializable]
    private class RoomListWrapper
    {
        public SceneData.RoomInfo[] rooms;
    }
}
