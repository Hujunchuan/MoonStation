using UnityEngine;

namespace Lunar.Core
{
    public class LunarCameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float movementSpeed = 2f;
        [SerializeField] private float rotationSpeed = 1f;
        [SerializeField] private float zoomSpeed = III 5f;
        [SerializeField] private float smoothTime = 0.2f;

        [Header("Constraints")]
        [SerializeField] private float minZoom = 2f;
        [SerializeField] private float maxZoom = 10f;
        [SerializeField] private float minY = 0.5f;
        [SerializeField] private float maxY = 5f;
        [SerializeField] private Vector2 xConstraints = new Vector2(-10f, 10f);
        [SerializeField] private Vector2 zConstraints = new Vector2(-10f, 10f);

        [Header("Input Settings")]
        [SerializeField] private bool enableMouseMovement = true;
        [SerializeField] private bool enableKeyboardMovement = true;
        [SerializeField] private bool enableZoom = true;
        [SerializeField] private float mouseSensitivity = 0.5f;

        private Vector3 targetPosition;
        private float targetZoom;
        private Vector3 velocity = Vector3.zero;
        private float zoomVelocity = 0f;

        private Camera mainCamera;
        private bool isCameraEnabled = true;

        private Vector2 lastMousePosition;
        private bool isDragging = false;

        private void Start()
        {
            mainCamera = GetComponent<Camera>();
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            targetPosition = transform.position;
            targetZoom = mainCamera != null ? mainCamera.orthographicSize : 5f;

            Cursor.lockState = CursorLockMode.Confined;
        }

        private void Update()
        {
            if (!isCameraEnabled) return;

            HandleMouseInput();
            HandleKeyboardInput();

            SmoothCameraMovement();
            SmoothZoom();
        }

        private void HandleMouseInput()
        {
            if (!enableMouseMovement) return;

            if (Input.GetMouseButtonDown(1))
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(1))
            {
                isDragging = false;
            }

            if (isDragging && Input.GetMouseButton(1))
            {
                Vector2 currentMousePos = Input.mousePosition;
                Vector2 delta = (currentMousePos - lastMousePosition) * mouseSensitivity;

                targetPosition += transform.right * (-delta.x * 0.01f * movementSpeed);
                targetPosition += transform.up * (-delta.y * 0.01f * movementSpeed);

                lastMousePosition = currentMousePos;
            }

            if (enableZoom)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll != 0f)
                {
                    targetZoom -= scroll * zoomSpeed;
                    targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
                }
            }
        }

        private void HandleKeyboardInput()
        {
            if (!enableKeyboardMovement) return;

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (horizontal != 0f || vertical != 0f)
            {
                Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
                targetPosition += transform.forward * (vertical * movementSpeed * Time.deltaTime);
                targetPosition += transform.right * (horizontal * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.Q))
            {
                transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.E))
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.R))
            {
                targetPosition.y += movementSpeed * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.F))
            {
                targetPosition.y -= movementSpeed * Time.deltaTime;
            }
        }

        private void SmoothCameraMovement()
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, xConstraints.x, xConstraints.y);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
            targetPosition.z = Mathf.Clamp(targetPosition.z, zConstraints.x, zConstraints.y);

            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }

        private void SmoothZoom()
        {
            if (mainCamera != null)
            {
                float currentZoom = mainCamera.orthographicSize;
                float newZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, smoothTime);
                mainCamera.orthographicSize = newZoom;
            }
        }

        public void SetCameraEnabled(bool enabled)
        {
            isCameraEnabled = enabled;

            if (!enabled)
            {
                isDragging = false;
            }
        }

        public void ToggleCameraEnabled()
        {
            isCameraEnabled = !isCameraEnabled;
            isDragging = false;
        }

        public bool IsCameraEnabled()
        {
            return isCameraEnabled;
        }

        public void SetTargetPosition(Vector3 position)
        {
            targetPosition = position;
        }

        public void ResetCamera()
        {
            targetPosition = Vector3.zero;
            targetZoom = 5f;
        }

        public void SetMovementSpeed(float speed)
        {
            movementSpeed = Mathf.Max(0.1f, speed);
        }

        public void SetRotationSpeed(float speed)
        {
            rotationSpeed = Mathf.Max(0.1f, speed);
        }

        public void SetZoomSpeed(float speed)
        {
            zoomSpeed = Mathf.Max(0.1f, speed);
        }

        public void SetConstraints(Vector2 xLimits, Vector2 zLimits, float yMin, float yMax)
        {
            xConstraints = xLimits;
            zConstraints = zLimits;
            minY = yMin;
            maxY = yMax;
        }

        public void SetMouseSensitivity(float sensitivity)
        {
            mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 2f);
        }

        public Vector3 GetCurrentPosition()
        {
            return transform.position;
        }

        public float GetCurrentZoom()
        {
            return mainCamera != null ? mainCamera.orthographicSize : 0f;
        }
    }
}