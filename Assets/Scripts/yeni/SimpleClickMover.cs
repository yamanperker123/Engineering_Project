using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleClickMover : MonoBehaviour
{
    [SerializeField] float moveSpeed = 3f;
    CharacterController ctrl;
    Vector3 target; bool hasTarget;

    void Awake() => ctrl = GetComponent<CharacterController>();

    void Update()
    {
        if (!hasTarget) return;

        Vector3 dir = target - transform.position;
        float d = dir.magnitude;

        if (d < 0.05f) { hasTarget = false; return; }

        ctrl.Move(dir.normalized * moveSpeed * Time.deltaTime);

        // basit yerçekimi
        if (!ctrl.isGrounded)
            ctrl.Move(Physics.gravity * Time.deltaTime);
    }

    public void SetTarget(Vector3 worldPoint)
    {
        target = worldPoint;
        hasTarget = true;
    }
     public void CancelTarget() => hasTarget = false;
}
