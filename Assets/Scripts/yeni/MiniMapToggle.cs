// Assets/Scripts/UI/MiniMapToggle.cs   
using UnityEngine;

public class MiniMapToggle : MonoBehaviour
{
    [SerializeField] GameObject miniMapPanel;   // RawImage veya Canvas altındaki Panel

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M) && miniMapPanel)
            miniMapPanel.SetActive(!miniMapPanel.activeSelf);
    }
}
