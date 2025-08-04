using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomListUI : MonoBehaviour
{
    [Header("References")]
    public PathMover player;          // Player üzerindeki PathMover
    public Transform content;         // ScrollView/Viewport/Content
    public Button    rowPrefab;       

void Start()
{
    /*  Controls */
    if (SceneData.Rooms == null || SceneData.Rooms.Length == 0)
    {
        Debug.LogWarning("RoomListUI -> SceneData.Rooms empty!");
        return;
    }
    if (!player)    Debug.LogError("RoomListUI -> Player empty!");
    if (!content)   Debug.LogError("RoomListUI -> Content empty!");
    if (!rowPrefab) Debug.LogError("RoomListUI -> RowPrefab empty!");

    
    foreach (var r in SceneData.Rooms)
    {
        Button btn = Instantiate(rowPrefab, content);

        
        string roomLabel = r.label.Trim();
        btn.GetComponentInChildren<TextMeshProUGUI>().text = roomLabel;

        btn.onClick.AddListener(() =>
        {
            Debug.Log($"Room '{roomLabel}' button clicked");

            if (!player) return;

            GameObject target = GameObject.Find(roomLabel);
            if (!target)
            {
                Debug.LogWarning($"RoomTarget '{roomLabel}' not found!");
                return;
            }
            player.MoveTo(target.transform.position);
        });
    }
}


}
