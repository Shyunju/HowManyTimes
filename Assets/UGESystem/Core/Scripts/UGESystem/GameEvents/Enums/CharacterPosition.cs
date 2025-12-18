namespace UGESystem
{
    /// <summary>
    /// Defines the <see cref="CharacterPosition"/> enumeration (Left, Center, Right)
    /// that specifies where a character will be placed on the screen.
    /// Used in <see cref="CharacterCommand"/> and <see cref="DialogueCommand"/>.
    /// </summary>
    public enum CharacterPosition 
    { 
        /// <summary>Character is positioned on the left side of the screen.</summary>
        Left, 
        /// <summary>Character is positioned in the center of the screen.</summary>
        Center, 
        /// <summary>Character is positioned on the right side of the screen.</summary>
        Right 
    }
}