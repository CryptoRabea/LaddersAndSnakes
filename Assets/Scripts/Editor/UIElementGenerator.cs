using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LAS.Core;

namespace LAS.Editor
{
    /// <summary>
    /// Core utility for generating UI elements in the scene
    /// </summary>
    public static class UIElementGenerator
    {
        // Default sizes and positions
        private const float DEFAULT_BUTTON_WIDTH = 160f;
        private const float DEFAULT_BUTTON_HEIGHT = 40f;
        private const float DEFAULT_TEXT_WIDTH = 160f;
        private const float DEFAULT_TEXT_HEIGHT = 30f;
        private const float DEFAULT_PANEL_WIDTH = 400f;
        private const float DEFAULT_PANEL_HEIGHT = 300f;
        private const float DEFAULT_IMAGE_SIZE = 100f;

        #region Create Individual UI Elements

        /// <summary>
        /// Creates a Button with TextMeshPro text
        /// </summary>
        public static GameObject CreateButton(string name, Transform parent = null, Vector2? position = null)
        {
            GameObject buttonObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();

            SetupRectTransform(rectTransform, parent, position, new Vector2(DEFAULT_BUTTON_WIDTH, DEFAULT_BUTTON_HEIGHT));

            // Setup Image
            Image image = buttonObj.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 1f);

            // Create text child
            GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.SetParent(rectTransform, false);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI textComponent = textObj.GetComponent<TextMeshProUGUI>();
            textComponent.text = name;
            textComponent.fontSize = 24;
            textComponent.color = Color.black;
            textComponent.alignment = TextAlignmentOptions.Center;

            Undo.RegisterCreatedObjectUndo(buttonObj, "Create Button");
            return buttonObj;
        }

        /// <summary>
        /// Creates a TextMeshProUGUI text element
        /// </summary>
        public static GameObject CreateText(string name, Transform parent = null, Vector2? position = null)
        {
            GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rectTransform = textObj.GetComponent<RectTransform>();

            SetupRectTransform(rectTransform, parent, position, new Vector2(DEFAULT_TEXT_WIDTH, DEFAULT_TEXT_HEIGHT));

            TextMeshProUGUI textComponent = textObj.GetComponent<TextMeshProUGUI>();
            textComponent.text = name;
            textComponent.fontSize = 24;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;

            Undo.RegisterCreatedObjectUndo(textObj, "Create Text");
            return textObj;
        }

        /// <summary>
        /// Creates a Panel (GameObject with Image)
        /// </summary>
        public static GameObject CreatePanel(string name, Transform parent = null, Vector2? position = null, Vector2? size = null)
        {
            GameObject panelObj = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rectTransform = panelObj.GetComponent<RectTransform>();

            Vector2 panelSize = size ?? new Vector2(DEFAULT_PANEL_WIDTH, DEFAULT_PANEL_HEIGHT);
            SetupRectTransform(rectTransform, parent, position, panelSize);

            Image image = panelObj.GetComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            Undo.RegisterCreatedObjectUndo(panelObj, "Create Panel");
            return panelObj;
        }

        /// <summary>
        /// Creates an Image element
        /// </summary>
        public static GameObject CreateImage(string name, Transform parent = null, Vector2? position = null)
        {
            GameObject imageObj = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rectTransform = imageObj.GetComponent<RectTransform>();

            SetupRectTransform(rectTransform, parent, position, new Vector2(DEFAULT_IMAGE_SIZE, DEFAULT_IMAGE_SIZE));

            Image image = imageObj.GetComponent<Image>();
            image.color = Color.white;

            Undo.RegisterCreatedObjectUndo(imageObj, "Create Image");
            return imageObj;
        }

