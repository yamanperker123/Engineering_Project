// Assets/Scripts/Utils/WebCursorFix.cs
using UnityEngine;

public class WebCursorFix : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    void Awake()
    {
        Cursor.lockState = CursorLockMode.None;   // İmleci kilitleme
        Cursor.visible   = true;                  // İmleç her zaman görünsün
    }
#endif
}

