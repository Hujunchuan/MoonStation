using UnityEngine;

namespace Lunar.Core
{
    public class LunarCameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float movementSpeed = 2f;
        [SerializeField] private float rotationSpeed = 60f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float smoothTime = 0.12f;

        [Header("Constraints")]
        [SerializeField] private float minZoom = 20f;
        [SerializeField] private float maxZoom = 70f;
        [SerializeField] private float minY = 0.5f;
        [SerializeField] private float maxY = 5f;
        [SerializeField] private Vector2 xConstraints = new Vector2(-10f, 10f);
        [SerializeField] private Vector2 zConstraints = new Vector2(-10f, 10f);

        [Header("Input Settings")]
        [SerializeField] private bool enableMouseMovement = true;
        [SerializeField] private bool enableKeyboardMovement = true;
        [SerializeField] private bool enableZoom = true;
        [SerializeField] private float mouseSensitivity = 0.5f;

        private Camera controlledCamera;
        private Vector3 targetPosition;
        private Vector3 velocity;
        private float targetZoom;
        private float zoomVelocity;
        private bool isCameraEnabled = true;
        private bool isDragging;
        private Vector2 lastMousePosition;

        private void Start()
        {
            controlledCamera = GetComponent<Camera>();
            if (controlledCamera == null)
            {
                controlledCamera = Camera.main;
            }

            targetPosition = transform.position;
            targetZoom = GetCurrentZoom();
            Cursor.lockState = CursorLockMode.Confined;
        }

        private void Update()
        {
            if (!isCameraEnabled)
            {
                return;
            }

            HandleMouseInput();
            HandleKeyboardInput();
            ApplyMovement();
            ApplyZoom();
        }

        private void HandleMouseInput()
        {
            if (!enableMouseMovement)
            {
                return;
            }

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
                Vector2 currentMousePosition = Input.mousePosition;
                Vector2 delta = (currentMousePosition - lastMousePosition) * mouseSensitivity;

                targetPosition += -transform.right * (delta.x * 0.01f * movementSpeed);
                targetPosition += -transform.up * (delta.y * 0.01f * movementSpeed);
                lastMousePosition = currentMousePosition;
            }

            if (enableZoom)
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (!Mathf.Approximately(scroll, 0f))
                {
                    targetZoom = Mathf.Clamp(targetZoom - (scroll * zoomSpeed * 10f), minZoom, maxZoom);
                }
            }
        }

        private void HandleKeyboardInput()
        {
            if (!enableKeyboardMovement)
            {
                return;
            }

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            if (!Mathf.Approximately(horizontal, 0f) || !Mathf.Approximately(vertical, 0f))
            {
                Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
                targetPosition += (forward * vertical + right * horizontal) * (movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.Q))
            {
                transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.World);
            }

            if (Input.GetKey(KeyCode.E))
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
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

        private void ApplyMovement()
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, xConstraints.x, xConstraints.y);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
            targetPosition.z = Mathf.Clamp(targetPosition.z, zConstraints.x, zConstraints.y);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }

        private void ApplyZoom()
        {
            if (controlledCamera == null)
            {
                return;
            }

            float currentZoom = GetCurrentZoom();
            float smoothedZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, smoothTime);

            if (controlledCamera.orthographic)
            {
                controlledCamera.orthographicSize = smoothedZoom;
            }
            else
            {
                controlledCamera.fieldOfView = smoothedZoom;
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

        public bool IsCameraEnabled()
        {
            return isCameraEnabled;
        }

        public void ResetCamera()
        {
            targetPosition = Vector3.zero;
            targetZoom = controlledCamera != null && controlledCamera.orthographic ? 5f : 60f;
        }

        public void SetMovementSpeed(float speed)
        {
            movementSpeed = Mathf.Max(0.1f, speed);
        }

        public void SetRotationSpeed(float speed)
        {
            rotationSpeed = Mathf.Max(1f, speed);
        }

        public void SetZoomSpeed(float speed)
        {
            zoomSpeed = Mathf.Max(0.1f, speed);
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
            if (controlledCamera == null)
            {
                return 60f;
            }

            return controlledCamera.orthographic
                ? controlledCamera.orthographicSize
                : controlledCamera.fieldOfView;
        }
    }
}
