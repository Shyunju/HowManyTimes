using System;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A simple data class that defines a character's expression by linking an expression name
    /// to its corresponding animation state name.
    /// </summary>
    [Serializable]
    public class CharacterExpression
    {
        /// <summary>
        /// The descriptive name of the expression (e.g., 'Happy', 'Angry').
        /// </summary>
        [field: SerializeField] public string ExpressionName { get; private set; } = "Default";
        /// <summary>
        /// The name of the animation state in the character's Animator controller that corresponds to this expression.
        /// </summary>
        [field: SerializeField] public string AnimationStateName { get; private set; } = "Default";

        /// <summary>
        /// Updates the expression data.
        /// /// (Korean) 표정 데이터를 업데이트합니다.
        /// </summary>
        public void SetData(string expressionName, string animationStateName)
        {
            ExpressionName = expressionName;
            AnimationStateName = animationStateName;
        }
    }
}