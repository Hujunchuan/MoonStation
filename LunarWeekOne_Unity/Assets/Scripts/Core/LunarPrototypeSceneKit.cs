using System.Collections.Generic;
using Lunar.Data;
using UnityEngine;

namespace Lunar.Core
{
    public class LunarPrototypeReferenceHub : MonoBehaviour
    {
        [SerializeField] private Transform playerAnchor;
        [SerializeField] private Light baseStatusLight;
        [SerializeField] private Light[] interiorLights;
        [SerializeField] private Light ritualAmbientLight;
        [SerializeField] private GameObject ritualIndicator;
        [SerializeField] private Renderer ritualValveRenderer;
        [SerializeField] private Renderer[] resourceRenderers;
        [SerializeField] private ParticleSystem dustParticles;
        [SerializeField] private ParticleSystem anomalyParticles;
        [SerializeField] private Transform operationsDisplayAnchor;
        [SerializeField] private Transform quietDeckDisplayAnchor;
        [SerializeField] private Transform resourceFocusAnchor;
        [SerializeField] private Transform ritualFocusAnchor;
        [SerializeField] private Transform quietDeckFocusAnchor;
        [SerializeField] private Renderer[] corridorGuideRenderers;
        [SerializeField] private Renderer[] resourceGuideRenderers;
        [SerializeField] private Renderer[] ritualGuideRenderers;
        [SerializeField] private Renderer operationsScreenRenderer;
        [SerializeField] private Renderer breathPanelRenderer;
        [SerializeField] private Renderer observationGlassRenderer;
        [SerializeField] private Renderer earthriseRenderer;

        public Transform PlayerAnchor => playerAnchor;
        public Light BaseStatusLight => baseStatusLight;
        public Light[] InteriorLights => interiorLights;
        public Light RitualAmbientLight => ritualAmbientLight;
        public GameObject RitualIndicator => ritualIndicator;
        public Renderer RitualValveRenderer => ritualValveRenderer;
        public Renderer[] ResourceRenderers => resourceRenderers;
        public ParticleSystem DustParticles => dustParticles;
        public ParticleSystem AnomalyParticles => anomalyParticles;
        public Transform OperationsDisplayAnchor => operationsDisplayAnchor;
        public Transform QuietDeckDisplayAnchor => quietDeckDisplayAnchor;
        public Transform ResourceFocusAnchor => resourceFocusAnchor;
        public Transform RitualFocusAnchor => ritualFocusAnchor;
        public Transform QuietDeckFocusAnchor => quietDeckFocusAnchor;
        public Renderer[] CorridorGuideRenderers => corridorGuideRenderers;
        public Renderer[] ResourceGuideRenderers => resourceGuideRenderers;
        public Renderer[] RitualGuideRenderers => ritualGuideRenderers;
        public Renderer OperationsScreenRenderer => operationsScreenRenderer;
        public Renderer BreathPanelRenderer => breathPanelRenderer;
        public Renderer ObservationGlassRenderer => observationGlassRenderer;
        public Renderer EarthriseRenderer => earthriseRenderer;

        public void Configure(
            Transform anchor,
            Light statusLight,
            Light[] shellLights,
            Light ritualLight,
            GameObject indicator,
            Renderer valveRenderer,
            Renderer[] resourceNodeRenderers,
            ParticleSystem dust,
            ParticleSystem anomaly,
            Transform operationsAnchor,
            Transform quietAnchor,
            Transform resourceAnchor,
            Transform ritualAnchor,
            Transform quietFocusAnchor,
            Renderer[] corridorGuides,
            Renderer[] resourceGuides,
            Renderer[] ritualGuides,
            Renderer operationsScreen,
            Renderer breathPanel,
            Renderer observationGlass,
            Renderer earthrise)
        {
            playerAnchor = anchor;
            baseStatusLight = statusLight;
            interiorLights = shellLights;
            ritualAmbientLight = ritualLight;
            ritualIndicator = indicator;
            ritualValveRenderer = valveRenderer;
            resourceRenderers = resourceNodeRenderers;
            dustParticles = dust;
            anomalyParticles = anomaly;
            operationsDisplayAnchor = operationsAnchor;
            quietDeckDisplayAnchor = quietAnchor;
            resourceFocusAnchor = resourceAnchor;
            ritualFocusAnchor = ritualAnchor;
            quietDeckFocusAnchor = quietFocusAnchor;
            corridorGuideRenderers = corridorGuides;
            resourceGuideRenderers = resourceGuides;
            ritualGuideRenderers = ritualGuides;
            operationsScreenRenderer = operationsScreen;
            breathPanelRenderer = breathPanel;
            observationGlassRenderer = observationGlass;
            earthriseRenderer = earthrise;
        }
    }

