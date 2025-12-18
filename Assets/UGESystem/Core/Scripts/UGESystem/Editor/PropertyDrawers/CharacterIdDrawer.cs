using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace UGESystem.Editor
{
    /// <summary>
    /// A custom property drawer for the <see cref="CharacterIdAttribute"/>,
    /// displaying a dropdown menu populated with all character names from the <see cref="CharacterDatabase"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(CharacterIdAttribute))]
    public class CharacterIdDrawer : CachedPopupDrawer
    {
        /// <summary>
        /// Overrides the base method to retrieve a list of character IDs and their display names from the <see cref="CharacterDatabase"/>.
        /// </summary>
        /// <param name="property">The <see cref="SerializedProperty"/> that this drawer is for.</param>
        /// <returns>A list of <see cref="CachedPopupDrawer.PopupOption"/> representing character IDs and names.</returns>
        protected override List<PopupOption> GetAdvancedOptions(SerializedProperty property)
        {
            string[] guids = AssetDatabase.FindAssets("t:CharacterDatabase");
            if (guids.Length == 0)
            {
                return new List<PopupOption> { new PopupOption { DisplayName = "CharacterDatabase not found", Value = "" } };
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            CharacterDatabase database = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(path);
            
            if (database == null || database.Characters == null)
            {
                return new List<PopupOption> { new PopupOption { DisplayName = "Failed to load CharacterDatabase", Value = "" } };
            }

            return database.Characters
                .Select(c => new PopupOption 
                { 
                    DisplayName = string.IsNullOrEmpty(c.Name) ? $"(ID: {c.CharacterID.Substring(0, 8)}...)" : c.Name, 
                    Value = c.CharacterID 
                })
                .ToList();
        }
    }
}
