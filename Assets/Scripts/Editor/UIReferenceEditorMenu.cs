using UnityEditor;
using UnityEngine;
using LaddersAndSnakes.Core;
using System.Collections.Generic;

namespace LaddersAndSnakes.Editor
{
    /// <summary>
    /// Editor menu items and utilities for UI Reference system
    /// </summary>
    public static class UIReferenceEditorMenu
    {
        [MenuItem("Tools/Ladders & Snakes/Bind All UI References in Scene")]
        public static void BindAllUIReferencesInScene()
        {
            List<MonoBehaviour> components = UIReferenceBinder.GetAllUIReferenceBehaviours();

            if (components.Count == 0)
            {
                EditorUtility.DisplayDialog("No Components Found",
                    "No components with [UIReference] attributes found in the current scene.", "OK");
                return;
            }

            int totalBound = 0;
            foreach (MonoBehaviour component in components)
            {
                if (component != null)
                {
                    Undo.RecordObject(component, "Bind UI References");
                    totalBound += UIReferenceBinder.BindUIReferences(component, false);
                    EditorUtility.SetDirty(component);
                }
            }

            EditorUtility.DisplayDialog("Binding Complete",
                $"Successfully bound {totalBound} UI references across {components.Count} components.", "OK");
        }

        [MenuItem("Tools/Ladders & Snakes/Validate All UI References")]
        public static void ValidateAllUIReferences()
        {
            UIReferenceBinder.ValidationResult result = UIReferenceBinder.ValidateAllReferences();

            string message = $"Validation Complete:\n\n" +
                           $"Total References: {result.TotalChecked}\n" +
                           $"✓ Success: {result.Success.Count}\n" +
                           $"⚠ Warnings: {result.Warnings.Count}\n" +
                           $"✗ Errors: {result.Errors.Count}";

            if (result.HasErrors)
            {
                Debug.LogWarning("UI Reference Validation failed with errors:");
                foreach (var error in result.Errors)
                {
                    Debug.LogError($"  {error.Component.GetType().Name}.{error.FieldName}: {error.Message}", error.Component);
                }
            }

            MessageType messageType = result.HasErrors ? MessageType.Error :
                                     result.Warnings.Count > 0 ? MessageType.Warning :
                                     MessageType.Info;

            EditorUtility.DisplayDialog(
                result.HasErrors ? "Validation Failed" : "Validation Complete",
                message,
                "OK");
        }

        [MenuItem("Tools/Ladders & Snakes/Bind Selected GameObject UI References")]
        public static void BindSelectedUIReferences()
        {
            if (Selection.activeGameObject == null)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select a GameObject first.", "OK");
                return;
            }

            MonoBehaviour[] components = Selection.activeGameObject.GetComponents<MonoBehaviour>();
            int totalBound = 0;
            int componentsWithRefs = 0;

            foreach (MonoBehaviour component in components)
            {
                if (component != null)
                {
                    Undo.RecordObject(component, "Bind UI References");
                    int bound = UIReferenceBinder.BindUIReferences(component, true);
                    if (bound > 0)
                    {
                        componentsWithRefs++;
                        totalBound += bound;
                        EditorUtility.SetDirty(component);
                    }
                }
            }

            if (componentsWithRefs == 0)
            {
                EditorUtility.DisplayDialog("No References",
                    "No [UIReference] attributes found on the selected GameObject.", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Binding Complete",
                    $"Bound {totalBound} references on {componentsWithRefs} component(s).", "OK");
            }
        }

        [MenuItem("Tools/Ladders & Snakes/Add Auto-Binding Component")]
        public static void AddAutoBindingComponent()
        {
            if (Selection.activeGameObject == null)
            {
                EditorUtility.DisplayDialog("No Selection", "Please select a GameObject first.", "OK");
                return;
            }

            if (Selection.activeGameObject.GetComponent<UIReferenceAutoBinding>() != null)
            {
                EditorUtility.DisplayDialog("Already Exists",
                    "This GameObject already has a UIReferenceAutoBinding component.", "OK");
                return;
            }

            Undo.AddComponent<UIReferenceAutoBinding>(Selection.activeGameObject);
            EditorUtility.DisplayDialog("Component Added",
                "UIReferenceAutoBinding component added successfully.", "OK");
        }

        [MenuItem("GameObject/UI/Add UI Reference Auto-Binding", false, 10)]
        public static void AddAutoBindingViaGameObjectMenu()
        {
            AddAutoBindingComponent();
        }
    }

    /// <summary>
    /// Custom inspector for components with UI references
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class UIReferenceInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MonoBehaviour monoBehaviour = (MonoBehaviour)target;

            // Check if this component has any UIReference attributes
            var fields = monoBehaviour.GetType().GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            bool hasUIReferences = false;
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<UIReferenceAttribute>() != null)
                {
                    hasUIReferences = true;
                    break;
                }
            }

            if (hasUIReferences)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("UI Reference Tools", EditorStyles.boldLabel);

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Bind References"))
                {
                    Undo.RecordObject(monoBehaviour, "Bind UI References");
                    int count = UIReferenceBinder.BindUIReferences(monoBehaviour, true);
                    EditorUtility.SetDirty(monoBehaviour);
                    Debug.Log($"Bound {count} UI references on {monoBehaviour.GetType().Name}");
                }

                if (GUILayout.Button("Validate"))
                {
                    UIReferenceBinder.ValidationResult result = new UIReferenceBinder.ValidationResult();
                    UIReferenceBinder.ValidateComponent(monoBehaviour, result);

                    if (result.HasErrors)
                    {
                        Debug.LogError($"Validation failed: {result.Errors.Count} errors found", monoBehaviour);
                        foreach (var error in result.Errors)
                        {
                            Debug.LogError($"  {error.FieldName}: {error.Message}", monoBehaviour);
                        }
                    }
                    else
                    {
                        Debug.Log($"Validation passed: {result.Success.Count} references OK", monoBehaviour);
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }
    }

    /// <summary>
    /// Runs validation when entering play mode
    /// </summary>
    [InitializeOnLoad]
    public static class PlayModeValidator
    {
        static PlayModeValidator()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                // Validate before entering play mode
                UIReferenceBinder.ValidationResult result = UIReferenceBinder.ValidateAllReferences();

                if (result.HasErrors)
                {
                    Debug.LogWarning($"[UI Reference] Entering play mode with {result.Errors.Count} unassigned required references!");
                    foreach (var error in result.Errors)
                    {
                        Debug.LogError($"  {error.Component.GetType().Name}.{error.FieldName}: {error.Message}", error.Component);
                    }

                    // Optionally prevent entering play mode
                    // EditorApplication.isPlaying = false;
                }
            }
        }
    }
}
