using UnityEngine;

public class OpenUrlOnClick : MonoBehaviour
{
    [Tooltip("Hedef web adresi")]
    [SerializeField] string url = "https://eng.yeditepe.edu.tr/en/computer-engineering-department/research";

#if UNITY_WEBGL && !UNITY_EDITOR
//js bridge
    [System.Runtime.InteropServices.DllImport("__Internal")]
    private static extern void OpenTab(string url);
#endif

    /* Tıklama */
    void OnMouseDown()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        OpenTab(url);                 // yeni sekme
#else
        Application.OpenURL(url);     // editorde açma
#endif
    }
}
