using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A marker attribute for identifying string fields that will hold the name of an <see cref="EventNodeData"/>,
    /// to be used with <see cref="Editor.PropertyDrawers.NodeNameDrawer"/>.
    /// </summary>
    public class NodeNameAttribute : PropertyAttribute
    {
    }
}