using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MacaRush
{
    public sealed class MacaRushMainMenu : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private MacaRushSceneBuilder sceneBuilder;
        [SerializeField] private MapPreset selectedPreset = MapPreset.FullRoute;

        [Header("Style")]
        [SerializeField] private Color panelColor = new Color(0.06f, 0.1f, 0.14f, 0.96f);
        [SerializeField] private Color primaryButtonColor = new Color(0.14f, 0.48f, 0.78f, 1f);
        [SerializeField] private Color secondaryButtonColor = new Color(0.2f, 0.34f, 0.5f, 1f);
        [SerializeField] private Color dangerButtonColor = new Color(0.72f, 0.15f, 0.15f, 1f);

        private Text mapText;
        private bool startedGame;

        public static void EnsureCreated(MacaRushSceneBuilder builder, MapPreset defaultPreset)
        {
            var existing = FindFirstObjectByType<MacaRushMainMenu>();
            if (existing != null)
            {
                existing.Configure(builder, defaultPreset);
                return;
            }

            var menuObject = new GameObject("MacaRush Main Menu");
            var menu = menuObject.AddComponent<MacaRushMainMenu>();
            menu.Configure(builder, defaultPreset);
        }

        public void Configure(MacaRushSceneBuilder builder, MapPreset defaultPreset)
        {
            sceneBuilder = builder;
            selectedPreset = defaultPreset;
        }

        private void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (sceneBuilder == null)
            {
                sceneBuilder = FindFirstObjectByType<MacaRushSceneBuilder>();
            }

            EnsureMenuCamera();
            EnsureEventSystem();
            BuildUi();

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDestroy()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (startedGame)
            {
                return;
            }

            Time.timeScale = 1f;
        }

        private void EnsureMenuCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
                if (cameras.Length > 0)
                {
                    cam = cameras[0];
                }
            }

            if (cam == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cam = cameraObject.AddComponent<Camera>();
                cam.tag = "MainCamera";
                cameraObject.AddComponent<AudioListener>();
            }

            cam.transform.position = new Vector3(0f, 6.4f, -11.4f);
            cam.transform.rotation = Quaternion.Euler(21f, 0f, 0f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.03f, 0.05f, 0.07f, 1f);

            var followCam = cam.GetComponent<SimpleFollowCamera>();
            if (followCam != null)
            {
                followCam.enabled = false;
            }
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private void BuildUi()
        {
            var canvasObject = new GameObject("Main Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var overlay = CreateImage("Overlay", canvasObject.transform, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.46f), true);
            overlay.rectTransform.anchorMin = Vector2.zero;
            overlay.rectTransform.anchorMax = Vector2.one;
            overlay.rectTransform.offsetMin = Vector2.zero;
            overlay.rectTransform.offsetMax = Vector2.zero;

            var panel = CreateImage("Panel", canvasObject.transform, Vector2.zero, new Vector2(680f, 420f), panelColor, true);
            panel.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            panel.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            panel.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            panel.rectTransform.anchoredPosition = Vector2.zero;

            var title = CreateText("Title", panel.transform, new Vector2(0f, -36f), new Vector2(600f, 70f), 54, TextAnchor.MiddleCenter);
            SetAnchorsTopCenter(title.rectTransform);
            title.text = "MACA RUSH";

            var subtitle = CreateText("Subtitle", panel.transform, new Vector2(0f, -98f), new Vector2(600f, 45f), 24, TextAnchor.MiddleCenter);
            SetAnchorsTopCenter(subtitle.rectTransform);
            subtitle.text = "Menu principal";
            subtitle.color = new Color(0.84f, 0.9f, 0.96f, 1f);

            mapText = CreateText("MapText", panel.transform, new Vector2(0f, -162f), new Vector2(600f, 40f), 28, TextAnchor.MiddleCenter);
            SetAnchorsTopCenter(mapText.rectTransform);
            UpdateMapText();

            var changeMapButton = CreateButton(panel.transform, "Trocar Mapa", new Vector2(0f, -212f), new Vector2(430f, 54f), secondaryButtonColor);
            changeMapButton.onClick.AddListener(CycleMap);

            var enterButton = CreateButton(panel.transform, "Entrar", new Vector2(0f, -280f), new Vector2(430f, 60f), primaryButtonColor);
            enterButton.onClick.AddListener(OnPlayClicked);

            var quitButton = CreateButton(panel.transform, "Sair", new Vector2(0f, -350f), new Vector2(430f, 48f), dangerButtonColor);
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void CycleMap()
        {
            var next = ((int)selectedPreset + 1) % 3;
            selectedPreset = (MapPreset)next;
            UpdateMapText();
        }

        private void UpdateMapText()
        {
            if (mapText == null) return;
            mapText.text = $"Mapa: {GetMapLabel(selectedPreset)}";
        }

        private static string GetMapLabel(MapPreset preset)
        {
            switch (preset)
            {
                case MapPreset.HospitalSprint:
                    return "Hospital Sprint";
                case MapPreset.StreetDash:
                    return "Street Dash";
                default:
                    return "Mapa Completo";
            }
        }

        private void OnPlayClicked()
        {
            if (sceneBuilder == null)
            {
                sceneBuilder = FindFirstObjectByType<MacaRushSceneBuilder>();
                if (sceneBuilder == null)
                {
                    Debug.LogError("MacaRushSceneBuilder nao encontrado na cena.");
                    return;
                }
            }

            startedGame = true;
            Time.timeScale = 1f;
            sceneBuilder.Build(selectedPreset);
            Destroy(gameObject);
        }

        private static void OnQuitClicked()
        {
            Time.timeScale = 1f;
            Application.Quit();
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            var buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.GetComponent<RectTransform>();
            SetAnchorsTopCenter(rect);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var image = buttonObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;

            var text = CreateText(label + " Text", buttonObject.transform, Vector2.zero, size, 28, TextAnchor.MiddleCenter);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            text.text = label;

            return buttonObject.GetComponent<Button>();
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
            text.font = GetBuiltinFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static Font GetBuiltinFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null) return font;

            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static Image CreateImage(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, Color color, bool raycastTarget)
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
            image.raycastTarget = raycastTarget;
            return image;
        }

        private static void SetAnchorsTopCenter(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
        }
    }
}
