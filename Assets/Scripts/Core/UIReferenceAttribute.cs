using System;
using UnityEngine;

namespace LAS.Core
{
    /// <summary>
    /// Attribute to mark UI fields for automatic reference binding.
    /// Can specify a custom path or name to find the UI element.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class UIReferenceAttribute : Attribute
    {
        /// <summary>
        /// Optional path to the UI element (e.g., "Canvas/Panel/Button")
        /// If not specified, will search by field name
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Optional GameObject name to search for
        /// If not specified, uses the field name (converted to PascalCase)
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether to search in children only (default: true)
        /// If false, will use FindObjectOfType
        /// </summary>
        public bool SearchInChildren { get; set; } = true;

        /// <summary>
        /// Whether this reference is required (will log error if not found)
        /// </summary>
        public bool Required { get; set; } = true;

        /// <summary>
        /// Tag to filter search by
        /// </summary>
        public string Tag { get; set; }

        public UIReferenceAttribute() { }

        public UIReferenceAttribute(string path)
        {
            Path = path;
        }
    }

    /// <summary>
    /// Attribute to mark a field as the root UI container for reference searches
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class UIRootAttribute : Attribute { }
}
