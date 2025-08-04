// Assets/Scripts/Movement/FPSController.cs  –  v2
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Hareket")]
    public float moveSpeed = 5f;
    public float gravity   = -9.81f;

    [Header("Fare Duyarlılığı")]
    public float mouseSens = 2f;
    public Transform cam;                    // Player içindeki kamera

    CharacterController ctrl;
    SimpleClickMover    clickMover;
    Vector3 velocity;
    float   pitch = 0f;
    bool    isAiming = false;

    void Awake()
    {
        ctrl       = GetComponent<CharacterController>();
        clickMover = GetComponent<SimpleClickMover>();
        if (!cam)  cam = Camera.main.transform;

        // Başlangıçta serbest imleç
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    void Update()
    {
        /* ——— RMB ile bakış ——— */
        if (Input.GetMouseButtonDown(1)) BeginAim();
        if (Input.GetMouseButtonUp(1))   EndAim();
        if (isAiming)                    LookWithMouse();

        MoveWithKeys();
    }

    void BeginAim()
    {
        isAiming = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    void EndAim()
    {
        isAiming = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    void LookWithMouse()
    {
        float mx = Input.GetAxis("Mouse X") * mouseSens;
        float my = Input.GetAxis("Mouse Y") * mouseSens;

        transform.Rotate(0f, mx, 0f);

        pitch -= my;
        pitch = Mathf.Clamp(pitch, -75f, 75f);
        cam.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    void MoveWithKeys()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = (transform.forward * v + transform.right * h).normalized;

        if (dir.sqrMagnitude > 0f)
        {
            if (GetComponent<UnityEngine.AI.NavMeshAgent>().hasPath)
            GetComponent<UnityEngine.AI.NavMeshAgent>().ResetPath();
            ctrl.Move(dir * moveSpeed * Time.deltaTime);
        }

        /* Yerçekimi */
        if (ctrl.isGrounded && velocity.y < 0) velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
        ctrl.Move(velocity * Time.deltaTime);
    }
}
