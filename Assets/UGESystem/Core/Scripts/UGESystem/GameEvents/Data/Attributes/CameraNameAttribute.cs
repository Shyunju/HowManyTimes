using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A custom attribute that transforms a string field into a dropdown list of Cinemachine camera names present in the scene,
    /// via <see cref="Editor.CameraNameDrawer"/>.
    /// </summary>
    public class CameraNameAttribute : PropertyAttribute
    {
    }
}