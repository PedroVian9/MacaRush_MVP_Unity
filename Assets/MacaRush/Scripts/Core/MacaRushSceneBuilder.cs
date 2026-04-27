using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MacaRush
{
    public sealed class MacaRushSceneBuilder : MonoBehaviour
    {
        [Header("Build")]
        [SerializeField] private bool clearPreviousGeneratedScene = true;
        [SerializeField] private string generatedRootName = "MacaRush_PrototypeScene";
        [SerializeField] private bool reuseSceneMainCamera = true;

        private readonly List<AutoDoor> createdDoors = new List<AutoDoor>();
        private readonly List<MovingObstacle> createdMovingObstacles = new List<MovingObstacle>();
        private readonly List<SlipperyZone> createdSlipperyZones = new List<SlipperyZone>();
        private readonly List<Light> createdLights = new List<Light>();

        [ContextMenu("Build Prototype Scene")]
        public void Build()
        {
            if (clearPreviousGeneratedScene)
            {
                ClearPreviousScene();
            }

            createdDoors.Clear();
            createdMovingObstacles.Clear();
            createdSlipperyZones.Clear();
            createdLights.Clear();

            var root = new GameObject(generatedRootName);
            var materials = SceneMaterials.Create();

            BuildHospital(root.transform, materials);
            BuildRouteChoice(root.transform, materials);
            BuildStreet(root.transform, materials);
            BuildAmbulance(root.transform, materials);

            var stretcher = CreateStretcher(root.transform, materials, out var patient);
            var player = CreatePlayer(root.transform, materials);
            CreateHud(root.transform, patient, out var sirenOverlay);
            var gameManager = CreateGameManager(root.transform, patient, stretcher);
            var cameraTransform = CreateCameraAndLighting(root.transform, player.transform);
            var pusher = player.GetComponent<ThirdPersonPusherController>();
            if (pusher != null)
            {
                pusher.Configure(cameraTransform);
            }

            CreateEventDirector(root.transform, stretcher, sirenOverlay);

            gameManager.SetObjective("Segure com E e leve a maca ate a ambulancia.");
            Debug.Log("Maca Rush prototype scene created. Press Play to test the single-player third-person prototype.");
        }

        [ContextMenu("Build MVP Scene")]
        public void BuildLegacyMenu()
        {
            Build();
        }

        private void BuildHospital(Transform root, SceneMaterials materials)
        {
            CreateCube("Area 1 - Hospital Floor", root, new Vector3(0f, 0f, 6.5f), new Vector3(6f, 0.2f, 20f), materials.Floor);
            CreateCube("Hospital Left Wall", root, new Vector3(-3.15f, 1.4f, 6.5f), new Vector3(0.25f, 2.8f, 20f), materials.Wall);
            CreateCube("Hospital Right Wall", root, new Vector3(3.15f, 1.4f, 6.5f), new Vector3(0.25f, 2.8f, 20f), materials.Wall);
            CreateCube("Hospital Start Back Wall", root, new Vector3(0f, 1.4f, -3f), new Vector3(6f, 2.8f, 0.25f), materials.Wall);
            for (var i = 0; i < 7; i++)
            {
                var z = -1f + i * 2.7f;
                CreateCube($"Hospital Left Wall Panel {i + 1}", root, new Vector3(-3.02f, 1.45f, z), new Vector3(0.04f, 1.45f, 1.35f), materials.Glass);
                CreateCube($"Hospital Right Wall Panel {i + 1}", root, new Vector3(3.02f, 1.45f, z), new Vector3(0.04f, 1.45f, 1.35f), materials.Glass);
            }
            CreateFloorStripe(root, "Hospital Center Guide", new Vector3(0f, 0.12f, 6.5f), new Vector3(0.12f, 0.03f, 18f), materials.FloorStripe);
            CreateArrowMarker(root, "Hospital Direction Arrow", new Vector3(0f, 0.14f, 1.2f), 0.9f, 1.1f, materials.Objective);
            CreateArrowMarker(root, "Hospital Direction Arrow 2", new Vector3(0f, 0.14f, 8.5f), 0.9f, 1.1f, materials.Objective);
            CreateSign(root, "Hospital Sign", "EMERGENCY EXIT", new Vector3(0f, 2.35f, 3f), new Vector3(2.4f, 0.55f, 0.08f), materials.Danger, Color.white);

            CreatePushable("Laundry Cart", root, new Vector3(-1.15f, 0.55f, 5.5f), new Vector3(1.15f, 0.7f, 0.75f), 5f, materials.Interactable);
            CreatePushable("Supply Box", root, new Vector3(1.45f, 0.45f, 8.4f), new Vector3(0.9f, 0.7f, 0.9f), 3f, materials.Interactable);
            CreatePushable("Loose Chair", root, new Vector3(-1.55f, 0.35f, 12f), new Vector3(0.8f, 0.5f, 0.8f), 2.5f, materials.Interactable);

            var npc = CreatePrimitive(PrimitiveType.Capsule, "Crossing NPC", root, new Vector3(0f, 1f, 10.5f), new Vector3(0.55f, 1f, 0.55f), materials.Npc);
            var npcBody = npc.AddComponent<Rigidbody>();
            npcBody.isKinematic = true;
            var movingNpc = npc.AddComponent<MovingObstacle>();
            movingNpc.Configure(new Vector3(-2.3f, 0f, 0f), new Vector3(2.3f, 0f, 0f), 1.6f, true);
            createdMovingObstacles.Add(movingNpc);

            var eventCart = CreateCube("Event Crossing Cart", root, new Vector3(-2.65f, 0.45f, 13.1f), new Vector3(1f, 0.65f, 0.7f), materials.Pushable);
            var eventCartBody = eventCart.AddComponent<Rigidbody>();
            eventCartBody.isKinematic = true;
            var eventCartMover = eventCart.AddComponent<MovingObstacle>();
            eventCartMover.Configure(Vector3.zero, new Vector3(5.3f, 0f, 0f), 1.9f, false);
            createdMovingObstacles.Add(eventCartMover);

            CreateAutoDoor("Hospital Auto Door", root, new Vector3(0f, 1.25f, 14.2f), 5.2f, 2.5f, 0.2f, 0.18f, materials.Door);
            CreateCube("Auto Door Glass Left", root, new Vector3(-1.75f, 1.25f, 14.05f), new Vector3(1.1f, 1.7f, 0.06f), materials.Glass);
            CreateCube("Auto Door Glass Right", root, new Vector3(1.75f, 1.25f, 14.05f), new Vector3(1.1f, 1.7f, 0.06f), materials.Glass);
            CreateObjectiveZone("Objective - Route Choice", root, new Vector3(0f, 1f, 16f), new Vector3(6f, 2f, 0.5f), "Escolha: elevador instavel ou escada rapida.");
        }

        private void BuildRouteChoice(Transform root, SceneMaterials materials)
        {
            CreateCube("Area 2 - Route Floor", root, new Vector3(0f, 0f, 23.2f), new Vector3(9.2f, 0.2f, 13.5f), materials.Floor);
            CreateCube("Route Left Wall", root, new Vector3(-4.75f, 1.4f, 23.2f), new Vector3(0.25f, 2.8f, 13.5f), materials.Wall);
            CreateCube("Route Right Wall", root, new Vector3(4.75f, 1.4f, 23.2f), new Vector3(0.25f, 2.8f, 13.5f), materials.Wall);
            CreateCube("Route Divider", root, new Vector3(0f, 0.8f, 23.2f), new Vector3(0.22f, 1.6f, 10.5f), materials.Wall);
            CreateFloorStripe(root, "Elevator Lane Stripe", new Vector3(-2.35f, 0.12f, 23.4f), new Vector3(0.12f, 0.03f, 9.8f), materials.Objective);
            CreateFloorStripe(root, "Stair Lane Stripe", new Vector3(2.35f, 0.12f, 23.4f), new Vector3(0.12f, 0.03f, 9.8f), materials.FloorStripe);
            CreateSign(root, "Elevator Route Sign", "ELEVATOR\nSAFER / SLOWER", new Vector3(-2.35f, 2.3f, 18.7f), new Vector3(2.7f, 0.85f, 0.08f), materials.Objective, Color.black);
            CreateSign(root, "Stair Route Sign", "STAIRS\nFAST / RISKY", new Vector3(2.35f, 2.3f, 18.7f), new Vector3(2.7f, 0.85f, 0.08f), materials.Danger, Color.white);

            CreateCube("Elevator Platform", root, new Vector3(-2.35f, 0.08f, 24.5f), new Vector3(3.35f, 0.28f, 5.6f), materials.Metal);
            CreateCube("Elevator Glow Zone", root, new Vector3(-2.35f, 0.17f, 24.5f), new Vector3(2.8f, 0.04f, 4.8f), materials.Objective);
            CreateAutoDoor("Elevator Door", root, new Vector3(-2.35f, 1.25f, 20.8f), 3.25f, 2.5f, 0.26f, 0.34f, materials.Door);
            CreateAutoDoor("Elevator Exit Door", root, new Vector3(-2.35f, 1.25f, 27.6f), 3.25f, 2.5f, 0.26f, 0.25f, materials.Door);

            for (var i = 0; i < 8; i++)
            {
                CreateCube(
                    $"Stair Bump {i + 1}",
                    root,
                    new Vector3(2.35f, 0.22f, 19.3f + i * 0.8f),
                    new Vector3(3.25f, 0.24f, 0.16f),
                    materials.Step);
            }
            CreateFloorStripe(root, "Stair Danger Stripe A", new Vector3(2.35f, 0.36f, 21.3f), new Vector3(3f, 0.035f, 0.12f), materials.Danger);
            CreateFloorStripe(root, "Stair Danger Stripe B", new Vector3(2.35f, 0.36f, 24.5f), new Vector3(3f, 0.035f, 0.12f), materials.Danger);

            CreatePushable("Rolling Bin", root, new Vector3(2.3f, 0.55f, 26.6f), new Vector3(0.9f, 0.85f, 0.9f), 5f, materials.Interactable);
            CreateObjectiveZone("Objective - Street", root, new Vector3(0f, 1f, 30.1f), new Vector3(9.2f, 2f, 0.5f), "Rua: cuidado com buracos, chuva e carros.");
        }

        private void BuildStreet(Transform root, SceneMaterials materials)
        {
            var ramp = CreateCube("Street Ramp", root, new Vector3(0f, -0.22f, 31.8f), new Vector3(9.2f, 0.2f, 5.2f), materials.Street);
            ramp.transform.eulerAngles = new Vector3(5f, 0f, 0f);

            CreateCube("Area 3 - Street", root, new Vector3(0f, -0.55f, 43.5f), new Vector3(12f, 0.2f, 24f), materials.Street);
            CreateCube("Street Left Curb", root, new Vector3(-6.1f, -0.2f, 43.5f), new Vector3(0.3f, 0.7f, 24f), materials.Curb);
            CreateCube("Street Right Curb", root, new Vector3(6.1f, -0.2f, 43.5f), new Vector3(0.3f, 0.7f, 24f), materials.Curb);
            for (var i = 0; i < 6; i++)
            {
                CreateFloorStripe(root, $"Street Lane Dash {i + 1}", new Vector3(0f, -0.42f, 34.5f + i * 3.6f), new Vector3(0.18f, 0.03f, 1.7f), materials.FloorStripe);
            }
            for (var i = 0; i < 5; i++)
            {
                CreateFloorStripe(root, $"Crosswalk Stripe {i + 1}", new Vector3(-2.4f + i * 1.2f, -0.41f, 55.1f), new Vector3(0.75f, 0.03f, 2.7f), materials.PlayerAccent);
            }
            CreateSign(root, "Street Warning Sign", "WET ROAD", new Vector3(-4.8f, 1.25f, 38.2f), new Vector3(1.6f, 0.65f, 0.08f), materials.Danger, Color.white);

            CreatePothole("Pothole A", root, new Vector3(-2.2f, -0.35f, 37f), materials.Hazard);
            CreatePothole("Pothole B", root, new Vector3(1.8f, -0.35f, 42.5f), materials.Hazard);
            CreatePothole("Pothole C", root, new Vector3(-0.8f, -0.35f, 48.3f), materials.Hazard);

            for (var i = 0; i < 7; i++)
            {
                var x = -4f + i * 1.35f;
                CreateCone($"Traffic Cone {i + 1}", root, new Vector3(x, -0.05f, 39.5f + (i % 2) * 3.4f), materials.Cone);
            }

            var rain = CreateCube("Rain Slippery Zone", root, new Vector3(0f, 0.25f, 44.2f), new Vector3(10.5f, 1.2f, 7.5f), materials.Rain);
            rain.GetComponent<Collider>().isTrigger = true;
            var slipperyZone = rain.AddComponent<SlipperyZone>();
            slipperyZone.Configure(true, 0.62f);
            slipperyZone.ConfigureFx(materials.SlipFx);
            createdSlipperyZones.Add(slipperyZone);

            CreateCar("Crossing Car A", root, new Vector3(0f, 0.15f, 45.8f), new Vector3(-8f, 0f, 0f), new Vector3(8f, 0f, 0f), 1.25f, materials.Car);
            CreateCar("Crossing Car B", root, new Vector3(0f, 0.15f, 51.4f), new Vector3(8f, 0f, 0f), new Vector3(-8f, 0f, 0f), 1.05f, materials.Car);
            CreateObjectiveZone("Objective - Ambulance", root, new Vector3(0f, 1f, 53.6f), new Vector3(12f, 2f, 0.5f), "Ultimo trecho: coloque a maca na ambulancia.");
        }

        private void BuildAmbulance(Transform root, SceneMaterials materials)
        {
            CreateCube("Area 4 - Ambulance Bay", root, new Vector3(0f, -0.55f, 58f), new Vector3(12f, 0.2f, 7f), materials.Floor);
            CreateCube("Ambulance Body", root, new Vector3(0f, 0.6f, 61.8f), new Vector3(4.4f, 2.2f, 3.1f), materials.Ambulance);
            CreateCube("Ambulance Red Stripe", root, new Vector3(0f, 0.98f, 60.18f), new Vector3(4.2f, 0.18f, 0.08f), materials.Danger);
            CreateCube("Ambulance Window", root, new Vector3(0f, 1.45f, 60.12f), new Vector3(2.4f, 0.55f, 0.08f), materials.Glass);
            CreateCube("Ambulance Door Left", root, new Vector3(-1.15f, 0.75f, 60.15f), new Vector3(1.8f, 1.7f, 0.12f), materials.Door);
            CreateCube("Ambulance Door Right", root, new Vector3(1.15f, 0.75f, 60.15f), new Vector3(1.8f, 1.7f, 0.12f), materials.Door);
            CreateSign(root, "Ambulance Finish Sign", "LOAD PATIENT", new Vector3(0f, 2.4f, 58.5f), new Vector3(2.9f, 0.55f, 0.08f), materials.Objective, Color.black);

            var finish = CreateCube("Finish Zone - Ambulance", root, new Vector3(0f, 0.25f, 58.8f), new Vector3(5f, 1.5f, 2.2f), materials.Finish);
            finish.GetComponent<Collider>().isTrigger = true;
            finish.AddComponent<FinishZone>();
        }

        private MacaStretcher CreateStretcher(Transform root, SceneMaterials materials, out PatientHealth patient)
        {
            var maca = new GameObject("Maca");
            maca.transform.SetParent(root, false);
            maca.transform.position = new Vector3(0f, 0.85f, 0.8f);
            TryAssignTag(maca, "Maca");

            CreateLocalPrimitive(PrimitiveType.Cube, "Maca Body", maca.transform, Vector3.zero, new Vector3(2.2f, 0.25f, 1.1f), materials.Stretcher);

            var rb = maca.AddComponent<Rigidbody>();
            rb.mass = 26f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var stretcher = maca.AddComponent<MacaStretcher>();
            stretcher.ConfigureFx(materials.ImpactFx);

            var patientObject = CreateLocalPrimitive(PrimitiveType.Capsule, "Patient", maca.transform, new Vector3(0f, 0.45f, 0f), new Vector3(0.45f, 0.9f, 0.45f), materials.Patient);
            patientObject.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            patientObject.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            patient = patientObject.AddComponent<PatientHealth>();
            patient.Configure(patientObject.GetComponent<Renderer>());
            stretcher.Configure(patient, patientObject.transform);

            CreateVisualPrimitive(PrimitiveType.Cube, "Maca Mattress", maca.transform, new Vector3(0f, 0.18f, 0f), new Vector3(2.35f, 0.16f, 1.2f), materials.Blanket);
            CreateVisualPrimitive(PrimitiveType.Cube, "Maca Left Rail", maca.transform, new Vector3(-1.28f, 0.32f, 0f), new Vector3(0.08f, 0.16f, 1.35f), materials.StretcherFrame);
            CreateVisualPrimitive(PrimitiveType.Cube, "Maca Right Rail", maca.transform, new Vector3(1.28f, 0.32f, 0f), new Vector3(0.08f, 0.16f, 1.35f), materials.StretcherFrame);
            CreateVisualPrimitive(PrimitiveType.Cube, "Maca Front Push Bar", maca.transform, new Vector3(0f, 0.42f, -0.78f), new Vector3(2.7f, 0.08f, 0.08f), materials.Handle);
            CreateVisualPrimitive(PrimitiveType.Cube, "Maca Back Rail", maca.transform, new Vector3(0f, 0.28f, 0.78f), new Vector3(2.5f, 0.08f, 0.08f), materials.StretcherFrame);
            CreateVisualPrimitive(PrimitiveType.Cube, "Maca Under Frame", maca.transform, new Vector3(0f, -0.18f, 0f), new Vector3(2.35f, 0.08f, 1.05f), materials.StretcherFrame);
            CreateWheel(maca.transform, "Wheel FL", new Vector3(-0.95f, -0.42f, -0.48f), materials.Wheel);
            CreateWheel(maca.transform, "Wheel FR", new Vector3(0.95f, -0.42f, -0.48f), materials.Wheel);
            CreateWheel(maca.transform, "Wheel BL", new Vector3(-0.95f, -0.42f, 0.48f), materials.Wheel);
            CreateWheel(maca.transform, "Wheel BR", new Vector3(0.95f, -0.42f, 0.48f), materials.Wheel);

            var criticalFx = CreateVisualPrimitive(PrimitiveType.Sphere, "Patient Critical Pulse", maca.transform, new Vector3(0f, 1f, 0f), new Vector3(0.55f, 0.08f, 0.55f), materials.Danger);
            var criticalRenderer = criticalFx.GetComponent<Renderer>();
            if (criticalRenderer != null)
            {
                criticalRenderer.enabled = false;
            }

            criticalFx.AddComponent<PatientCriticalFx>().Configure(patient, criticalRenderer);

            return stretcher;
        }

        private GameObject CreatePlayer(Transform root, SceneMaterials materials)
        {
            var player = new GameObject("Player");
            player.transform.SetParent(root, false);
            player.transform.position = new Vector3(0f, 1f, -1.4f);
            TryAssignTag(player, "Player");

            var collider = player.AddComponent<CapsuleCollider>();
            collider.height = 1.85f;
            collider.radius = 0.34f;
            collider.center = new Vector3(0f, 0.02f, 0f);

            var rb = player.AddComponent<Rigidbody>();
            rb.mass = 3f;
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            var visualRoot = new GameObject("Player Visual");
            visualRoot.transform.SetParent(player.transform, false);
            visualRoot.transform.localPosition = Vector3.zero;

            CreateVisualPrimitive(PrimitiveType.Capsule, "Player Torso", visualRoot.transform, new Vector3(0f, 0.32f, 0f), new Vector3(0.46f, 0.68f, 0.32f), materials.Player1);
            CreateVisualPrimitive(PrimitiveType.Sphere, "Player Head", visualRoot.transform, new Vector3(0f, 1.12f, 0f), new Vector3(0.34f, 0.34f, 0.34f), materials.PlayerSkin);
            CreateVisualPrimitive(PrimitiveType.Cube, "Player Mask", visualRoot.transform, new Vector3(0f, 1.08f, -0.18f), new Vector3(0.32f, 0.11f, 0.04f), materials.PlayerAccent);
            CreateVisualPrimitive(PrimitiveType.Cube, "Player Medical Pack", visualRoot.transform, new Vector3(0f, 0.3f, 0.24f), new Vector3(0.45f, 0.52f, 0.14f), materials.Interactable);

            var leftArm = CreateVisualPrimitive(PrimitiveType.Capsule, "Player Left Arm", visualRoot.transform, new Vector3(-0.38f, 0.42f, -0.02f), new Vector3(0.16f, 0.48f, 0.16f), materials.PlayerSkin).transform;
            var rightArm = CreateVisualPrimitive(PrimitiveType.Capsule, "Player Right Arm", visualRoot.transform, new Vector3(0.38f, 0.42f, -0.02f), new Vector3(0.16f, 0.48f, 0.16f), materials.PlayerSkin).transform;
            var leftLeg = CreateVisualPrimitive(PrimitiveType.Capsule, "Player Left Leg", visualRoot.transform, new Vector3(-0.16f, -0.42f, 0f), new Vector3(0.18f, 0.55f, 0.18f), materials.Player1).transform;
            var rightLeg = CreateVisualPrimitive(PrimitiveType.Capsule, "Player Right Leg", visualRoot.transform, new Vector3(0.16f, -0.42f, 0f), new Vector3(0.18f, 0.55f, 0.18f), materials.Player1).transform;

            var controller = player.AddComponent<ThirdPersonPusherController>();
            player.AddComponent<PlayerVisualAnimator>().Configure(controller, visualRoot.transform, leftArm, rightArm, leftLeg, rightLeg);
            return player;
        }

        private GameManager CreateGameManager(Transform root, PatientHealth patient, MacaStretcher stretcher)
        {
            var managerObject = new GameObject("GameManager");
            managerObject.transform.SetParent(root, false);
            var manager = managerObject.AddComponent<GameManager>();
            manager.Configure(patient, stretcher);
            return manager;
        }

        private SimpleHud CreateHud(Transform root, PatientHealth patient, out Image sirenOverlay)
        {
            var canvasObject = new GameObject("HUD Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(root, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);

            var statusText = CreateText("Status Text", canvasObject.transform, new Vector2(20f, -20f), new Vector2(520f, 128f), 24, TextAnchor.UpperLeft);
            var alertText = CreateText("Alert Text", canvasObject.transform, new Vector2(0f, -90f), new Vector2(700f, 180f), 42, TextAnchor.UpperCenter);
            alertText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            alertText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            alertText.rectTransform.pivot = new Vector2(0.5f, 1f);

            var healthBack = CreateImage("Health Back", canvasObject.transform, new Vector2(20f, -155f), new Vector2(360f, 24f), new Color(0.05f, 0.05f, 0.05f, 0.85f));
            var healthFill = CreateImage("Health Fill", healthBack.transform, Vector2.zero, Vector2.zero, Color.green);
            healthFill.rectTransform.anchorMin = Vector2.zero;
            healthFill.rectTransform.anchorMax = Vector2.one;
            healthFill.rectTransform.offsetMin = Vector2.zero;
            healthFill.rectTransform.offsetMax = Vector2.zero;
            healthFill.type = Image.Type.Filled;
            healthFill.fillMethod = Image.FillMethod.Horizontal;

            sirenOverlay = CreateImage("Siren Overlay", canvasObject.transform, Vector2.zero, Vector2.zero, Color.clear);
            sirenOverlay.rectTransform.anchorMin = Vector2.zero;
            sirenOverlay.rectTransform.anchorMax = Vector2.one;
            sirenOverlay.rectTransform.offsetMin = Vector2.zero;
            sirenOverlay.rectTransform.offsetMax = Vector2.zero;
            sirenOverlay.raycastTarget = false;

            var hud = canvasObject.AddComponent<SimpleHud>();
            hud.Configure(patient, statusText, alertText, healthFill, sirenOverlay);
            return hud;
        }

        private Transform CreateCameraAndLighting(Transform root, Transform followTarget)
        {
            if (followTarget == null)
            {
                var pusher = FindFirstObjectByType<ThirdPersonPusherController>();
                if (pusher != null)
                {
                    followTarget = pusher.transform;
                }
            }

            var camerasInScene = FindObjectsByType<Camera>(FindObjectsSortMode.None);
            Camera chosenCamera = null;

            if (reuseSceneMainCamera)
            {
                if (Camera.main != null)
                {
                    chosenCamera = Camera.main;
                }
                else if (camerasInScene.Length > 0)
                {
                    chosenCamera = camerasInScene[0];
                }
            }

            GameObject cameraObject = chosenCamera != null ? chosenCamera.gameObject : null;
            if (cameraObject == null)
            {
                cameraObject = new GameObject("Main Camera");
                cameraObject.transform.SetParent(root, false);
            }

            var camera = cameraObject.GetComponent<Camera>();
            if (camera == null)
            {
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.tag = "MainCamera";
            camera.fieldOfView = 62f;

            if (followTarget != null)
            {
                var startPivot = followTarget.position + new Vector3(0f, 1.6f, 0f);
                cameraObject.transform.position = startPivot + Quaternion.Euler(16f, followTarget.eulerAngles.y, 0f) * new Vector3(0f, 0f, -4.6f);
                cameraObject.transform.rotation = Quaternion.LookRotation(startPivot - cameraObject.transform.position, Vector3.up);
            }

            if (cameraObject.GetComponent<AudioListener>() == null)
            {
                cameraObject.AddComponent<AudioListener>();
            }

            var followCam = cameraObject.GetComponent<SimpleFollowCamera>();
            if (followCam == null)
            {
                followCam = cameraObject.AddComponent<SimpleFollowCamera>();
            }

            followCam.Configure(followTarget);

            for (var i = 0; i < camerasInScene.Length; i++)
            {
                var other = camerasInScene[i];
                if (other == null || other == camera) continue;

                if (other.CompareTag("MainCamera"))
                {
                    other.tag = "Untagged";
                }
            }

            var sunObject = new GameObject("Scene Sun");
            sunObject.transform.SetParent(root, false);
            sunObject.transform.rotation = Quaternion.Euler(48f, -25f, 0f);
            var sun = sunObject.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.intensity = 1f;
            createdLights.Add(sun);

            CreatePointLight("Hospital Light", root, new Vector3(0f, 4f, 9f), 1.4f, 12f);
            CreatePointLight("Street Light", root, new Vector3(0f, 5f, 43f), 1.15f, 16f);
            CreatePointLight("Ambulance Light", root, new Vector3(0f, 4f, 59f), 1.6f, 10f);

            return cameraObject.transform;
        }

        private void CreateEventDirector(Transform root, MacaStretcher stretcher, Image sirenOverlay)
        {
            var directorObject = new GameObject("Random Event Director");
            directorObject.transform.SetParent(root, false);
            var director = directorObject.AddComponent<RandomEventDirector>();
            director.Configure(
                stretcher,
                sirenOverlay,
                createdDoors.ToArray(),
                createdMovingObstacles.ToArray(),
                createdSlipperyZones.ToArray(),
                createdLights.ToArray());
        }

        private void CreateAutoDoor(string name, Transform root, Vector3 panelPosition, float width, float height, float depth, float jamChance, Material material)
        {
            var panel = CreateCube($"{name} Panel", root, panelPosition, new Vector3(width, height, depth), material);

            var trigger = CreateCube($"{name} Trigger", root, panelPosition + Vector3.back * 0.7f, new Vector3(width + 0.4f, height, 2.4f), material);
            trigger.GetComponent<Renderer>().enabled = false;
            trigger.GetComponent<Collider>().isTrigger = true;

            var door = trigger.AddComponent<AutoDoor>();
            door.Configure(panel.transform, new Vector3(0f, height + 0.45f, 0f), 3.4f, jamChance);
            createdDoors.Add(door);
        }

        private void CreatePushable(string name, Transform root, Vector3 position, Vector3 scale, float mass, Material material)
        {
            var pushable = CreateCube(name, root, position, scale, material);
            var rb = pushable.AddComponent<Rigidbody>();
            rb.mass = mass;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void CreateWheel(Transform parent, string name, Vector3 localPosition, Material material)
        {
            var wheel = CreateVisualPrimitive(PrimitiveType.Cylinder, name, parent, localPosition, new Vector3(0.22f, 0.08f, 0.22f), material);
            wheel.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
        }

        private void CreatePothole(string name, Transform root, Vector3 position, Material material)
        {
            var pothole = CreatePrimitive(PrimitiveType.Cylinder, name, root, position, new Vector3(1.35f, 0.05f, 1.35f), material);
            pothole.GetComponent<Collider>().isTrigger = true;
            pothole.AddComponent<HazardDamageZone>().Configure(7f);
        }

        private void CreateCone(string name, Transform root, Vector3 position, Material material)
        {
            var cone = CreatePrimitive(PrimitiveType.Cylinder, name, root, position, new Vector3(0.28f, 0.5f, 0.28f), material);
            var rb = cone.AddComponent<Rigidbody>();
            rb.mass = 0.8f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void CreateCar(string name, Transform root, Vector3 position, Vector3 pointA, Vector3 pointB, float speed, Material material)
        {
            var car = CreateCube(name, root, position, new Vector3(2.8f, 0.9f, 1.4f), material);
            var rb = car.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            var mover = car.AddComponent<MovingObstacle>();
            mover.Configure(pointA, pointB, speed, true);
            createdMovingObstacles.Add(mover);
        }

        private void CreateObjectiveZone(string name, Transform root, Vector3 position, Vector3 scale, string objective)
        {
            var zone = CreateCube(name, root, position, scale, null);
            zone.GetComponent<Renderer>().enabled = false;
            zone.GetComponent<Collider>().isTrigger = true;
            zone.AddComponent<ObjectiveZone>().Configure(objective);
        }

        private void CreateSign(Transform root, string name, string label, Vector3 position, Vector3 scale, Material backing, Color textColor)
        {
            var sign = CreateCube($"{name} Backing", root, position, scale, backing);
            var textObject = new GameObject($"{name} Text");
            textObject.transform.SetParent(root, false);
            textObject.transform.position = position + Vector3.back * (scale.z * 0.55f + 0.02f);
            textObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            var textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = label;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.characterSize = 0.22f;
            textMesh.fontSize = 48;
            textMesh.color = textColor;
        }

        private GameObject CreateFloorStripe(Transform root, string name, Vector3 position, Vector3 scale, Material material)
        {
            return CreateCube(name, root, position + Vector3.up * 0.02f, scale, material);
        }

        private void CreateArrowMarker(Transform root, string name, Vector3 position, float width, float length, Material material)
        {
            CreateFloorStripe(root, $"{name} Body", position, new Vector3(width * 0.35f, 0.025f, length), material);
            var head = CreateFloorStripe(root, $"{name} Head", position + Vector3.forward * (length * 0.55f), new Vector3(width, 0.025f, width), material);
            head.transform.eulerAngles = new Vector3(0f, 45f, 0f);
        }

        private void CreatePointLight(string name, Transform root, Vector3 position, float intensity, float range)
        {
            var lightObject = new GameObject(name);
            lightObject.transform.SetParent(root, false);
            lightObject.transform.position = position;
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.intensity = intensity;
            light.range = range;
            createdLights.Add(light);
        }

        private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            return CreatePrimitive(PrimitiveType.Cube, name, parent, position, scale, material);
        }

        private static GameObject CreatePrimitive(PrimitiveType type, string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.position = position;
            obj.transform.localScale = scale;

            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            return obj;
        }

        private static GameObject CreateLocalPrimitive(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = localScale;

            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            return obj;
        }

        private static GameObject CreateVisualPrimitive(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var obj = CreateLocalPrimitive(type, name, parent, localPosition, localScale, material);
            var collider = obj.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(collider);
                }
                else
                {
                    DestroyImmediate(collider);
                }
            }

            return obj;
        }

        private static Text CreateText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(parent, false);

            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static Image CreateImage(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            var imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            imageObject.transform.SetParent(parent, false);

            var rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private void ClearPreviousScene()
        {
            var previous = GameObject.Find(generatedRootName);
            if (previous == null) return;

            if (Application.isPlaying)
            {
                Destroy(previous);
            }
            else
            {
                DestroyImmediate(previous);
            }
        }

        private static void TryAssignTag(GameObject obj, string tagName)
        {
            try
            {
                obj.tag = tagName;
            }
            catch (UnityException)
            {
                Debug.LogWarning($"Tag '{tagName}' nao existe. O prototipo funciona sem ela, mas voce pode cria-la no Tag Manager.");
            }
        }

        private sealed class SceneMaterials
        {
            public Material Floor;
            public Material Wall;
            public Material Street;
            public Material Curb;
            public Material Pushable;
            public Material Npc;
            public Material Door;
            public Material Metal;
            public Material Step;
            public Material Hazard;
            public Material Cone;
            public Material Rain;
            public Material Car;
            public Material Ambulance;
            public Material Finish;
            public Material Stretcher;
            public Material StretcherFrame;
            public Material Wheel;
            public Material Patient;
            public Material Blanket;
            public Material Handle;
            public Material Player1;
            public Material PlayerSkin;
            public Material PlayerAccent;
            public Material Player2;
            public Material Player3;
            public Material Player4;
            public Material Interactable;
            public Material Danger;
            public Material Objective;
            public Material FloorStripe;
            public Material Glass;
            public Material SlipFx;
            public Material ImpactFx;

            public static SceneMaterials Create()
            {
                return new SceneMaterials
                {
                    Floor = CreateMaterial("Floor", new Color(0.55f, 0.58f, 0.56f)),
                    Wall = CreateMaterial("Wall", new Color(0.78f, 0.8f, 0.78f)),
                    Street = CreateMaterial("Street", new Color(0.17f, 0.17f, 0.18f)),
                    Curb = CreateMaterial("Curb", new Color(0.45f, 0.45f, 0.43f)),
                    Pushable = CreateMaterial("Pushable", new Color(0.62f, 0.45f, 0.28f)),
                    Npc = CreateMaterial("Npc", new Color(0.95f, 0.55f, 0.72f)),
                    Door = CreateMaterial("Door", new Color(0.2f, 0.45f, 0.72f)),
                    Metal = CreateMaterial("Metal", new Color(0.42f, 0.45f, 0.48f)),
                    Step = CreateMaterial("Step", new Color(0.33f, 0.34f, 0.35f)),
                    Hazard = CreateMaterial("Hazard", new Color(0.03f, 0.03f, 0.035f)),
                    Cone = CreateMaterial("Cone", new Color(1f, 0.42f, 0.05f)),
                    Rain = CreateTransparentMaterial("Rain", new Color(0.25f, 0.55f, 0.95f, 0.2f)),
                    Car = CreateMaterial("Car", new Color(0.9f, 0.12f, 0.12f)),
                    Ambulance = CreateMaterial("Ambulance", new Color(0.94f, 0.94f, 0.9f)),
                    Finish = CreateTransparentMaterial("Finish", new Color(0.2f, 1f, 0.25f, 0.32f)),
                    Stretcher = CreateMaterial("Stretcher", new Color(0.9f, 0.88f, 0.78f)),
                    StretcherFrame = CreateMaterial("Stretcher Frame", new Color(0.12f, 0.18f, 0.22f)),
                    Wheel = CreateMaterial("Wheel", new Color(0.03f, 0.035f, 0.04f)),
                    Patient = CreateMaterial("Patient", new Color(0.3f, 0.95f, 0.45f)),
                    Blanket = CreateMaterial("Blanket", new Color(0.25f, 0.55f, 0.82f)),
                    Handle = CreateMaterial("Handle", new Color(0.05f, 0.08f, 0.1f)),
                    Player1 = CreateMaterial("Player 1", new Color(0.12f, 0.65f, 0.72f)),
                    PlayerSkin = CreateMaterial("Player Skin", new Color(0.86f, 0.62f, 0.42f)),
                    PlayerAccent = CreateMaterial("Player Accent", new Color(0.95f, 0.96f, 0.9f)),
                    Player2 = CreateMaterial("Player 2", new Color(1f, 0.25f, 0.2f)),
                    Player3 = CreateMaterial("Player 3", new Color(0.22f, 0.9f, 0.35f)),
                    Player4 = CreateMaterial("Player 4", new Color(1f, 0.85f, 0.1f)),
                    Interactable = CreateMaterial("Interactable", new Color(0.95f, 0.72f, 0.18f)),
                    Danger = CreateMaterial("Danger", new Color(0.95f, 0.1f, 0.08f)),
                    Objective = CreateTransparentMaterial("Objective", new Color(0.12f, 1f, 0.42f, 0.45f)),
                    FloorStripe = CreateMaterial("Floor Stripe", new Color(0.98f, 0.94f, 0.45f)),
                    Glass = CreateTransparentMaterial("Glass", new Color(0.32f, 0.75f, 0.95f, 0.24f)),
                    SlipFx = CreateTransparentMaterial("Slip FX", new Color(0.42f, 0.85f, 1f, 0.38f)),
                    ImpactFx = CreateTransparentMaterial("Impact FX", new Color(1f, 0.72f, 0.12f, 0.9f))
                };
            }

            private static Material CreateMaterial(string name, Color color)
            {
                var material = new Material(FindDefaultShader())
                {
                    name = $"MacaRush {name}",
                    color = color
                };
                return material;
            }

            private static Material CreateTransparentMaterial(string name, Color color)
            {
                var material = CreateMaterial(name, color);
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                return material;
            }

            private static Shader FindDefaultShader()
            {
                var shader = Shader.Find("Standard");
                if (shader != null) return shader;

                shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader != null) return shader;

                return Shader.Find("Unlit/Color");
            }
        }
    }
}
