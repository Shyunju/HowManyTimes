using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A marker attribute for identifying string fields that will hold the ID of a <see cref="UGEEventTaskRunner"/>,
    /// to be used with <see cref="Editor.PropertyDrawers.RunnerIdDrawer"/>.
    /// </summary>
    public class RunnerIdAttribute : PropertyAttribute { }
}