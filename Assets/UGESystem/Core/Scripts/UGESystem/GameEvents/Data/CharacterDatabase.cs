using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace UGESystem
{
    /// <summary>
    /// A ScriptableObject that acts as a central database for all <see cref="CharacterData"/> in the project,
    /// providing lookup methods and JSON serialization.
    /// </summary>
    //[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "UGESystem/Character Database")]
    public class CharacterDatabase : ScriptableObject
    {
        // Internal DTOs for clean serialization
        private class CharacterDataDto
        {
            public string CharacterID;
            public string Name;
            public bool Is3D;
            public List<CharacterExpressionDto> Expressions;
        }

        private class CharacterExpressionDto
        {
            public string ExpressionName;
            public string AnimationStateName;
        }

        [field: SerializeField]
        private List<CharacterData> _characters = new List<CharacterData>();
        /// <summary>
        /// Gets a list of all <see cref="CharacterData"/> entries in the database.
        /// </summary>
        public List<CharacterData> Characters => _characters;

        /// <summary>
        /// Retrieves <see cref="CharacterData"/> for a specific character by their unique ID.
        /// </summary>
        /// <param name="characterID">The unique identifier of the character.</param>
        /// <returns>The <see cref="CharacterData"/> if found; otherwise, <c>null</c>.</returns>
        public CharacterData GetCharacterData(string characterID)
        {
            return _characters.FirstOrDefault(c => c.CharacterID == characterID);
        }

        /// <summary>
        /// Retrieves a list of all unique character IDs present in the database.
        /// </summary>
        /// <returns>A list of character ID strings.</returns>
        public List<string> GetAllCharacterIDs()
        {
            return _characters.Select(c => c.CharacterID).ToList();
        }

        /// <summary>
        /// Fully updates a target character's data using information from another character data object.
        /// /// (Korean) 다른 캐릭터 데이터 객체의 정보를 사용하여 대상 캐릭터의 데이터를 완전히 업데이트합니다.
        /// </summary>
        /// <param name="targetID">The ID of the character to update. /// (Korean) 업데이트할 캐릭터의 ID입니다.</param>
        /// <param name="sourceData">The source data to copy from. /// (Korean) 복사해올 원본 데이터입니다.</param>
        public void UpdateCharacter(string targetID, CharacterData sourceData)
        {
            var target = GetCharacterData(targetID);
            if (target != null && sourceData != null)
            {
                target.UpdateData(sourceData.Name, sourceData.Is3D, sourceData.Prefab, sourceData.Expressions);
            }
        }

        /// <summary>
        /// Serializes the entire <see cref="CharacterDatabase"/> content into a JSON string.
        /// </summary>
        /// <returns>A JSON string representation of the character database.</returns>
        public string ToJson()
        {
            var dtoList = _characters.Select(c => new CharacterDataDto
            {
                CharacterID = c.CharacterID,
                Name = c.Name,
                Is3D = c.Is3D,
                Expressions = c.Expressions.Select(e => new CharacterExpressionDto
                {
                    ExpressionName = e.ExpressionName,
                    AnimationStateName = e.AnimationStateName
                }).ToList()
            }).ToList();

            return JsonConvert.SerializeObject(dtoList, Formatting.Indented);
        }

        /// <summary>
        /// Deserializes character data from a JSON string,
        /// updating existing entries and adding new ones based on character IDs.
        /// </summary>
        /// <param name="json">The JSON string containing character data.</param>
        public void FromJson(string json)
        {
            var dtoList = JsonConvert.DeserializeObject<List<CharacterDataDto>>(json);
            if (dtoList == null) return;

            foreach (var dto in dtoList)
            {
                var existingCharacter = _characters.FirstOrDefault(c => c.CharacterID == dto.CharacterID);

                // Convert DTO expressions to CharacterExpression list for update
                List<CharacterExpression> newExpressions = new List<CharacterExpression>();
                if (dto.Expressions != null)
                {
                    foreach (var expDto in dto.Expressions)
                    {
                        var exp = new CharacterExpression();
                        exp.SetData(expDto.ExpressionName, expDto.AnimationStateName);
                        newExpressions.Add(exp);
                    }
                }

                if (existingCharacter != null)
                {
                    // Update existing character using type-safe method
                    existingCharacter.UpdateData(dto.Name, dto.Is3D, existingCharacter.Prefab, newExpressions);
                }
                else
                {
                    // Create new character
                    var newCharacter = new CharacterData();
                    newCharacter.SetID(dto.CharacterID);
                    newCharacter.UpdateData(dto.Name, dto.Is3D, null, newExpressions);
                    _characters.Add(newCharacter);
                }
            }
        }
    }
}