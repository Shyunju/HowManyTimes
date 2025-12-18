namespace UGESystem
{
    /// <summary>
    /// Defines the type of sound. Used with <see cref="PlaySoundCommand"/>.
    /// </summary>
    public enum SoundType { 
        /// <summary>Background music.</summary>
        BGM, 
        /// <summary>Sound effect.</summary>
        SFX 
    }
    /// <summary>
    /// Defines actions that can be performed on a sound. Used with <see cref="PlaySoundCommand"/>.
    /// </summary>
    public enum SoundAction { 
        /// <summary>Play the sound.</summary>
        Play, 
        /// <summary>Stop the sound.</summary>
        Stop 
    }
}