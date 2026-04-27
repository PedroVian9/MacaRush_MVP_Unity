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
        [SerializeField] private Color panelColor = new Color(0.04f, 0.08f, 0.12f, 0.92f);
        [SerializeField] private Color buttonColor = new Color(0.16f, 0.48f, 0.78f, 1f);
        [SerializeField] private Color dangerButtonColor = new Color(0.75f, 0.15f, 0.15f, 1f);

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
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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

            cam.transform.position = new Vector3(0f, 7.8f, -13.8f);
            cam.transform.rotation = Quaternion.Euler(23f, 0f, 0f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.03f, 0.05f, 0.07f);

            var follow = cam.GetComponent<SimpleFollowCamera>();
            if (follow != null)
            {
                follow.enabled = false;
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

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var overlay = CreateImage("Overlay", canvasObject.transform, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.34f));
            overlay.rectTransform.anchorMin = Vector2.zero;
            overlay.rectTransform.anchorMax = Vector2.one;
            overlay.rectTransform.offsetMin = Vector2.zero;
            overlay.rectTransform.offsetMax = Vector2.zero;

            var panel = CreateImage("Panel", canvasObject.transform, Vector2.zero, new Vector2(640f, 450f), panelColor);
            panel.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            panel.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            panel.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            panel.rectTransform.anchoredPosition = Vector2.zero;
            panel.rectTransform.sizeDelta = new Vector2(640f, 450f);

            var title = CreateText("Title", panel.transform, new Vector2(0f, -36f), new Vector2(560f, 70f), 52, TextAnchor.MiddleCenter);
            title.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.text = "MACA RUSH";

            var subtitle = CreateText("Subtitle", panel.transform, new Vector2(0f, -108f), new Vector2(560f, 52f), 24, TextAnchor.MiddleCenter);
            subtitle.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            subtitle.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            subtitle.rectTransform.pivot = new Vector2(0.5f, 1f);
            subtitle.color = new Color(0.85f, 0.89f, 0.93f, 1f);
            subtitle.text = "Escolha o mapa e inicie a corrida da maca";

            var dropdownLabel = CreateText("Map Label", panel.transform, new Vector2(0f, -176f), new Vector2(420f, 32f), 24, TextAnchor.MiddleCenter);
            dropdownLabel.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            dropdownLabel.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            dropdownLabel.rectTransform.pivot = new Vector2(0.5f, 1f);
            dropdownLabel.text = "Mapa";

            var dropdown = CreateDropdown(panel.transform, new Vector2(0f, -214f), new Vector2(420f, 44f));
            dropdown.value = Mathf.Clamp((int)selectedPreset, 0, dropdown.options.Count - 1);
            dropdown.RefreshShownValue();
            dropdown.onValueChanged.AddListener(OnMapChanged);

            var playButton = CreateButton(panel.transform, "Jogar", new Vector2(0f, -292f), new Vector2(420f, 56f), buttonColor);
            playButton.onClick.AddListener(OnPlayClicked);

            var quitButton = CreateButton(panel.transform, "Sair", new Vector2(0f, -360f), new Vector2(420f, 48f), dangerButtonColor);
            quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnMapChanged(int value)
        {
            selectedPreset = (MapPreset)Mathf.Clamp(value, 0, (int)MapPreset.StreetDash);
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

            sceneBuilder.Build(selectedPreset);
            Destroy(gameObject);
        }

        private static void OnQuitClicked()
        {
            Application.Quit();
        }

        private static Dropdown CreateDropdown(Transform parent, Vector2 anchoredPosition, Vector2 size)
        {
            var dropdownObject = new GameObject("Map Dropdown", typeof(RectTransform), typeof(Image), typeof(Dropdown));
            dropdownObject.transform.SetParent(parent, false);

            var rect = dropdownObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var image = dropdownObject.GetComponent<Image>();
            image.color = new Color(0.95f, 0.95f, 0.95f, 1f);

            var dropdown = dropdownObject.GetComponent<Dropdown>();
            dropdown.options = new System.Collections.Generic.List<Dropdown.OptionData>
            {
                new Dropdown.OptionData("Mapa Completo"),
                new Dropdown.OptionData("Hospital Sprint"),
                new Dropdown.OptionData("Street Dash")
            };

            var label = CreateText("Label", dropdownObject.transform, new Vector2(16f, -10f), new Vector2(size.x - 48f, size.y - 12f), 22, TextAnchor.MiddleLeft);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = new Vector2(12f, 4f);
            label.rectTransform.offsetMax = new Vector2(-34f, -4f);
            label.color = Color.black;
            dropdown.captionText = label;

            var template = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            template.transform.SetParent(dropdownObject.transform, false);
            var templateRect = template.GetComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0f, 0f);
            templateRect.anchorMax = new Vector2(1f, 0f);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.anchoredPosition = new Vector2(0f, 2f);
            templateRect.sizeDelta = new Vector2(0f, 136f);

            var templateImage = template.GetComponent<Image>();
            templateImage.color = new Color(0.95f, 0.95f, 0.95f, 1f);
            template.SetActive(false);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(template.transform, false);
            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            var viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.02f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform), typeof(ToggleGroup));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 136f);

            var scrollRect = template.GetComponent<ScrollRect>();
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var item = new GameObject("Item", typeof(RectTransform), typeof(Image), typeof(Toggle));
            item.transform.SetParent(content.transform, false);
            var itemRect = item.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 1f);
            itemRect.anchorMax = new Vector2(1f, 1f);
            itemRect.pivot = new Vector2(0.5f, 1f);
            itemRect.sizeDelta = new Vector2(0f, 44f);
            itemRect.anchoredPosition = new Vector2(0f, -22f);
            item.GetComponent<Image>().color = Color.white;

            var itemCheckmark = new GameObject("Item Checkmark", typeof(RectTransform), typeof(Image));
            itemCheckmark.transform.SetParent(item.transform, false);
            var itemCheckRect = itemCheckmark.GetComponent<RectTransform>();
            itemCheckRect.anchorMin = new Vector2(0f, 0.5f);
            itemCheckRect.anchorMax = new Vector2(0f, 0.5f);
            itemCheckRect.sizeDelta = new Vector2(18f, 18f);
            itemCheckRect.anchoredPosition = new Vector2(14f, 0f);
            itemCheckmark.GetComponent<Image>().color = new Color(0.2f, 0.5f, 0.82f, 1f);

            var itemLabel = CreateText("Item Label", item.transform, Vector2.zero, new Vector2(0f, 0f), 21, TextAnchor.MiddleLeft);
            itemLabel.rectTransform.anchorMin = Vector2.zero;
            itemLabel.rectTransform.anchorMax = Vector2.one;
            itemLabel.rectTransform.offsetMin = new Vector2(34f, 4f);
            itemLabel.rectTransform.offsetMax = new Vector2(-8f, -4f);
            itemLabel.color = Color.black;

            var itemToggle = item.GetComponent<Toggle>();
            itemToggle.targetGraphic = item.GetComponent<Image>();
            itemToggle.graphic = itemCheckmark.GetComponent<Image>();

            dropdown.template = templateRect;
            dropdown.itemText = itemLabel;
            dropdown.itemImage = itemCheckmark.GetComponent<Image>();

            return dropdown;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            var buttonObject = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var image = buttonObject.GetComponent<Image>();
            image.color = color;

            var text = CreateText(label + " Text", buttonObject.transform, Vector2.zero, size, 26, TextAnchor.MiddleCenter);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
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
    }
}
