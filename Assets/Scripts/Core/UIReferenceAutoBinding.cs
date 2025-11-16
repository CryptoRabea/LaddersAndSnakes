using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace LaddersAndSnakes.Core
{
    /// <summary>
    /// Component that automatically binds UI references at runtime
    /// Add this to any GameObject that has components with [UIReference] attributes
    /// </summary>
    public class UIReferenceAutoBinding : MonoBehaviour
    {
        [Header("Auto Binding Settings")]
        [Tooltip("Bind references in Awake (earlier) or Start (later)")]
        public bool bindInAwake = true;

        [Tooltip("Log binding results to console")]
        public bool logResults = false;

        [Tooltip("Automatically bind all MonoBehaviours on this GameObject")]
        public bool bindAllComponents = true;

        [Tooltip("If not bindAllComponents, specify which components to bind")]
        public MonoBehaviour[] specificComponents;

        private void Awake()
        {
            if (bindInAwake)
            {
                BindReferences();
            }
        }

        private void Start()
        {
            if (!bindInAwake)
            {
                BindReferences();
            }
        }

        private void BindReferences()
        {
            if (bindAllComponents)
            {
                MonoBehaviour[] components = GetComponents<MonoBehaviour>();
                foreach (MonoBehaviour component in components)
                {
                    if (component != null && component != this)
                    {
                        UIReferenceBinder.BindUIReferences(component, logResults);
                    }
                }
            }
            else if (specificComponents != null)
            {
                foreach (MonoBehaviour component in specificComponents)
                {
                    if (component != null)
                    {
                        UIReferenceBinder.BindUIReferences(component, logResults);
                    }
                }
            }
        }

        /// <summary>
        /// Manually trigger rebinding of references
        /// </summary>
        public void RebindReferences()
        {
            BindReferences();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-populate specificComponents if not binding all
            if (!bindAllComponents && (specificComponents == null || specificComponents.Length == 0))
            {
                MonoBehaviour[] components = GetComponents<MonoBehaviour>();
                System.Collections.Generic.List<MonoBehaviour> validComponents = new System.Collections.Generic.List<MonoBehaviour>();

                foreach (MonoBehaviour component in components)
                {
                    if (component != null && component != this)
                    {
                        // Check if component has any UIReference attributes
                        var fields = component.GetType().GetFields(
                            System.Reflection.BindingFlags.Public |
                            System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Instance);

                        foreach (var field in fields)
                        {
                            var attrs = field.GetCustomAttributes(typeof(UIReferenceAttribute), false);
                            if (attrs != null && attrs.Length > 0)
                            {
                                validComponents.Add(component);
                                break;
                            }
                        }
                    }
                }

                specificComponents = validComponents.ToArray();
            }
        }
#endif
    }
}
