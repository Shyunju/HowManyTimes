using UnityEditor;
using UnityEngine;

namespace UGESystem.Editor
{
    /// <summary>
    /// Renders a custom inspector UI for the <see cref="CharacterExpression"/> class,
    /// used when editing a character's expression list.
    /// </summary>
    [CustomPropertyDrawer(typeof(CharacterExpression))]
    public class CharacterExpressionDrawer : PropertyDrawer
    {
        // Approximate height for a 2-line error box
        private const float HelpBoxHeight = 40f; 

        /// <summary>
        /// Draws the custom GUI for a <see cref="CharacterExpression"/> property,
        /// including validation for a missing animation state name and fields for expression name and animation state name.
        /// </summary>
        /// <param name="position">The position and size of the property field.</param>
        /// <param name="property">The <see cref="SerializedProperty"/> to draw.</param>
        /// <param name="label">The label for the property.</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            // Find properties
            var expressionNameProp = property.FindPropertyRelative("<ExpressionName>k__BackingField");
            var animationStateNameProp = property.FindPropertyRelative("<AnimationStateName>k__BackingField");

            bool isAnimationStateNameMissing = string.IsNullOrEmpty(animationStateNameProp.stringValue);

            // Use ExpressionName for Foldout label
            GUIContent foldoutLabel = new GUIContent(string.IsNullOrEmpty(expressionNameProp.stringValue) ? label.text : expressionNameProp.stringValue);

            // Draw Foldout
            Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, foldoutLabel, true);

            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                Rect contentRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);

                // Warning HelpBox if AnimationStateName is missing
                if (isAnimationStateNameMissing)
                {
                    var helpBoxRect = new Rect(contentRect.x, contentRect.y, contentRect.width, HelpBoxHeight);
                    EditorGUI.HelpBox(helpBoxRect, "Animation State Name must be assigned!", MessageType.Error);
                    contentRect.y += HelpBoxHeight;
                }
                
                // Draw properties
                // ExpressionName (editable)
                EditorGUI.PropertyField(new Rect(contentRect.x, contentRect.y, contentRect.width, EditorGUI.GetPropertyHeight(expressionNameProp)), expressionNameProp, new GUIContent("Expression Name"));
                contentRect.y += EditorGUI.GetPropertyHeight(expressionNameProp);

                // AnimationStateName
                EditorGUI.PropertyField(new Rect(contentRect.x, contentRect.y, contentRect.width, EditorGUI.GetPropertyHeight(animationStateNameProp)), animationStateNameProp);
                contentRect.y += EditorGUI.GetPropertyHeight(animationStateNameProp);
                
                EditorGUI.indentLevel--;
            }

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Calculates the total height required for drawing the <see cref="CharacterExpression"/> property in the inspector,
        /// accounting for expanded state and warning messages.
        /// </summary>
        /// <param name="property">The <see cref="SerializedProperty"/> to measure.</param>
        /// <param name="label">The label for the property.</param>
        /// <returns>The total height in pixels.</returns>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight;

            if (property.isExpanded)
            {
                var animationStateNameProp = property.FindPropertyRelative("<AnimationStateName>k__BackingField");
                bool isAnimationStateNameMissing = string.IsNullOrEmpty(animationStateNameProp.stringValue);

                if (isAnimationStateNameMissing)
                {
                    totalHeight += HelpBoxHeight;
                }
                
                totalHeight += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("<ExpressionName>k__BackingField"));
                totalHeight += EditorGUI.GetPropertyHeight(animationStateNameProp);
            }
            
            return totalHeight;
        }
    }
}