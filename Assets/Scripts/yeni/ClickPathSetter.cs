// Assets/Scripts/Movement/ClickPathSetter.cs
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(PathMover))]
public class ClickPathSetter : MonoBehaviour
{
    public LayerMask groundMask;              // “Ground”
    public Camera    mainCam;
    PathMover mover;

    void Awake()
    {
        mover   = GetComponent<PathMover>();
        if (!mainCam) mainCam = Camera.main;
        if (groundMask == 0) groundMask = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit, 500f, groundMask))
            mover.MoveTo(hit.point);
    }
}