    public static class LunarPrototypeSceneKit
    {
        public const string ShellRootName = "Prototype Greybox Shell";
        private const string WindowGlassName = "Observation Glass";

        public static LunarPrototypeReferenceHub EnsureGreyboxShell(Transform parent = null)
        {
            LunarPrototypeReferenceHub existingHub = Object.FindObjectOfType<LunarPrototypeReferenceHub>();
            if (existingHub != null)
            {
                return existingHub;
            }

            GameObject root = new GameObject(ShellRootName);
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            Material hullMaterial = CreateLitMaterial("PrototypeHull", new Color(0.18f, 0.21f, 0.27f), 0.05f);
            Material trimMaterial = CreateLitMaterial("PrototypeTrim", new Color(0.31f, 0.34f, 0.4f), 0.08f);
            Material glassMaterial = CreateLitMaterial("PrototypeGlass", new Color(0.22f, 0.42f, 0.52f, 0.55f), 0.18f);
            Material deckMaterial = CreateLitMaterial("PrototypeDeck", new Color(0.12f, 0.13f, 0.17f), 0.02f);
            Material regolithMaterial = CreateLitMaterial("PrototypeRegolith", new Color(0.57f, 0.56f, 0.58f), 0.01f);

            Transform shell = root.transform;
            Transform anchor = CreateAnchor(shell);
            Light baseStatusLight = CreateStatusLight(shell);
            List<Light> interiorLights = CreateInteriorLights(shell);
            Light ritualLight = CreateRitualLight(shell);
            Renderer operationsScreenRenderer;
            Renderer breathPanelRenderer;
            Renderer observationGlassRenderer;
            Renderer earthriseRenderer;

            CreateCorridor(shell, hullMaterial, trimMaterial, deckMaterial);
            Renderer[] corridorGuideRenderers = CreateCorridorGuides(shell);
            Transform resourceFocusAnchor = CreateFocusAnchor(shell, "Resource Focus Anchor", new Vector3(0f, 2.05f, -2.25f), Quaternion.Euler(10f, 0f, 0f));
            Transform operationsDisplayAnchor = CreateOperationsDisplayMount(shell, trimMaterial, glassMaterial, out operationsScreenRenderer);
            CreateSideBay(shell, "Energy Bay", new Vector3(-4.6f, 0f, 1.6f), hullMaterial, trimMaterial);
            CreateSideBay(shell, "Oxygen Bay", new Vector3(0f, 0f, 1.6f), hullMaterial, trimMaterial);
            CreateSideBay(shell, "Water Bay", new Vector3(4.6f, 0f, 1.6f), hullMaterial, trimMaterial);
            Transform quietDeckDisplayAnchor = CreateQuietDeck(shell, hullMaterial, trimMaterial, glassMaterial, out breathPanelRenderer);
            Transform quietDeckFocusAnchor = CreateFocusAnchor(shell, "Quiet Deck Focus Anchor", new Vector3(-1.6f, 2f, 6f), Quaternion.Euler(11f, -30f, 0f));
            CreateObservationDeck(shell, trimMaterial, glassMaterial, regolithMaterial, out observationGlassRenderer, out earthriseRenderer);
            Renderer[] resourceGuideRenderers = CreateResourceGuides(shell);

            Renderer[] resourceRenderers =
            {
                CreateResourceStation(shell, "Energy", ResourceType.Energy, new Vector3(-4.6f, 1.35f, 1.6f), new Color(1f, 0.82f, 0.28f), trimMaterial),
                CreateResourceStation(shell, "Oxygen", ResourceType.Oxygen, new Vector3(0f, 1.35f, 1.6f), new Color(0.45f, 0.82f, 1f), trimMaterial),
                CreateResourceStation(shell, "Water", ResourceType.Water, new Vector3(4.6f, 1.35f, 1.6f), new Color(0.28f, 0.92f, 0.72f), trimMaterial)
            };

            GameObject ritualIndicator = CreateRitualIndicator(shell, ritualLight);
            Renderer valveRenderer = CreateRitualChamber(shell, hullMaterial, trimMaterial);
            Transform ritualFocusAnchor = CreateFocusAnchor(shell, "Ritual Focus Anchor", new Vector3(0f, 1.95f, 3.25f), Quaternion.Euler(8f, 0f, 0f));
            Renderer[] ritualGuideRenderers = CreateRitualGuides(shell);
            ParticleSystem dustParticles = CreateDustParticles(shell, new Vector3(0f, 1.9f, 8.8f), new Vector3(6f, 2.5f, 0.8f), new Color(0.78f, 0.84f, 1f, 0.18f), "Dust Particles");
            ParticleSystem anomalyParticles = CreateDustParticles(shell, new Vector3(0f, 1.4f, 5.2f), new Vector3(4f, 2f, 6f), new Color(1f, 0.38f, 0.22f, 0.28f), "Anomaly Particles");
            anomalyParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            if (observationGlassRenderer != null)
            {
                observationGlassRenderer.material = glassMaterial;
            }

            LunarPrototypeReferenceHub hub = root.AddComponent<LunarPrototypeReferenceHub>();
            hub.Configure(
                anchor,
                baseStatusLight,
                interiorLights.ToArray(),
                ritualLight,
                ritualIndicator,
                valveRenderer,
                resourceRenderers,
                dustParticles,
                anomalyParticles,
                operationsDisplayAnchor,
                quietDeckDisplayAnchor,
                resourceFocusAnchor,
                ritualFocusAnchor,
                quietDeckFocusAnchor,
                corridorGuideRenderers,
                resourceGuideRenderers,
                ritualGuideRenderers,
                operationsScreenRenderer,
                breathPanelRenderer,
                observationGlassRenderer,
                earthriseRenderer);

            return hub;
        }