        /// <summary>
        /// Creates a Slider
        /// </summary>
        public static GameObject CreateSlider(string name, Transform parent = null, Vector2? position = null)
        {
            GameObject sliderObj = new GameObject(name, typeof(RectTransform), typeof(Slider));
            RectTransform rectTransform = sliderObj.GetComponent<RectTransform>();

            SetupRectTransform(rectTransform, parent, position, new Vector2(160f, 20f));

            // Background
            GameObject background = CreateImage("Background", rectTransform);
            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.25f);
            bgRect.anchorMax = new Vector2(1, 0.75f);
            bgRect.sizeDelta = Vector2.zero;
            background.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Fill Area
            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.SetParent(rectTransform, false);
            fillAreaRect.anchorMin = new Vector2(0, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1, 0.75f);
            fillAreaRect.sizeDelta = new Vector2(-20, 0);

            GameObject fill = CreateImage("Fill", fillAreaRect);
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fill.GetComponent<Image>().color = Color.green;

            // Handle Slide Area
            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
            handleAreaRect.SetParent(rectTransform, false);
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.sizeDelta = new Vector2(-20, 0);

            GameObject handle = CreateImage("Handle", handleAreaRect);
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 0);
            handle.GetComponent<Image>().color = Color.white;

            // Setup Slider component
            Slider slider = sliderObj.GetComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handle.GetComponent<Image>();

            Undo.RegisterCreatedObjectUndo(sliderObj, "Create Slider");
            return sliderObj;
        }

        /// <summary>
        /// Creates a Toggle
        /// </summary>
        public static GameObject CreateToggle(string name, Transform parent = null, Vector2? position = null)
        {
            GameObject toggleObj = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            RectTransform rectTransform = toggleObj.GetComponent<RectTransform>();

            SetupRectTransform(rectTransform, parent, position, new Vector2(160f, 20f));

            // Background
            GameObject background = CreateImage("Background", rectTransform);
            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = new Vector2(0, 1);
            bgRect.sizeDelta = new Vector2(20, 0);
            background.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f, 1f);

            // Checkmark
            GameObject checkmark = CreateImage("Checkmark", background.transform);
            RectTransform checkRect = checkmark.GetComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.sizeDelta = Vector2.zero;
            checkmark.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 1f);

            // Label
            GameObject label = CreateText("Label", rectTransform);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 0);
            labelRect.anchorMax = new Vector2(1, 1);
            labelRect.offsetMin = new Vector2(23, 0);
            labelRect.offsetMax = Vector2.zero;
            label.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Left;
            label.GetComponent<TextMeshProUGUI>().text = name;

            // Setup Toggle component
            Toggle toggle = toggleObj.GetComponent<Toggle>();
            toggle.graphic = checkmark.GetComponent<Image>();
            toggle.targetGraphic = background.GetComponent<Image>();

            Undo.RegisterCreatedObjectUndo(toggleObj, "Create Toggle");
            return toggleObj;
        }

        /// <summary>
        /// Creates an InputField (TMP_InputField)
        /// </summary>
        public static GameObject CreateInputField(string name, Transform parent = null, Vector2? position = null)
        {
            GameObject inputObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            RectTransform rectTransform = inputObj.GetComponent<RectTransform>();

            SetupRectTransform(rectTransform, parent, position, new Vector2(160f, 30f));

            Image image = inputObj.GetComponent<Image>();
            image.color = Color.white;

            // Text Area
            GameObject textArea = new GameObject("Text Area", typeof(RectTransform));
            RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.SetParent(rectTransform, false);
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.sizeDelta = Vector2.zero;
            textAreaRect.offsetMin = new Vector2(10, 6);
            textAreaRect.offsetMax = new Vector2(-10, -7);

            // Placeholder
            GameObject placeholder = CreateText("Placeholder", textAreaRect);
            RectTransform placeholderRect = placeholder.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI placeholderText = placeholder.GetComponent<TextMeshProUGUI>();
            placeholderText.text = "Enter text...";
            placeholderText.fontSize = 14;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            placeholderText.alignment = TextAlignmentOptions.Left;

            // Text
            GameObject text = CreateText("Text", textAreaRect);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            TextMeshProUGUI textComponent = text.GetComponent<TextMeshProUGUI>();
            textComponent.text = "";
            textComponent.fontSize = 14;
            textComponent.color = Color.black;
            textComponent.alignment = TextAlignmentOptions.Left;

            // Setup InputField
            TMP_InputField inputField = inputObj.GetComponent<TMP_InputField>();
            inputField.textViewport = textAreaRect;
            inputField.textComponent = textComponent;
            inputField.placeholder = placeholderText;

            Undo.RegisterCreatedObjectUndo(inputObj, "Create Input Field");
            return inputObj;
        }

        /// <summary>
        /// Creates a Dropdown (TMP_Dropdown)
        /// </summary>
        public static GameObject CreateDropdown(string name, Transform parent = null, Vector2? position = null)
        {
            GameObject dropdownObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_Dropdown));
            RectTransform rectTransform = dropdownObj.GetComponent<RectTransform>();

            SetupRectTransform(rectTransform, parent, position, new Vector2(160f, 30f));

            Image image = dropdownObj.GetComponent<Image>();
            image.color = Color.white;

            // Label
            GameObject label = CreateText("Label", rectTransform);
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10, 6);
            labelRect.offsetMax = new Vector2(-25, -7);
            TextMeshProUGUI labelText = label.GetComponent<TextMeshProUGUI>();
            labelText.text = "Option A";
            labelText.fontSize = 14;
            labelText.color = Color.black;
            labelText.alignment = TextAlignmentOptions.Left;

            // Arrow
            GameObject arrow = CreateText("Arrow", rectTransform);
            RectTransform arrowRect = arrow.GetComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1, 0.5f);
            arrowRect.anchorMax = new Vector2(1, 0.5f);
            arrowRect.sizeDelta = new Vector2(20, 20);
            arrowRect.anchoredPosition = new Vector2(-15, 0);
            TextMeshProUGUI arrowText = arrow.GetComponent<TextMeshProUGUI>();
            arrowText.text = "▼";
            arrowText.fontSize = 14;
            arrowText.color = Color.black;
            arrowText.alignment = TextAlignmentOptions.Center;

            // Template (simplified)
            GameObject template = CreatePanel("Template", rectTransform, null, new Vector2(160f, 150f));
            template.SetActive(false);
            RectTransform templateRect = template.GetComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.anchoredPosition = new Vector2(0, 2);

            // Setup Dropdown
            TMP_Dropdown dropdown = dropdownObj.GetComponent<TMP_Dropdown>();
            dropdown.captionText = labelText;
            dropdown.template = templateRect;

            Undo.RegisterCreatedObjectUndo(dropdownObj, "Create Dropdown");
            return dropdownObj;
        }

        #endregion

        #region Auto-Generation from UIReference Attributes

        /// <summary>
        /// Automatically generates UI elements based on [UIReference] attributes in a MonoBehaviour
        /// </summary>
        public static List<GameObject> GenerateUIFromComponent(MonoBehaviour component, Transform parent = null)
        {
            if (component == null)
            {
                Debug.LogError("Cannot generate UI: component is null");
                return new List<GameObject>();
            }

            List<GameObject> createdObjects = new List<GameObject>();
            Type componentType = component.GetType();
            FieldInfo[] fields = componentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            Transform targetParent = parent ?? GetOrCreateCanvas().transform;
            Vector2 currentPosition = new Vector2(0, 0);
            float verticalSpacing = 50f;

            foreach (FieldInfo field in fields)
            {
                var attrs = field.GetCustomAttributes(typeof(UIReferenceAttribute), false);
                if (attrs == null || attrs.Length == 0) continue;

                UIReferenceAttribute attribute = (UIReferenceAttribute)attrs[0];

                // Get the name for the GameObject
                string objectName = !string.IsNullOrEmpty(attribute.Name)
                    ? attribute.Name
                    : ConvertFieldNameToObjectName(field.Name);

                // Determine parent based on path
                Transform elementParent = targetParent;
                if (!string.IsNullOrEmpty(attribute.Path))
                {
                    elementParent = GetOrCreatePath(attribute.Path, targetParent);
                }

                // Create the UI element based on field type
                GameObject createdObject = CreateUIElementForType(field.FieldType, objectName, elementParent, currentPosition);

                if (createdObject != null)
                {
                    createdObjects.Add(createdObject);
                    currentPosition.y -= verticalSpacing;

                    Debug.Log($"Created UI element '{objectName}' for field '{field.Name}'");
                }
            }

            return createdObjects;
        }

        /// <summary>
        /// Creates a UI element appropriate for the given type
        /// </summary>
        private static GameObject CreateUIElementForType(Type fieldType, string name, Transform parent, Vector2 position)
        {
            if (fieldType == typeof(Button))
                return CreateButton(name, parent, position);

            if (fieldType == typeof(TextMeshProUGUI))
                return CreateText(name, parent, position);

            if (fieldType == typeof(Image))
                return CreateImage(name, parent, position);

            if (fieldType == typeof(Slider))
                return CreateSlider(name, parent, position);

            if (fieldType == typeof(Toggle))
                return CreateToggle(name, parent, position);

            if (fieldType == typeof(TMP_InputField))
                return CreateInputField(name, parent, position);

            if (fieldType == typeof(TMP_Dropdown))
                return CreateDropdown(name, parent, position);

            if (fieldType == typeof(GameObject))
                return CreatePanel(name, parent, position);

            // Default: create empty GameObject
            GameObject obj = new GameObject(name, typeof(RectTransform));
            SetupRectTransform(obj.GetComponent<RectTransform>(), parent, position, new Vector2(100, 100));
            Undo.RegisterCreatedObjectUndo(obj, "Create GameObject");
            return obj;
        }

        /// <summary>
        /// Gets or creates a path in the hierarchy
        /// </summary>
        private static Transform GetOrCreatePath(string path, Transform root)
        {
            string[] parts = path.Split('/');
            Transform current = root;

            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part)) continue;

                Transform child = current.Find(part);
                if (child == null)
                {
                    // Create the missing part
                    GameObject obj = CreatePanel(part, current);
                    child = obj.transform;
                }
                current = child;
            }

            return current;
        }

        /// <summary>
        /// Converts a field name to a GameObject name
        /// </summary>
        private static string ConvertFieldNameToObjectName(string fieldName)
        {
            // Remove common prefixes
            string[] prefixes = { "_", "m_", "s_" };
            foreach (string prefix in prefixes)
            {
                if (fieldName.StartsWith(prefix))
                    fieldName = fieldName.Substring(prefix.Length);
            }

            // Capitalize first letter
            if (fieldName.Length > 0)
                fieldName = char.ToUpper(fieldName[0]) + fieldName.Substring(1);

            return fieldName;
        }

        #endregion

        #region Canvas Management

        /// <summary>
        /// Gets the Canvas in the scene or creates one if it doesn't exist
        /// </summary>
        public static GameObject GetOrCreateCanvas()
        {
            Canvas canvas = UnityEngine.Object.FindObjectOfType<Canvas>();

            if (canvas != null)
                return canvas.gameObject;

            // Create new Canvas
            GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvasComponent = canvasObj.GetComponent<Canvas>();
            canvasComponent.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            // Create EventSystem if needed
            if (UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
            }

            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            return canvasObj;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Sets up a RectTransform with common settings
        /// </summary>
        private static void SetupRectTransform(RectTransform rectTransform, Transform parent, Vector2? position, Vector2 size)
        {
            if (parent != null)
                rectTransform.SetParent(parent, false);

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position ?? Vector2.zero;
        }

        #endregion
    }
}
