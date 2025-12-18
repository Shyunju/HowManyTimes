namespace UGESystem
{
    /// <summary>
    /// Defines the <see cref="UGECameraActionType"/> enumeration (SwitchTo, Zoom, Shake, Reset)
    /// used in <see cref="UGECameraCommand"/>.
    /// </summary>
    public enum UGECameraActionType
    {
        /// <summary>Switch to a specific virtual camera.</summary>
        SwitchTo,       
        /// <summary>Change Field of View (zoom in/out).</summary>
        Zoom,           
        /// <summary>Shake the camera.</summary>
        Shake,          
        /// <summary>Revert to default camera state before event starts.</summary>
        Reset           
    }
}