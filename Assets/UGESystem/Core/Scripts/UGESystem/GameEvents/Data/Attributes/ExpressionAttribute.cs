using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A custom attribute used on string fields that takes the name of a sibling character ID field,
    /// allowing <see cref="Editor.ExpressionDrawer"/> to display a list of relevant expressions.
    /// </summary>
    public class ExpressionAttribute : PropertyAttribute
    {
        /// <summary>
        /// Gets the name of the sibling field that holds the character's ID.
        /// </summary>
        public string CharacterIdFieldName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionAttribute"/>.
        /// </summary>
        /// <param name="characterIdFieldName">The name of the sibling string field that contains the character's ID.</param>
        public ExpressionAttribute(string characterIdFieldName)
        {
            CharacterIdFieldName = characterIdFieldName;
        }
    }
}