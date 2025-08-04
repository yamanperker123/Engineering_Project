// SaveUtils.cs 
using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;

public static class SaveUtil
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void DownloadFile(string fileName, string content);
#endif


    public static void SaveJson(string json, string fileName = "plan.json")
    {
        if (string.IsNullOrEmpty(json)) return;

#if UNITY_WEBGL && !UNITY_EDITOR
        DownloadFile(fileName, json);
#else
        var path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllText(path, json);
        Debug.Log($"[SaveUtils] JSON written → {path}");
#endif
        PlayerPrefs.SetString("lastPlanJson", json);
        PlayerPrefs.Save();
    }
}
