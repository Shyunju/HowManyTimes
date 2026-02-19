using System;
using System.Collections.Generic;
using UnityEngine;

namespace UGESystem
{
    /// <summary>
    /// A data class that holds all information for a single character,
    /// including ID, name, 3D/2D state, prefab, and a list of expressions.
    /// </summary>
    [Serializable]
    public class CharacterData
    {
        /// <summary>
        /// The unique identifier for this character.
        /// </summary>
        [field: SerializeField] public string CharacterID { get; private set; }
        /// <summary>
        /// The display name of the character.
        /// </summary>
        [field: SerializeField] public string Name { get; private set; }
        /// <summary>
        /// Indicates whether this character uses a 3D model (true) or a 2D sprite (false).
        /// </summary>
        [field: SerializeField] public bool Is3D { get; private set; }

        /// <summary>
        /// The GameObject prefab for this character (either 2D or 3D).
        /// </summary>
        [field: SerializeField, Tooltip("Assign a 2D or 3D character prefab.")]
        public GameObject Prefab { get; private set; }

        /// <summary>
        /// A list of <see cref="CharacterExpression"/> defining the various expressions available for this character.
        /// </summary>
        [field: SerializeField] 
        public List<CharacterExpression> Expressions { get; private set; } = new List<CharacterExpression>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CharacterData"/> class.
        /// Note that primary initialization is handled by the <see cref="CharacterDatabaseEditor"/> for robust serialization.
        /// </summary>
        public CharacterData()
        {
            // Initialization is now handled by CharacterDatabaseEditor to robustly deal with
            // Unity's serialization behavior (e.g., duplicating elements instead of calling constructors).
        }

        /// <summary>
        /// Updates the character's properties, keeping the CharacterID unchanged.
        /// /// (Korean) CharacterID는 유지한 채 캐릭터의 속성을 업데이트합니다.
        /// </summary>
        public void UpdateData(string name, bool is3D, GameObject prefab, List<CharacterExpression> expressions)
        {
            Name = name;
            Is3D = is3D;
            Prefab = prefab;
            
            Expressions.Clear();
            if (expressions != null)
            {
                foreach (var exp in expressions)
                {
                    var newExp = new CharacterExpression();
                    newExp.SetData(exp.ExpressionName, exp.AnimationStateName);
                    Expressions.Add(newExp);
                }
            }
        }

        /// <summary>
        /// Sets the CharacterID. Should be used primarily during initialization or cloning.
        /// /// (Korean) CharacterID를 설정합니다. 주로 초기화나 클로닝 중에 사용해야 합니다.
        /// </summary>
        public void SetID(string id)
        {
            CharacterID = id;
        }
    }
}