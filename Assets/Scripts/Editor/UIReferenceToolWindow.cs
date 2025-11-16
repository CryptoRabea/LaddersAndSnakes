using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using LAS.Core;

namespace LAS.Editor
{
    /// <summary>
    /// Editor window for managing UI references in the scene
    /// Access via LAS > UI > UI Reference Tool Window
    /// </summary>
    public class UIReferenceToolWindow : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<MonoBehaviour> uiComponents;
        private Dictionary<MonoBehaviour, bool> foldoutStates = new Dictionary<MonoBehaviour, bool>();
        private UIReferenceBinder.ValidationResult lastValidation;
        private bool autoBindOnLoad = true;
        private bool showSuccessful = false;

        // Removed [MenuItem] - now accessed via LASEditorMenu
        public static void ShowWindow()
        {
            UIReferenceToolWindow window = GetWindow<UIReferenceToolWindow>();
            window.titleContent = new GUIContent("UI Reference Tool");
            window.Show();
        }

        private void OnEnable()
        {
            RefreshComponentList();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            DrawHeader();
            DrawToolbar();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (uiComponents == null || uiComponents.Count == 0)
            {
                EditorGUILayout.HelpBox("No components with [UIReference] attributes found in the scene.", MessageType.Info);
            }
            else
            {
                DrawComponentsList();
            }

            if (lastValidation != null)
            {
                DrawValidationResults();
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("UI Reference Management Tool", headerStyle);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "This tool helps you manage UI element references marked with [UIReference] attribute.\n" +
                "• Bind All: Automatically find and assign all UI references\n" +
                "• Validate: Check all references are properly assigned\n" +
                "• Clear: Remove all assigned references",
                MessageType.Info);

            EditorGUILayout.Space(10);
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                RefreshComponentList();
            }