        private static Transform CreateAnchor(Transform parent)
        {
            GameObject anchor = new GameObject("Player Anchor");
            anchor.transform.SetParent(parent, false);
            anchor.transform.localPosition = new Vector3(0f, 1.7f, -3.5f);
            anchor.transform.localRotation = Quaternion.identity;
            return anchor.transform;
        }

        private static void CreateCorridor(Transform parent, Material hullMaterial, Material trimMaterial, Material deckMaterial)
        {
            CreateBox(parent, "Deck", new Vector3(0f, -0.1f, 2.5f), new Vector3(14f, 0.24f, 22f), deckMaterial);
            CreateBox(parent, "Ceiling", new Vector3(0f, 3.15f, 2.5f), new Vector3(14f, 0.24f, 22f), hullMaterial);
            CreateBox(parent, "Left Wall", new Vector3(-7f, 1.5f, 2.5f), new Vector3(0.24f, 3f, 22f), hullMaterial);
            CreateBox(parent, "Right Wall", new Vector3(7f, 1.5f, 2.5f), new Vector3(0.24f, 3f, 22f), hullMaterial);
            CreateBox(parent, "Rear Bulkhead", new Vector3(0f, 1.5f, -8.5f), new Vector3(14f, 3f, 0.24f), hullMaterial);
            CreateBox(parent, "Center Walkway", new Vector3(0f, 0.04f, 2.4f), new Vector3(3.2f, 0.06f, 19f), trimMaterial);
            CreateBox(parent, "Center Rail Left", new Vector3(-1.75f, 0.55f, 2.4f), new Vector3(0.08f, 1f, 19f), trimMaterial);
            CreateBox(parent, "Center Rail Right", new Vector3(1.75f, 0.55f, 2.4f), new Vector3(0.08f, 1f, 19f), trimMaterial);
        }

        private static Renderer[] CreateCorridorGuides(Transform parent)
        {
            return new[]
            {
                CreateGuideStrip(parent, "Corridor Guide 1", new Vector3(0f, 0.05f, -3.8f), new Vector3(0.82f, 0.03f, 1.8f), new Color(0.22f, 0.62f, 0.78f)),
                CreateGuideStrip(parent, "Corridor Guide 2", new Vector3(0f, 0.05f, 0.1f), new Vector3(0.82f, 0.03f, 1.8f), new Color(0.22f, 0.62f, 0.78f)),
                CreateGuideStrip(parent, "Corridor Guide 3", new Vector3(0f, 0.05f, 4.1f), new Vector3(0.82f, 0.03f, 1.8f), new Color(0.22f, 0.62f, 0.78f)),
                CreateGuideStrip(parent, "Corridor Guide 4", new Vector3(0f, 0.05f, 8.1f), new Vector3(0.82f, 0.03f, 1.8f), new Color(0.22f, 0.62f, 0.78f))
            };
        }

