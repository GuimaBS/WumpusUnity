using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControllerInput : MonoBehaviour
{
    [Header("Velocidades")]
    public float dragSpeed = 5f;
    public float zoomSpeed = 10f;
    public float rotationSpeed = 100f;
    public float verticalRotationSpeed = 50f;

    [Header("Zoom")]
    public float minZoom = 20f;
    public float maxZoom = 80f;

    [Header("Limites de rotação")]
    public float minRotationY = -45f;
    public float maxRotationY = 45f;
    public float minRotationX = 45f;
    public float maxRotationX = 90f;

    private Camera cam;
    private Controles controls;

    private Vector2 lookInput;
    private Vector2 rotateInput;
    private float verticalRotationInput = 0f;
    private bool dragging = false;
    private bool rotating = false;

    private int xSize = 10;
    private int ySize = 10;

    void Awake()
    {
        cam = Camera.main;
        controls = new Controles();

        controls.Camera.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Camera.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Camera.Drag.started += ctx => dragging = true;
        controls.Camera.Drag.canceled += ctx => dragging = false;

        controls.Camera.Zoom.performed += ctx => ZoomCamera(ctx.ReadValue<float>());

        controls.Camera.Rotate.performed += ctx => rotateInput = ctx.ReadValue<Vector2>();
        controls.Camera.Rotate.canceled += ctx => rotateInput = Vector2.zero;

        controls.Camera.RotateEnable.started += ctx => rotating = true;
        controls.Camera.RotateEnable.canceled += ctx => rotating = false;

        controls.Camera.RotateVertical.performed += ctx => verticalRotationInput = ctx.ReadValue<float>();
        controls.Camera.RotateVertical.canceled += ctx => verticalRotationInput = 0f;
    }

    void OnEnable() => controls.Camera.Enable();
    void OnDisable() => controls.Camera.Disable();

    void Start()
    {
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        xSize = PlayerPrefs.GetInt("mapX", 10);
        ySize = PlayerPrefs.GetInt("mapY", 10);
    }

    void Update()
    {
        if (dragging && !rotating) // só move se não estiver rotacionando
        {
            Vector3 move = new Vector3(-lookInput.x, 0, -lookInput.y) * dragSpeed * Time.deltaTime;
            Vector3 newPosition = transform.position + move;

            float border = 2f;
            float minX = -border;
            float maxX = (xSize - 1) + border;
            float minZ = -border;
            float maxZ = (ySize - 1) + border;

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

            transform.position = newPosition;
        }

        if (rotating && rotateInput != Vector2.zero)
        {
            // ROTATE Y - giro lateral (horizontal)
            float currentY = transform.eulerAngles.y;
            if (currentY > 180f) currentY -= 360f;
            float targetY = currentY + rotateInput.x * rotationSpeed * Time.deltaTime;
            targetY = Mathf.Clamp(targetY, minRotationY, maxRotationY);

            // ROTATE X - inclinação (vertical)
            float currentX = transform.eulerAngles.x;
            if (currentX > 180f) currentX -= 360f;
            float targetX = currentX - rotateInput.y * rotationSpeed * Time.deltaTime;
            targetX = Mathf.Clamp(targetX, minRotationX, maxRotationX);

            transform.rotation = Quaternion.Euler(targetX, targetY, 0f);
        }

        // Rotação vertical extra por tecla (R/F)
        if (verticalRotationInput != 0f)
        {
            float currentX = transform.eulerAngles.x;
            if (currentX > 180f) currentX -= 360f;
            float targetX = currentX - verticalRotationInput * verticalRotationSpeed * Time.deltaTime;
            targetX = Mathf.Clamp(targetX, minRotationX, maxRotationX);

            transform.rotation = Quaternion.Euler(targetX, transform.eulerAngles.y, 0f);
        }
    }

    void ZoomCamera(float scroll)
    {
        float fov = cam.fieldOfView;
        fov -= scroll * zoomSpeed;
        fov = Mathf.Clamp(fov, minZoom, maxZoom);
        cam.fieldOfView = fov;
    }
}
