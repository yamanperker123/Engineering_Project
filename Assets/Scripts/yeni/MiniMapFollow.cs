// Assets/Scripts/UI/MiniMapFollow.cs
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MiniMapFollow : MonoBehaviour
{
    public Transform target;      // Player
    [Min(1f)] public float height = 30f;
    public bool rotateWithTarget  = false;

    void LateUpdate()
    {
        if (!target) return;

        /* Pozisyon */
        Vector3 p = target.position;
        transform.position = new Vector3(p.x, p.y + height, p.z);

        /* İsteğe bağlı rotasyon */
        if (rotateWithTarget)
            transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f);
    }
}
