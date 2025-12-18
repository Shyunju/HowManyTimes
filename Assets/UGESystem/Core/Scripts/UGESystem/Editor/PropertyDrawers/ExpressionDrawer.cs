using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace UGESystem.Editor
{
    /// <summary>
    /// A custom property drawer for the <see cref="ExpressionAttribute"/>,
    /// dynamically populating a dropdown with available expressions for the character
    /// selected in a <see cref="CharacterIdAttribute"/> field within the same object.
    /// </summary>
    [CustomPropertyDrawer(typeof(ExpressionAttribute))]
    public class ExpressionDrawer : CachedPopupDrawer
    {
        /// <summary>
        /// Overrides the base method to retrieve a list of available expressions for the character
        /// specified by the <see cref="CharacterIdAttribute"/> field in the same object.
        /// </summary>
        /// <param name="property">The <see cref="SerializedProperty"/> that this drawer is for.</param>
        /// <returns>A list of <see cref="CachedPopupDrawer.PopupOption"/> representing character expressions.</returns>
        protected override List<PopupOption> GetAdvancedOptions(SerializedProperty property)
        {
            var characterIdProperty = FindCharacterIdProperty(property);
            if (characterIdProperty == null || string.IsNullOrEmpty(characterIdProperty.stringValue))
            {
                return new List<PopupOption> { new PopupOption{ DisplayName = "Select a character first", Value = "" } };
            }
            
            string characterId = characterIdProperty.stringValue;

            string[] guids = AssetDatabase.FindAssets("t:CharacterDatabase");
            if (guids.Length == 0)
            {
                return new List<PopupOption> { new PopupOption{ DisplayName = "CharacterDatabase not found", Value = "" } };
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            CharacterDatabase database = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(path);
            if (database == null)
            {
                return new List<PopupOption> { new PopupOption{ DisplayName = "Failed to load CharacterDatabase", Value = "" } };
            }
            
            CharacterData characterData = database.GetCharacterData(characterId);

            if (characterData != null && characterData.Expressions != null)
            {
                return characterData.Expressions
                    .Where(e => !string.IsNullOrEmpty(e.ExpressionName))
                    .Select(e => new PopupOption { DisplayName = e.ExpressionName, Value = e.ExpressionName })
                    .ToList();
            }

            return new List<PopupOption>();
        }

        /// <summary>
        /// Generates a unique cache key for the property drawer, incorporating the <c>characterId</c>
        /// to ensure the cache is refreshed when the selected character changes.
        /// </summary>
        /// <param name="property">The <see cref="SerializedProperty"/> for which to generate the cache key.</param>
        /// <returns>A unique string key for caching purposes, dependent on the selected character.</returns>
        protected override string GetCacheKey(SerializedProperty property)
        {
            var characterIdProperty = FindCharacterIdProperty(property);
            string characterId = characterIdProperty?.stringValue ?? "null";
            return $"{GetType().FullName}_{property.propertyPath}_{characterId}";
        }

        /// <summary>
        /// Helper method to find the <see cref="SerializedProperty"/> for the character ID field based on the attribute's <see cref="ExpressionAttribute.CharacterIdFieldName"/>.
        /// </summary>
        /// <param name="property">The current <see cref="SerializedProperty"/> being drawn.</param>
        /// <returns>The <see cref="SerializedProperty"/> for the character ID, or <c>null</c> if not found.</returns>
        private SerializedProperty FindCharacterIdProperty(SerializedProperty property)
        {
            var expressionAttribute = (ExpressionAttribute)attribute;
            string characterIdFieldName = expressionAttribute.CharacterIdFieldName;
            
            int lastDot = property.propertyPath.LastIndexOf('.');
            string basePath = lastDot > 0 ? property.propertyPath.Substring(0, lastDot + 1) : "";
            string characterIdPath = basePath + characterIdFieldName;
            
            return property.serializedObject.FindProperty(characterIdPath);
        }
    }
}