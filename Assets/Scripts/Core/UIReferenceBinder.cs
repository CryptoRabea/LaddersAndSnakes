using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace LaddersAndSnakes.Core
{
    /// <summary>
    /// Utility class for automatically binding UI references marked with UIReferenceAttribute
    /// </summary>
    public static class UIReferenceBinder
    {
        /// <summary>
        /// Helper method to get custom attribute in a Unity-compatible way
        /// </summary>
        private static T GetCustomAttribute<T>(this FieldInfo field) where T : Attribute
        {
            var attrs = field.GetCustomAttributes(typeof(T), false);
            return attrs != null && attrs.Length > 0 ? (T)attrs[0] : null;
        }

        /// <summary>
        /// Binds all fields marked with [UIReference] attribute on the given component
        /// </summary>
        /// <param name="component">Component to bind references for</param>
        /// <param name="logResults">Whether to log binding results</param>
        /// <returns>Number of references successfully bound</returns>
        public static int BindUIReferences(MonoBehaviour component, bool logResults = true)
        {
            if (component == null)
            {
                Debug.LogError("UIReferenceBinder: Component is null");
                return 0;
            }

            Type type = component.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            Transform rootTransform = GetUIRoot(component, fields);
            int boundCount = 0;
            int totalCount = 0;

            foreach (FieldInfo field in fields)
            {
                UIReferenceAttribute attribute = field.GetCustomAttribute<UIReferenceAttribute>();
                if (attribute == null) continue;

                totalCount++;

                // Skip if already assigned
                object currentValue = field.GetValue(component);
                if (currentValue != null && !currentValue.Equals(null))
                {
                    if (logResults)
                        Debug.Log($"UIReferenceBinder: Skipped '{field.Name}' (already assigned) on {component.name}");
                    boundCount++;
                    continue;
                }

                // Try to find and bind the UI element
                UnityEngine.Object foundObject = FindUIElement(field, attribute, component.gameObject, rootTransform);

                if (foundObject != null)
                {
                    field.SetValue(component, foundObject);
                    boundCount++;
                    if (logResults)
                        Debug.Log($"UIReferenceBinder: Bound '{field.Name}' to {foundObject.name} on {component.name}");
                }
                else
                {
                    if (attribute.Required)
                    {
                        Debug.LogError($"UIReferenceBinder: Required field '{field.Name}' not found on {component.name}");
                    }
                    else if (logResults)
                    {
                        Debug.LogWarning($"UIReferenceBinder: Optional field '{field.Name}' not found on {component.name}");
                    }
                }
            }

            if (logResults && totalCount > 0)
            {
                Debug.Log($"UIReferenceBinder: Bound {boundCount}/{totalCount} UI references on {component.name}");
            }

            return boundCount;
        }

        /// <summary>
        /// Gets the UI root transform for searching, either from UIRoot attribute or the component's transform
        /// </summary>
        private static Transform GetUIRoot(MonoBehaviour component, FieldInfo[] fields)
        {
            foreach (FieldInfo field in fields)
            {
                if (field.GetCustomAttribute<UIRootAttribute>() != null)
                {
                    object value = field.GetValue(component);
                    if (value is Transform transform)
                        return transform;
                    if (value is GameObject gameObject)
                        return gameObject.transform;
                    if (value is Component comp)
                        return comp.transform;
                }
            }

            return component.transform;
        }

        /// <summary>
        /// Finds a UI element based on field info and attribute settings
        /// </summary>
        private static UnityEngine.Object FindUIElement(FieldInfo field, UIReferenceAttribute attribute, GameObject root, Transform searchRoot)
        {
            Type fieldType = field.FieldType;
            string searchName = GetSearchName(field.Name, attribute);

            // Try path-based search first
            if (!string.IsNullOrEmpty(attribute.Path))
            {
                Transform found = searchRoot.Find(attribute.Path);
                if (found != null)
                    return GetComponentOfType(found.gameObject, fieldType);
            }

            // Try name-based search
            if (attribute.SearchInChildren)
            {
                return FindInChildren(searchRoot, searchName, fieldType, attribute.Tag);
            }
            else
            {
                return FindInScene(searchName, fieldType, attribute.Tag);
            }
        }

        /// <summary>
        /// Gets the name to search for based on field name and attribute
        /// </summary>
        private static string GetSearchName(string fieldName, UIReferenceAttribute attribute)
        {
            if (!string.IsNullOrEmpty(attribute.Name))
                return attribute.Name;

            // Convert field name to object name (e.g., rollDiceButton -> RollDiceButton)
            return ConvertFieldNameToObjectName(fieldName);
        }

        /// <summary>
        /// Converts a field name to a likely GameObject name
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

        /// <summary>
        /// Searches for UI element in children
        /// </summary>
        private static UnityEngine.Object FindInChildren(Transform root, string name, Type componentType, string tag = null)
        {
            // First try exact name match
            Transform exactMatch = FindChildRecursive(root, name);
            if (exactMatch != null && (string.IsNullOrEmpty(tag) || exactMatch.CompareTag(tag)))
            {
                UnityEngine.Object component = GetComponentOfType(exactMatch.gameObject, componentType);
                if (component != null)
                    return component;
            }

            // Try case-insensitive search
            Component[] allComponents = root.GetComponentsInChildren(componentType, true);
            foreach (Component comp in allComponents)
            {
                if (comp.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(tag) || comp.CompareTag(tag))
                        return comp;
                }
            }

            return null;
        }

        /// <summary>
        /// Recursively finds a child by name with depth limiting to prevent stack overflow
        /// </summary>
        private static Transform FindChildRecursive(Transform parent, string name, int maxDepth = 50, int currentDepth = 0)
        {
            // Prevent stack overflow by limiting recursion depth
            if (currentDepth >= maxDepth)
                return null;

            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;

                Transform found = FindChildRecursive(child, name, maxDepth, currentDepth + 1);
                if (found != null)
                    return found;
            }

            return null;
        }

        /// <summary>
        /// Searches for UI element in entire scene
        /// </summary>
        private static UnityEngine.Object FindInScene(string name, Type componentType, string tag = null)
        {
            UnityEngine.Object[] allObjects = UnityEngine.Object.FindObjectsOfType(componentType, true);

            foreach (UnityEngine.Object obj in allObjects)
            {
                Component comp = obj as Component;
                if (comp != null && comp.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(tag) || comp.CompareTag(tag))
                        return comp;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets component of the specified type from a GameObject
        /// </summary>
        private static UnityEngine.Object GetComponentOfType(GameObject obj, Type type)
        {
            if (type == typeof(GameObject))
                return obj;

            if (type.IsSubclassOf(typeof(Component)))
                return obj.GetComponent(type);

            return null;
        }

        /// <summary>
        /// Gets all MonoBehaviours in the scene that have UIReference attributes
        /// </summary>
        public static List<MonoBehaviour> GetAllUIReferenceBehaviours()
        {
            List<MonoBehaviour> result = new List<MonoBehaviour>();
            MonoBehaviour[] allBehaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);

            foreach (MonoBehaviour behaviour in allBehaviours)
            {
                Type type = behaviour.GetType();
                FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (FieldInfo field in fields)
                {
                    if (field.GetCustomAttribute<UIReferenceAttribute>() != null)
                    {
                        result.Add(behaviour);
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Validates all UI references in the scene
        /// </summary>
        public static ValidationResult ValidateAllReferences()
        {
            ValidationResult result = new ValidationResult();
            List<MonoBehaviour> behaviours = GetAllUIReferenceBehaviours();

            foreach (MonoBehaviour behaviour in behaviours)
            {
                ValidateComponent(behaviour, result);
            }

            return result;
        }

        /// <summary>
        /// Validates UI references on a specific component
        /// </summary>
        public static void ValidateComponent(MonoBehaviour component, ValidationResult result)
        {
            Type type = component.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                UIReferenceAttribute attribute = field.GetCustomAttribute<UIReferenceAttribute>();
                if (attribute == null) continue;

                object value = field.GetValue(component);
                bool isAssigned = value != null && !value.Equals(null);

                if (!isAssigned && attribute.Required)
                {
                    result.AddError(component, field.Name, "Required reference is null");
                }
                else if (isAssigned)
                {
                    result.AddSuccess(component, field.Name);
                }
                else
                {
                    result.AddWarning(component, field.Name, "Optional reference is null");
                }
            }
        }

        public class ValidationResult
        {
            public List<ValidationEntry> Errors = new List<ValidationEntry>();
            public List<ValidationEntry> Warnings = new List<ValidationEntry>();
            public List<ValidationEntry> Success = new List<ValidationEntry>();

            public void AddError(MonoBehaviour component, string fieldName, string message)
            {
                Errors.Add(new ValidationEntry { Component = component, FieldName = fieldName, Message = message });
            }

            public void AddWarning(MonoBehaviour component, string fieldName, string message)
            {
                Warnings.Add(new ValidationEntry { Component = component, FieldName = fieldName, Message = message });
            }

            public void AddSuccess(MonoBehaviour component, string fieldName)
            {
                Success.Add(new ValidationEntry { Component = component, FieldName = fieldName, Message = "OK" });
            }

            public bool HasErrors => Errors.Count > 0;
            public int TotalChecked => Errors.Count + Warnings.Count + Success.Count;
        }

        public class ValidationEntry
        {
            public MonoBehaviour Component;
            public string FieldName;
            public string Message;
        }
    }
}
