using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UGESystem.Editor
{
    /// <summary>
    /// An abstract base class for creating efficient dropdown-style property drawers that cache option lists
    /// to improve Unity editor performance.
    /// </summary>
    public abstract class CachedPopupDrawer : PropertyDrawer
    {
        /// <summary>
        /// Represents a single option for a popup dropdown in the editor,
        /// containing a display name and its corresponding value.
        /// </summary>
        public struct PopupOption
        {
            /// <summary>
            /// The name to display in the popup.
            /// </summary>
            public string DisplayName;
            /// <summary>
            /// The actual string value associated with the option.
            /// </summary>
            public string Value;
        }
        
        private static readonly Dictionary<string, (List<PopupOption> options, float lastUpdateTime)> _cache = 
            new Dictionary<string, (List<PopupOption> options, float lastUpdateTime)>();
        
        private const float CACHE_REFRESH_INTERVAL = 1.0f;

        /// <summary>
        /// Retrieves a list of string options for the popup.
        /// This method is for backward compatibility; <see cref="GetAdvancedOptions"/> should be preferred.
        /// </summary>
        /// <param name="property">The <see cref="SerializedProperty"/> for which to get options.</param>
        /// <returns>A list of string options, or <c>null</c> if <see cref="GetAdvancedOptions"/> is used.</returns>
        protected virtual List<string> GetOptions(SerializedProperty property) => null;
        
        /// <summary>
        /// Retrieves a list of <see cref="PopupOption"/> objects for the popup,
        /// allowing for separate display names and values. Override this method in derived classes.
        /// </summary>
        /// <param name="property">The <see cref="SerializedProperty"/> for which to get advanced options.</param>
        /// <returns>A list of <see cref="PopupOption"/>, or <c>null</c> if <see cref="GetOptions"/> is used.</returns>
        protected virtual List<PopupOption> GetAdvancedOptions(SerializedProperty property) => null;

        /// <summary>
        /// Generates a unique cache key for the property drawer based on its type and the property path.
        /// </summary>
        /// <param name="property">The <see cref="SerializedProperty"/> for which to generate the cache key.</param>
        /// <returns>A unique string key for caching purposes.</returns>
        protected virtual string GetCacheKey(SerializedProperty property)
        {
            return $"{GetType().FullName}_{property.propertyPath}";
        }
        
        /// <summary>
        /// Draws the custom GUI for the property, presenting a cached dropdown list of options
        /// derived from <see cref="GetOptions"/> or <see cref="GetAdvancedOptions"/>.
        /// </summary>
        /// <param name="position">The position and size of the property field.</param>
        /// <param name="property">The <see cref="SerializedProperty"/> to draw.</param>
        /// <param name="label">The label for the property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "This attribute can only be used on string types.");
                return;
            }

            string cacheKey = GetCacheKey(property);
            
            if (!_cache.TryGetValue(cacheKey, out var cacheEntry) || Time.realtimeSinceStartup > cacheEntry.lastUpdateTime + CACHE_REFRESH_INTERVAL)
            {
                List<PopupOption> options = GetAdvancedOptions(property);
                if (options == null)
                {
                    // Fallback to old method for backward compatibility
                    var stringOptions = GetOptions(property) ?? new List<string>();
                    options = stringOptions.Select(o => new PopupOption { DisplayName = o, Value = o }).ToList();
                }

                if (options.Count > 0 && options[0].Value != "None")
                {
                    options.Insert(0, new PopupOption { DisplayName = "None", Value = string.Empty });
                }
                else if (options.Count == 0)
                {
                    options.Add(new PopupOption { DisplayName = "None", Value = string.Empty });
                }
                
                cacheEntry = (options, Time.realtimeSinceStartup);
                _cache[cacheKey] = cacheEntry;
            }

            var cachedOptions = cacheEntry.options;
            string currentValue = property.stringValue;
            int currentIndex = cachedOptions.FindIndex(o => o.Value == currentValue);
            
            var displayNames = cachedOptions.Select(o => o.DisplayName).ToList();

            bool isInvalid = false;
            if (currentIndex < 0 && !string.IsNullOrEmpty(currentValue))
            {
                isInvalid = true;
                displayNames.Insert(1, $"(Invalid) {currentValue}");
                currentIndex = 1;
            }
            
            EditorGUI.BeginChangeCheck();

            Color originalColor = GUI.backgroundColor;
            if (isInvalid)
            {
                GUI.backgroundColor = new Color(1f, 0.6f, 0.6f, 1f);
            }

            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, displayNames.ToArray());

            if (isInvalid)
            {
                GUI.backgroundColor = originalColor;
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                if (isInvalid && newIndex == 1) 
                {
                    // User re-selected the invalid entry, do nothing.
                }
                else
                {
                    property.stringValue = cachedOptions[newIndex].Value;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}