            if (GUILayout.Button("Bind All", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                BindAllReferences();
            }

            if (GUILayout.Button("Validate All", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                ValidateAllReferences();
            }

            if (GUILayout.Button("Clear All", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                ClearAllReferences();
            }

            GUILayout.FlexibleSpace();

            autoBindOnLoad = GUILayout.Toggle(autoBindOnLoad, "Auto-bind on Load", EditorStyles.toolbarButton, GUILayout.Width(120));
            showSuccessful = GUILayout.Toggle(showSuccessful, "Show Successful", EditorStyles.toolbarButton, GUILayout.Width(120));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
        }

        private void DrawComponentsList()
        {
            EditorGUILayout.LabelField($"Components with UI References ({uiComponents.Count})", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            foreach (MonoBehaviour component in uiComponents)
            {
                if (component == null) continue;

                DrawComponent(component);
            }
        }

        private void DrawComponent(MonoBehaviour component)
        {
            if (!foldoutStates.ContainsKey(component))
                foldoutStates[component] = false;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // Component header
            EditorGUILayout.BeginHorizontal();

            foldoutStates[component] = EditorGUILayout.Foldout(foldoutStates[component],
                $"{component.GetType().Name} ({component.gameObject.name})", true);

            if (GUILayout.Button("Bind", GUILayout.Width(50)))
            {
                BindComponent(component);
            }

            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                Selection.activeGameObject = component.gameObject;
            }

            EditorGUILayout.EndHorizontal();

            // Component details
            if (foldoutStates[component])
            {
                EditorGUI.indentLevel++;
                DrawComponentFields(component);
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private void DrawComponentFields(MonoBehaviour component)
        {
            var fields = component.GetType().GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            bool hasAnyReference = false;

            foreach (var field in fields)
            {
                var attrs = field.GetCustomAttributes(typeof(UIReferenceAttribute), false);
                if (attrs == null || attrs.Length == 0) continue;

                var attribute = (UIReferenceAttribute)attrs[0];
                hasAnyReference = true;
                object value = field.GetValue(component);
                bool isAssigned = value != null && !value.Equals(null);

                EditorGUILayout.BeginHorizontal();

                // Status icon
                GUIStyle iconStyle = new GUIStyle(GUI.skin.label);
                if (isAssigned)
                {
                    EditorGUILayout.LabelField("✓", iconStyle, GUILayout.Width(20));
                    GUI.color = Color.green;
                }
                else
                {
                    EditorGUILayout.LabelField("✗", iconStyle, GUILayout.Width(20));
                    GUI.color = attribute.Required ? Color.red : Color.yellow;
                }

                // Field name and info
                string fieldLabel = field.Name;
                if (!string.IsNullOrEmpty(attribute.Path))
                    fieldLabel += $" (Path: {attribute.Path})";
                else if (!string.IsNullOrEmpty(attribute.Name))
                    fieldLabel += $" (Name: {attribute.Name})";

                EditorGUILayout.LabelField(fieldLabel, GUILayout.MinWidth(150));

                GUI.color = Color.white;

                // Field value
                if (value is UnityEngine.Object unityObj)
                {
                    EditorGUILayout.ObjectField(unityObj, field.FieldType, true);
                }
                else
                {
                    EditorGUILayout.LabelField(isAssigned ? value.ToString() : "NULL", EditorStyles.miniLabel);
                }

                EditorGUILayout.EndHorizontal();
            }

            if (!hasAnyReference)
            {
                EditorGUILayout.LabelField("No UI reference fields found", EditorStyles.miniLabel);
            }
        }

        private void DrawValidationResults()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Validation Results", EditorStyles.boldLabel);

            // Summary
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Total: {lastValidation.TotalChecked}", GUILayout.Width(100));

            GUI.color = Color.green;
            EditorGUILayout.LabelField($"✓ {lastValidation.Success.Count}", GUILayout.Width(60));

            GUI.color = Color.yellow;
            EditorGUILayout.LabelField($"⚠ {lastValidation.Warnings.Count}", GUILayout.Width(60));

            GUI.color = Color.red;
            EditorGUILayout.LabelField($"✗ {lastValidation.Errors.Count}", GUILayout.Width(60));

            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            // Errors
            if (lastValidation.Errors.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Errors:", EditorStyles.boldLabel);
                foreach (var error in lastValidation.Errors)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    GUI.color = Color.red;
                    EditorGUILayout.LabelField("✗", GUILayout.Width(20));
                    GUI.color = Color.white;
                    EditorGUILayout.LabelField($"{error.Component.GetType().Name}.{error.FieldName}");
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = error.Component.gameObject;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            // Warnings
            if (lastValidation.Warnings.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Warnings:", EditorStyles.boldLabel);
                foreach (var warning in lastValidation.Warnings)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    GUI.color = Color.yellow;
                    EditorGUILayout.LabelField("⚠", GUILayout.Width(20));
                    GUI.color = Color.white;
                    EditorGUILayout.LabelField($"{warning.Component.GetType().Name}.{warning.FieldName}");
                    if (GUILayout.Button("Select", GUILayout.Width(60)))
                    {
                        Selection.activeGameObject = warning.Component.gameObject;
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            // Success (optional)
            if (showSuccessful && lastValidation.Success.Count > 0)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Successfully Bound:", EditorStyles.boldLabel);
                foreach (var success in lastValidation.Success)
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    GUI.color = Color.green;
                    EditorGUILayout.LabelField("✓", GUILayout.Width(20));
                    GUI.color = Color.white;
                    EditorGUILayout.LabelField($"{success.Component.GetType().Name}.{success.FieldName}");
                    EditorGUILayout.EndHorizontal();
                }
            }
        }

        private void RefreshComponentList()
        {
            uiComponents = UIReferenceBinder.GetAllUIReferenceBehaviours();
            Repaint();
        }

        private void BindAllReferences()
        {
            if (uiComponents == null || uiComponents.Count == 0)
            {
                EditorUtility.DisplayDialog("No Components", "No components with [UIReference] attributes found.", "OK");
                return;
            }

            int totalBound = 0;
            foreach (MonoBehaviour component in uiComponents)
            {
                if (component != null)
                {
                    Undo.RecordObject(component, "Bind UI References");
                    totalBound += UIReferenceBinder.BindUIReferences(component, false);
                    EditorUtility.SetDirty(component);
                }
            }

            RefreshComponentList();
            EditorUtility.DisplayDialog("Binding Complete",
                $"Successfully bound {totalBound} UI references across {uiComponents.Count} components.", "OK");
        }

        private void ValidateAllReferences()
        {
            lastValidation = UIReferenceBinder.ValidateAllReferences();

            if (lastValidation.HasErrors)
            {
                Debug.LogWarning($"UI Reference Validation: Found {lastValidation.Errors.Count} errors and {lastValidation.Warnings.Count} warnings");
            }
            else
            {
                Debug.Log($"UI Reference Validation: All {lastValidation.TotalChecked} references valid!");
            }

            Repaint();
        }

        private void ClearAllReferences()
        {
            if (!EditorUtility.DisplayDialog("Clear All References",
                "Are you sure you want to clear ALL UI references? This cannot be undone.", "Yes", "Cancel"))
            {
                return;
            }

            if (uiComponents == null) return;

            foreach (MonoBehaviour component in uiComponents)
            {
                if (component == null) continue;

                Undo.RecordObject(component, "Clear UI References");

                var fields = component.GetType().GetFields(
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                foreach (var field in fields)
                {
                    var attrs = field.GetCustomAttributes(typeof(UIReferenceAttribute), false);
                    if (attrs != null && attrs.Length > 0)
                    {
                        field.SetValue(component, null);
                    }
                }

                EditorUtility.SetDirty(component);
            }

            RefreshComponentList();
            EditorUtility.DisplayDialog("Clear Complete", "All UI references have been cleared.", "OK");
        }

        private void BindComponent(MonoBehaviour component)
        {
            Undo.RecordObject(component, "Bind UI References");
            int count = UIReferenceBinder.BindUIReferences(component, true);
            EditorUtility.SetDirty(component);
            RefreshComponentList();
        }
    }
}
