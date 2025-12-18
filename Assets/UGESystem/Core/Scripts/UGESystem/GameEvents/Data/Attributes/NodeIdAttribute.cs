using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A marker attribute for identifying string fields that will hold the unique ID of an <see cref="EventNodeData"/>,
    /// to be used with <see cref="Editor.NodeIdDrawer"/>.
    /// </summary>
    public class NodeIdAttribute : PropertyAttribute
    {
        // This is a marker attribute. No additional properties are needed here.
    }
}