        private static void CreateSideBay(Transform parent, string name, Vector3 center, Material hullMaterial, Material trimMaterial)
        {
            GameObject bay = new GameObject(name);
            bay.transform.SetParent(parent, false);
            bay.transform.localPosition = center;

            CreateBox(bay.transform, "Rear Wall", new Vector3(0f, 1.2f, 1.65f), new Vector3(3.1f, 2.4f, 0.18f), hullMaterial);
            CreateBox(bay.transform, "Left Column", new Vector3(-1.45f, 1.2f, 0f), new Vector3(0.22f, 2.4f, 3.3f), hullMaterial);
            CreateBox(bay.transform, "Right Column", new Vector3(1.45f, 1.2f, 0f), new Vector3(0.22f, 2.4f, 3.3f), hullMaterial);
            CreateBox(bay.transform, "Header", new Vector3(0f, 2.4f, 0f), new Vector3(3.1f, 0.18f, 3.3f), hullMaterial);
            CreateBox(bay.transform, "Apron", new Vector3(0f, 0.01f, 0f), new Vector3(2.4f, 0.04f, 2.5f), trimMaterial);
        }

        private static Renderer CreateResourceStation(Transform parent, string label, ResourceType resourceType, Vector3 nodePosition, Color nodeColor, Material pedestalMaterial)
        {
            GameObject stationRoot = new GameObject($"{label} Station");
            stationRoot.transform.SetParent(parent, false);
            stationRoot.transform.localPosition = new Vector3(nodePosition.x, 0f, nodePosition.z);

            CreateBox(stationRoot.transform, "Pedestal Base", new Vector3(0f, 0.3f, 0f), new Vector3(1.2f, 0.6f, 1.2f), pedestalMaterial);
            CreateBox(stationRoot.transform, "Pedestal Neck", new Vector3(0f, 0.85f, 0f), new Vector3(0.5f, 0.5f, 0.5f), pedestalMaterial);
            CreateTextMesh(stationRoot.transform, label.ToUpperInvariant(), new Vector3(0f, 1.95f, -0.65f), 0.11f, new Color(0.84f, 0.9f, 0.98f));

            GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            node.name = $"{label} Node";
            node.transform.SetParent(stationRoot.transform, false);
            node.transform.localPosition = new Vector3(0f, 1.28f, 0f);
            node.transform.localScale = new Vector3(0.55f, 0.36f, 0.55f);
            node.AddComponent<ResourceInteractable>().Configure(resourceType, nodeColor);

            Renderer renderer = node.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material nodeMaterial = CreateLitMaterial($"{label} Node Material", nodeColor, 0.12f);
                nodeMaterial.EnableKeyword("_EMISSION");
                nodeMaterial.SetColor("_EmissionColor", nodeColor * 1.5f);
                renderer.material = nodeMaterial;
            }

            return renderer;
        }

        private static Renderer[] CreateResourceGuides(Transform parent)
        {
            return new[]
            {
                CreateGuideStrip(parent, "Energy Guide", new Vector3(-3.05f, 0.05f, 1.1f), new Vector3(2.2f, 0.03f, 0.26f), new Color(0.95f, 0.72f, 0.18f)),
                CreateGuideStrip(parent, "Oxygen Guide", new Vector3(0f, 0.05f, 1.1f), new Vector3(2.2f, 0.03f, 0.26f), new Color(0.4f, 0.82f, 1f)),
                CreateGuideStrip(parent, "Water Guide", new Vector3(3.05f, 0.05f, 1.1f), new Vector3(2.2f, 0.03f, 0.26f), new Color(0.22f, 0.9f, 0.72f))
            };
        }

