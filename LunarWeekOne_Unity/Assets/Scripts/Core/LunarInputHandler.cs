using UnityEngine;
using UnityEngine.EventSystems;

namespace Lunar.Core
{
    public class LunarInputHandler : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [SerializeField] private float clickDelay = 0.4f;
        [SerializeField] private float interactionCooldown = 0.2f;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private float maxInteractionDistance = 5f;

        private Camera mainCamera;
        private float lastClickTime;
        private float lastInteractionTime;
        private bool isInputEnabled = true;

        private Interactable currentInteractable;

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[LunarInputHandler] Main camera not found");
            }
        }

        private void Update()
        {
            if (!isInputEnabled) return;

            HandleMouseInput();
            HandleKeyboardInput();
        }

        private void HandleMouseInput()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (Time.time - lastClickTime < clickDelay) return;

                lastClickTime = Time.time;
                PerformInteraction();
            }

            UpdateHoverTarget();
        }

        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                LunarExperienceController.Instance?.EndExperience();
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleInputEnabled();
            }

            if (Input.GetKeyDown(KeyCode.Space) && ResourceManager.Instance != null)
            {
                if (Time.time - lastInteractionTime < interactionCooldown) return;

                lastInteractionTime = Time.time;
                ResourceManager.Instance.PerformResourceAction(ResourceType.Energy);
            }

            if (Input.GetKeyDown(KeyCode.R) && RitualEngine.Instance != null)
            {
                if (RitualEngine.Instance.IsRitualActive())
                {
                    RitualEngine.Instance.PerformValveInteraction();
                }
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                LunarDayStateMachine.Instance?.SkipToNextState();
            }
        }

        private void PerformInteraction()
        {
            if (mainCamera == null) return;
            if (Time.time - lastInteractionTime < interactionCooldown) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxInteractionDistance, GetEffectiveLayerMask()))
            {
                GameObject hitObject = hit.collider.gameObject;
                if (ProcessInteraction(hitObject, hit.point))
                {
                    lastInteractionTime = Time.time;
                }
            }
        }

        private bool ProcessInteraction(GameObject target, Vector3 hitPoint)
        {
            if (target == null) return false;

            Interactable interactable = target.GetComponent<Interactable>();
            if (interactable != null)
            {
                interactable.OnInteract(hitPoint);
                return true;
            }

            if (target.CompareTag("Resource_Energy"))
            {
                ResourceManager.Instance?.PerformResourceAction(ResourceType.Energy);
                return true;
            }

            if (target.CompareTag("Resource_Oxygen"))
            {
                ResourceManager.Instance?.PerformResourceAction(ResourceType.Oxygen);
                return true;
            }

            if (target.CompareTag("Resource_Water"))
            {
                ResourceManager.Instance?.PerformResourceAction(ResourceType.Water);
                return true;
            }

            if (target.CompareTag("Ritual_Valve") && RitualEngine.Instance != null)
            {
                if (RitualEngine.Instance.IsRitualActive())
                {
                    RitualEngine.Instance.PerformValveInteraction();
                    return true;
                }

                return false;
            }

            if (target.CompareTag("Ritual_Start"))
            {
                LunarDayStateMachine.Instance?.SkipToNextState();
                return true;
            }

            return false;
        }

        private void UpdateHoverTarget()
        {
            if (mainCamera == null) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxInteractionDistance, GetEffectiveLayerMask()))
            {
                Interactable newInteractable = hit.collider.GetComponent<Interactable>();
                if (newInteractable != currentInteractable)
                {
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnHoverExit();
                    }

                    currentInteractable = newInteractable;
                    if (currentInteractable != null)
                    {
                        currentInteractable.OnHoverEnter();
                    }
                }
            }
            else if (currentInteractable != null)
            {
                currentInteractable.OnHoverExit();
                currentInteractable = null;
            }
        }

        public void SetInputEnabled(bool enabled)
        {
            isInputEnabled = enabled;
        }

        public void ToggleInputEnabled()
        {
            isInputEnabled = !isInputEnabled;
        }

        public bool IsInputEnabled()
        {
            return isInputEnabled;
        }

        public void SetClickDelay(float delay)
        {
            clickDelay = Mathf.Max(0.1f, delay);
        }

        public float GetCurrentInteractionDelay()
        {
            return clickDelay;
        }

        private LayerMask GetEffectiveLayerMask()
        {
            return interactableLayer.value == 0 ? Physics.DefaultRaycastLayers : interactableLayer;
        }
    }

    public abstract class Interactable : MonoBehaviour
    {
        public abstract void OnInteract(Vector3 hitPoint);
        public virtual void OnHoverEnter() { }
        public virtual void OnHoverExit() { }
    }

    public class ResourceInteractable : Interactable
    {
        [SerializeField] private ResourceType resourceType;
        [SerializeField] private Color highlightColor = Color.yellow;

        private Material originalMaterial;
        private Material highlightMaterial;
        private Renderer objectRenderer;

        private void Start()
        {
            objectRenderer = GetComponent<Renderer>();
            if (objectRenderer != null)
            {
                originalMaterial = objectRenderer.material;
            }

            highlightMaterial = new Material(Shader.Find("Standard"));
            highlightMaterial.color = highlightColor;
            highlightMaterial.EnableKeyword("_EMISSION");
            highlightMaterial.SetColor("_EmissionColor", highlightColor * 0.5f);
        }

        public override void OnInteract(Vector3 hitPoint)
        {
            ResourceManager.Instance?.PerformResourceAction(resourceType);
            StartCoroutine(FlashFeedback());
        }

        private System.Collections.IEnumerator FlashFeedback()
        {
            if (objectRenderer != null && highlightMaterial != null)
            {
                objectRenderer.material = highlightMaterial;
                yield return new WaitForSeconds(0.2f);
                objectRenderer.material = originalMaterial;
            }
        }

        public override void OnHoverEnter()
        {
            if (objectRenderer != null && highlightMaterial != null)
            {
                objectRenderer.material = highlightMaterial;
            }
        }

        public override void OnHoverExit()
        {
            if (objectRenderer != null && originalMaterial != null)
            {
                objectRenderer.material = originalMaterial;
            }
        }

        public void Configure(ResourceType type, Color color)
        {
            resourceType = type;
            highlightColor = color;
        }
    }

    public class RitualValveInteractable : Interactable
    {
        private Animator animator;
        private bool isValveOpen = false;
        private Vector3 originalScale;

        private void Start()
        {
            animator = GetComponent<Animator>();
            originalScale = transform.localScale;
        }

        public override void OnInteract(Vector3 hitPoint)
        {
            if (RitualEngine.Instance != null && RitualEngine.Instance.IsRitualActive())
            {
                RitualEngine.Instance.PerformValveInteraction();
                ToggleValve();
            }
        }

        private void ToggleValve()
        {
            isValveOpen = !isValveOpen;
            if (animator != null)
            {
                animator.SetBool("IsOpen", isValveOpen);
            }
        }

        public override void OnHoverEnter()
        {
            transform.localScale = originalScale * 1.1f;
        }

        public override void OnHoverExit()
        {
            transform.localScale = originalScale;
        }
    }
}
