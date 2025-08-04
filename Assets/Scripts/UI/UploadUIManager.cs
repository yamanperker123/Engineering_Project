// Assets/Scripts/UI/UploadUIManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Runtime.InteropServices;

public class UploadUIManager : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void PickAndSend(string goName, string url);
#endif

    // ===============  PARAMETRELER  ===============
    private const string apiURLCloud = "https://fastapi-yaman.onrender.com/convert";
    private const string apiURLLocal = "http://127.0.0.1:8080/convert";

    [Header("UI")]
    [SerializeField] TMP_Text status;
    [SerializeField] Button   generateBtn;

    string apiRuntimeURL;
    // ==============================================

    void Start()
    {
        if (generateBtn) generateBtn.interactable = false;

        apiRuntimeURL =
            Application.platform == RuntimePlatform.WebGLPlayer
            ? apiURLCloud         // Build tarayıcıda -> Render HTTPS URL
            : apiURLLocal;        // Editor -> localhost
    }

    public void SelectImage()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (status) status.text = "Uploading…";
        PickAndSend(gameObject.name, apiRuntimeURL);
#else
        Debug.LogWarning("SelectImage works only in WebGL build.");
#endif
    }

    public void OnJsonReceived(string json)
    {
        if (string.IsNullOrEmpty(json) || json.Contains("\"error\""))
        {
            if (status) status.text = "<color=red>Upload error</color>";
            return;
        }

        SceneData.PlanJson = json;
        SceneData.Plan     = PlanJsonUtility.FromJson(json);

        if (status)      status.text = "Done — press Edit";
        if (generateBtn) generateBtn.interactable = true;
    }

    public void GoToGameScene()
    {
        if (string.IsNullOrEmpty(SceneData.PlanJson))
        {
            if (status) status.text = "No data yet!";
            return;
        }
        SceneManager.LoadScene("EditScene");
    }
}
