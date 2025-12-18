using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A custom attribute that transforms a string field into a dropdown list of all character IDs
    /// from the <see cref="CharacterDatabase"/>, via <see cref="Editor.CharacterIdDrawer"/>.
    /// </summary>
    public class CharacterIdAttribute : PropertyAttribute { }
}