        private static Renderer CreateRitualChamber(Transform parent, Material hullMaterial, Material trimMaterial)
        {
            GameObject chamber = new GameObject("Ritual Chamber");
            chamber.transform.SetParent(parent, false);
            chamber.transform.localPosition = new Vector3(0f, 0f, 7.4f);

            CreateBox(chamber.transform, "Rear Arch", new Vector3(0f, 1.25f, 1.1f), new Vector3(4.6f, 2.5f, 0.18f), hullMaterial);
            CreateBox(chamber.transform, "Left Arch", new Vector3(-2.2f, 1.25f, 0f), new Vector3(0.18f, 2.5f, 2.2f), hullMaterial);
            CreateBox(chamber.transform, "Right Arch", new Vector3(2.2f, 1.25f, 0f), new Vector3(0.18f, 2.5f, 2.2f), hullMaterial);
            CreateBox(chamber.transform, "Ring Base", new Vector3(0f, 0.04f, 0f), new Vector3(4.2f, 0.08f, 3.4f), trimMaterial);
            CreateBox(chamber.transform, "Control Plinth", new Vector3(0f, 0.54f, -0.65f), new Vector3(1.8f, 1.08f, 0.72f), trimMaterial);

            GameObject valve = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            valve.name = "Ritual Valve";
            valve.transform.SetParent(chamber.transform, false);
            valve.transform.localPosition = new Vector3(0f, 1.05f, -0.65f);
            valve.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            valve.transform.localScale = new Vector3(0.85f, 0.1f, 0.85f);
            valve.AddComponent<RitualValveInteractable>();

            Renderer valveRenderer = valve.GetComponent<Renderer>();
            if (valveRenderer != null)
            {
                Material valveMaterial = CreateLitMaterial("RitualValveMaterial", new Color(0.3f, 0.74f, 0.92f), 0.18f);
                valveMaterial.EnableKeyword("_EMISSION");
                valveMaterial.SetColor("_EmissionColor", new Color(0.24f, 0.92f, 1f) * 0.4f);
                valveRenderer.material = valveMaterial;
            }

            CreateTextMesh(chamber.transform, "QUIET DECK", new Vector3(0f, 2.38f, 0.1f), 0.13f, new Color(0.8f, 0.87f, 0.98f));
            return valveRenderer;
        }

        private static Renderer[] CreateRitualGuides(Transform parent)
        {
            return new[]
            {
                CreateGuideStrip(parent, "Ritual Guide 1", new Vector3(0f, 0.05f, 4.9f), new Vector3(0.92f, 0.03f, 1.15f), new Color(0.28f, 0.84f, 1f)),
                CreateGuideStrip(parent, "Ritual Guide 2", new Vector3(0f, 0.05f, 6.4f), new Vector3(0.92f, 0.03f, 1.15f), new Color(0.28f, 0.84f, 1f)),
                CreateGuideStrip(parent, "Ritual Guide 3", new Vector3(0f, 0.05f, 7.95f), new Vector3(1.35f, 0.03f, 0.9f), new Color(0.28f, 0.84f, 1f))
            };
        }

        private static GameObject CreateRitualIndicator(Transform parent, Light ritualLight)
        {
            GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.name = "Ritual Indicator";
            indicator.transform.SetParent(parent, false);
            indicator.transform.localPosition = new Vector3(0f, 2.45f, 7.05f);
            indicator.transform.localScale = new Vector3(0.28f, 0.28f, 0.28f);

            Renderer renderer = indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material indicatorMaterial = CreateLitMaterial("RitualIndicatorMaterial", new Color(0.35f, 0.92f, 1f), 0.25f);
                indicatorMaterial.EnableKeyword("_EMISSION");
                indicatorMaterial.SetColor("_EmissionColor", new Color(0.25f, 0.8f, 1f) * 2f);
                renderer.material = indicatorMaterial;
            }

            if (ritualLight != null)
            {
                ritualLight.transform.SetParent(indicator.transform, true);
            }

