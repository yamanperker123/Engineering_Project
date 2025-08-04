// Assets/Scripts/yeni/PathDirectionsUI.cs
using UnityEngine;
using TMPro;
using System.Text;


public class PathDirectionsUI : MonoBehaviour
{
    [Tooltip("Player üzerindeki PathMover")]
    public PathMover mover;

    [Tooltip("Direction TMP bileşeni")]
    public TextMeshProUGUI txt;

    [Header("Angle Degree")]
    [Range(5f, 90f)] public float turnThreshold = 30f;

    
    [Header("Path Scale")]
    [Tooltip("100m = 1m, lower it for small values")]
    public float baselinePixelsPerMeter = 100f;
   

    void OnEnable()  { if (mover) mover.OnPathComputed += WriteDirections; }
    void OnDisable() { if (mover) mover.OnPathComputed -= WriteDirections; }

    void WriteDirections(Vector3[] corners)
    {
        if (!txt) return;
        if (corners == null || corners.Length < 2)
        {
            txt.text = "";
            return;
        }

        /* ppu → metre ölçek faktörü */
        float scaleFactor = 1f;
        if (baselinePixelsPerMeter > 0.01f && SceneData.pixelsPerUnit > 0.01f)
            scaleFactor = SceneData.pixelsPerUnit / baselinePixelsPerMeter;

        StringBuilder sb   = new();
        Vector3 prevDir    = (corners[1] - corners[0]).normalized;
        float   accum      = Vector3.Distance(corners[0], corners[1]);

        for (int i = 2; i < corners.Length; i++)
        {
            Vector3 curDir = (corners[i] - corners[i - 1]).normalized;
            float   angle  = Vector3.SignedAngle(prevDir, curDir, Vector3.up);

            if (Mathf.Abs(angle) > turnThreshold)
            {
                // biriktirilmiş ileri git
                sb.AppendLine($"{Mathf.RoundToInt(accum * scaleFactor)} m forward");

                // dönüş
                sb.AppendLine(angle > 0 ? "Turn right" : "Turn left");

                // yeni segment
                accum   = Vector3.Distance(corners[i - 1], corners[i]);
                prevDir = curDir;
            }
            else
            {
                accum += Vector3.Distance(corners[i - 1], corners[i]);
            }
        }

        // son düz mesafe
        if (accum > 0.1f)
            sb.AppendLine($"{Mathf.RoundToInt(accum * scaleFactor)} m forward");

        sb.AppendLine("Destination Reached");
        txt.text = sb.ToString();
    }
}
