// Assets/Scripts/MiniMap.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class MiniMap2 : MonoBehaviour, IPointerClickHandler
{
    public Camera miniCam;
    public PathMover player;
    public LayerMask groundMask;
    [Min(1f)] public float rayLen = 500f;

    public void OnPointerClick(PointerEventData e)
    {
        if (!miniCam || !player) return;

        RectTransform rt = (RectTransform)transform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rt, e.position, e.pressEventCamera, out var local))
            return;

        Vector2 uv = new((local.x / rt.rect.width) + .5f,
                         (local.y / rt.rect.height) + .5f);

        Ray ray = miniCam.ViewportPointToRay(new Vector3(uv.x, uv.y, 0));
        if (Physics.Raycast(ray, out var hit, rayLen, groundMask))
            player.MoveTo(hit.point);   
    }
}
