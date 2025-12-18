namespace UGESystem
{
    /// <summary>
    /// Defines the <see cref="CharacterAction"/> enumeration (Show, Hide, ChangeExpression)
    /// used by <see cref="CharacterCommand"/> to specify character manipulations.
    /// </summary>
    public enum CharacterAction 
    { 
        /// <summary>Display the character.</summary>
        Show, 
        /// <summary>Hide the character.</summary>
        Hide, 
        /// <summary>Change the character's expression.</summary>
        ChangeExpression 
    }
}