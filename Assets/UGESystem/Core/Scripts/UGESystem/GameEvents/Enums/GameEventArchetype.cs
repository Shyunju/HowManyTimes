namespace UGESystem
{
    /// <summary>
    /// Classifies the intended use-cases of a <see cref="GameEvent"/> (Generic, Dialogue, CinematicText),
    /// defining an enumeration that allows for different handlers and editor functionalities for each.
    /// </summary>
    public enum GameEventArchetype
    {
        /// <summary>A general-purpose event archetype.</summary>
        Generic,
        /// <summary>An event primarily focused on displaying dialogue.</summary>
        Dialogue,
        /// <summary>An event designed for cinematic text sequences.</summary>
        CinematicText
    }
}