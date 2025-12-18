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

                if (existingCharacter != null)
                {
                    // Update existing character using Reflection to respect private setters
                    typeof(CharacterData).GetProperty("Name").SetValue(existingCharacter, dto.Name);
                    typeof(CharacterData).GetProperty("Is3D").SetValue(existingCharacter, dto.Is3D);

                    existingCharacter.Expressions.Clear();
                    if(dto.Expressions != null)
                    {
                        foreach (var expDto in dto.Expressions)
                        {
                            var newExp = new CharacterExpression();
                            typeof(CharacterExpression).GetProperty("ExpressionName").SetValue(newExp, expDto.ExpressionName);
                            typeof(CharacterExpression).GetProperty("AnimationStateName").SetValue(newExp, expDto.AnimationStateName);
                            existingCharacter.Expressions.Add(newExp);
                        }
                    }
                }
                else
                {
                    // Create new character
                    var newCharacter = new CharacterData();
                    typeof(CharacterData).GetProperty("CharacterID").SetValue(newCharacter, dto.CharacterID);
                    typeof(CharacterData).GetProperty("Name").SetValue(newCharacter, dto.Name);
                    typeof(CharacterData).GetProperty("Is3D").SetValue(newCharacter, dto.Is3D);
                    
                    newCharacter.Expressions.Clear(); // Constructor adds one, so clear it first.
                    if(dto.Expressions != null)
                    {
                        foreach (var expDto in dto.Expressions)
                        {
                            var newExp = new CharacterExpression();
                            typeof(CharacterExpression).GetProperty("ExpressionName").SetValue(newExp, expDto.ExpressionName);
                            typeof(CharacterExpression).GetProperty("AnimationStateName").SetValue(newExp, expDto.AnimationStateName);
                            newCharacter.Expressions.Add(newExp);
                        }
                    }
                    _characters.Add(newCharacter);
                }
            }
        }
    }
}