            return indicator;
        }

        private static Transform CreateOperationsDisplayMount(Transform parent, Material trimMaterial, Material glassMaterial, out Renderer displayRenderer)
        {
            CreateBox(parent, "Operations Screen Housing", new Vector3(6.72f, 1.55f, -0.8f), new Vector3(0.2f, 1.85f, 2.8f), trimMaterial);
            GameObject screen = CreateBox(parent, "Operations Screen Surface", new Vector3(6.62f, 1.55f, -0.8f), new Vector3(0.04f, 1.56f, 2.45f), glassMaterial);
            Renderer screenRenderer = screen.GetComponent<Renderer>();
            if (screenRenderer != null)
            {
                screenRenderer.material.EnableKeyword("_EMISSION");
                screenRenderer.material.SetColor("_EmissionColor", new Color(0.08f, 0.22f, 0.32f));
            }

            displayRenderer = screenRenderer;
            return CreateDisplayAnchor(parent, "Operations Display Anchor", new Vector3(6.58f, 1.55f, -0.8f), Quaternion.Euler(0f, -90f, 0f));
        }

        private static Transform CreateQuietDeck(Transform parent, Material hullMaterial, Material trimMaterial, Material glassMaterial, out Renderer breathRenderer)
        {
            GameObject quietDeck = new GameObject("Quiet Deck");
            quietDeck.transform.SetParent(parent, false);
            quietDeck.transform.localPosition = new Vector3(-5f, 0f, 6.6f);

            CreateBox(quietDeck.transform, "Bench Base", new Vector3(0f, 0.34f, 0f), new Vector3(2.4f, 0.24f, 0.7f), trimMaterial);
            CreateBox(quietDeck.transform, "Bench Back", new Vector3(0f, 0.9f, -0.27f), new Vector3(2.4f, 0.9f, 0.14f), trimMaterial);
            CreateBox(quietDeck.transform, "Observation Shelf", new Vector3(0f, 1.15f, 0.95f), new Vector3(2.7f, 0.08f, 0.42f), hullMaterial);
            CreateTextMesh(quietDeck.transform, "REST / OBSERVE", new Vector3(0f, 1.95f, 0.95f), 0.1f, new Color(0.82f, 0.88f, 0.96f));

            GameObject panel = CreateBox(quietDeck.transform, "Breath Panel", new Vector3(0f, 1.45f, 0.78f), new Vector3(1.4f, 0.7f, 0.08f), glassMaterial);
            Renderer panelRenderer = panel.GetComponent<Renderer>();
            if (panelRenderer != null)
            {
                panelRenderer.material.EnableKeyword("_EMISSION");
                panelRenderer.material.SetColor("_EmissionColor", new Color(0.12f, 0.34f, 0.4f));
            }

            breathRenderer = panelRenderer;
            return CreateDisplayAnchor(quietDeck.transform, "Quiet Display Anchor", new Vector3(0f, 1.45f, 0.73f), Quaternion.Euler(0f, 180f, 0f));
        }

        private static void CreateObservationDeck(
            Transform parent,
            Material trimMaterial,
            Material glassMaterial,
            Material regolithMaterial,
            out Renderer glassRenderer,
            out Renderer earthRenderer)
        {
            GameObject deck = new GameObject("Observation Deck");
            deck.transform.SetParent(parent, false);
            deck.transform.localPosition = new Vector3(0f, 0f, 10.2f);

            CreateBox(deck.transform, "Frame Left", new Vector3(-2.95f, 1.5f, 0f), new Vector3(0.18f, 3f, 0.24f), trimMaterial);
            CreateBox(deck.transform, "Frame Right", new Vector3(2.95f, 1.5f, 0f), new Vector3(0.18f, 3f, 0.24f), trimMaterial);
            CreateBox(deck.transform, "Frame Top", new Vector3(0f, 2.95f, 0f), new Vector3(6.1f, 0.18f, 0.24f), trimMaterial);
            CreateBox(deck.transform, "Frame Bottom", new Vector3(0f, 0.22f, 0f), new Vector3(6.1f, 0.18f, 0.24f), trimMaterial);

            GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Quad);
            glass.name = WindowGlassName;
            glass.transform.SetParent(deck.transform, false);
            glass.transform.localPosition = new Vector3(0f, 1.55f, -0.02f);
            glass.transform.localScale = new Vector3(5.55f, 2.55f, 1f);
            glass.transform.localRotation = Quaternion.identity;
            glassRenderer = glass.GetComponent<Renderer>();
            if (glassRenderer != null)
            {
                glassRenderer.material = glassMaterial;
            }

            GameObject moonSurface = CreateBox(deck.transform, "Moon Surface", new Vector3(0f, -1.1f, 5f), new Vector3(20f, 0.18f, 14f), regolithMaterial);
            moonSurface.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
            GameObject crater = CreateBox(deck.transform, "Crater Ridge", new Vector3(2.6f, -0.5f, 4.2f), new Vector3(4.2f, 0.28f, 2.4f), regolithMaterial);
            crater.transform.localRotation = Quaternion.Euler(18f, -8f, 0f);

            GameObject earth = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            earth.name = "Earthrise Marker";
            earth.transform.SetParent(deck.transform, false);
            earth.transform.localPosition = new Vector3(4.2f, 2.75f, 10.2f);
            earth.transform.localScale = Vector3.one * 1.35f;
            earthRenderer = earth.GetComponent<Renderer>();
            if (earthRenderer != null)
            {
                Material earthMaterial = CreateLitMaterial("EarthriseMaterial", new Color(0.24f, 0.52f, 0.9f), 0.22f);
                earthMaterial.EnableKeyword("_EMISSION");
                earthMaterial.SetColor("_EmissionColor", new Color(0.08f, 0.18f, 0.3f));
                earthRenderer.material = earthMaterial;
            }
        }

        private static Light CreateStatusLight(Transform parent)
        {
            GameObject lightObject = new GameObject("Base Status Light");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = new Vector3(0f, 2.6f, -1.5f);

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 18f;
            light.intensity = 1.1f;
            light.color = new Color(0.85f, 0.9f, 1f);
            return light;
        }

        private static List<Light> CreateInteriorLights(Transform parent)
        {
            var lights = new List<Light>();
            float[] zPositions = { -4.6f, 0.4f, 5.2f };

            for (int index = 0; index < zPositions.Length; index++)
            {
                GameObject lightObject = new GameObject($"Interior Light {index + 1}");
                lightObject.transform.SetParent(parent, false);
                lightObject.transform.localPosition = new Vector3(0f, 2.75f, zPositions[index]);

                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 12f;
                light.intensity = 1f;
                light.color = new Color(0.86f, 0.9f, 1f);
                lights.Add(light);
            }

            return lights;
        }

        private static Light CreateRitualLight(Transform parent)
        {
            GameObject lightObject = new GameObject("Ritual Ambient Light");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = new Vector3(0f, 2.2f, 7.1f);

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 8f;
            light.intensity = 0.3f;
            light.color = new Color(0.24f, 0.78f, 1f);
            return light;
        }

        private static ParticleSystem CreateDustParticles(Transform parent, Vector3 position, Vector3 boxSize, Color color, string name)
        {
            GameObject particleObject = new GameObject(name);
            particleObject.transform.SetParent(parent, false);
            particleObject.transform.localPosition = position;

            ParticleSystem system = particleObject.AddComponent<ParticleSystem>();
            var main = system.main;
            main.loop = true;
            main.startLifetime = 6f;
            main.startSpeed = 0.08f;
            main.startSize = 0.06f;
            main.maxParticles = 180;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;

            var emission = system.emission;
            emission.rateOverTime = 12f;

            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = boxSize;

            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.x = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.015f, 0.015f);
            velocity.z = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);

            var noise = system.noise;
            noise.enabled = true;
            noise.strength = 0.08f;
            noise.frequency = 0.25f;

            system.Play();
            return system;
        }

        private static Renderer CreateGuideStrip(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color)
        {
            GameObject strip = CreateBox(parent, name, localPosition, localScale, CreateLitMaterial($"{name} Material", color, 0.06f));
            Renderer renderer = strip.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.EnableKeyword("_EMISSION");
                renderer.material.SetColor("_EmissionColor", color * 0.12f);
            }

            return renderer;
        }

        private static Transform CreateDisplayAnchor(Transform parent, string name, Vector3 localPosition, Quaternion localRotation)
        {
            GameObject anchor = new GameObject(name);
            anchor.transform.SetParent(parent, false);
            anchor.transform.localPosition = localPosition;
            anchor.transform.localRotation = localRotation;
            return anchor.transform;
        }

        private static Transform CreateFocusAnchor(Transform parent, string name, Vector3 localPosition, Quaternion localRotation)
        {
            GameObject anchor = new GameObject(name);
            anchor.transform.SetParent(parent, false);
            anchor.transform.localPosition = localPosition;
            anchor.transform.localRotation = localRotation;
            return anchor.transform;
        }

        private static GameObject CreateBox(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = localPosition;
            box.transform.localScale = localScale;

            Renderer renderer = box.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.material = material;
            }

            return box;
        }

        private static void CreateTextMesh(Transform parent, string text, Vector3 localPosition, float characterSize, Color color)
        {
            GameObject textObject = new GameObject($"{text} Label");
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = localPosition;

            TextMesh mesh = textObject.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.characterSize = characterSize;
            mesh.fontSize = 48;
            mesh.color = color;
        }

        private static Material CreateLitMaterial(string name, Color color, float metallic)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.name = name;
            material.color = color;
            material.SetFloat("_Glossiness", 0.28f);
            material.SetFloat("_Metallic", metallic);
            return material;
        }
    }
}
