using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace UGESystem
{
    /// <summary>
    /// Creates a custom inspector for <see cref="GameEvent"/> assets,
    /// managing lists of various command types and providing JSON import/export functionality.
    /// </summary>
    [CustomEditor(typeof(GameEvent))]
    public class GameEventEditor : UnityEditor.Editor
    {
        private GameEvent _gameEvent;
        private SerializedProperty _guidProperty;
        private SerializedProperty _archetypeProperty;
        private SerializedProperty _commandsProperty;
        
        private int _commandToDelete = -1;
        private (int oldIndex, int newIndex) _commandToMove = (-1, -1);

        // --- Character Name Cache ---
        private static Dictionary<string, string> _characterNameCache;
        private static float _lastCacheUpdateTime;
        private const float CACHE_REFRESH_INTERVAL = 2.0f;

        private void OnEnable()
        {
            _gameEvent = (GameEvent)target;
            _guidProperty = serializedObject.FindProperty("<Guid>k__BackingField");
            _archetypeProperty = serializedObject.FindProperty("_archetype");
            _commandsProperty = serializedObject.FindProperty("_commands");

            RefreshCharacterNameCache();
        }

        private void RefreshCharacterNameCache()
        {
            _characterNameCache = new Dictionary<string, string>();
            string[] guids = AssetDatabase.FindAssets("t:CharacterDatabase");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var database = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(path);
                if (database != null)
                {
                    foreach (var character in database.Characters)
                    {
                        _characterNameCache[character.CharacterID] = character.Name;
                    }
                }
            }
            _lastCacheUpdateTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Draws the custom inspector GUI for the <see cref="GameEvent"/> asset,
        /// including fields for GUID and Archetype, a list of commands with reordering and deletion,
        /// and JSON import/export functionality.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw the GUID field as read-only
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_guidProperty, new GUIContent("GUID"));
            GUI.enabled = true;

            if (Time.realtimeSinceStartup > _lastCacheUpdateTime + CACHE_REFRESH_INTERVAL)
            {
                RefreshCharacterNameCache();
            }

            EditorGUILayout.PropertyField(_archetypeProperty);
            EditorGUILayout.Space();

            _commandToDelete = -1;
            _commandToMove = (-1, -1);

            for (int i = 0; i < _commandsProperty.arraySize; i++)
            {
                SerializedProperty commandProperty = _commandsProperty.GetArrayElementAtIndex(i);
                DrawCommand(commandProperty, i);
            }

            if (_commandToDelete > -1)
            {
                _commandsProperty.DeleteArrayElementAtIndex(_commandToDelete);
            }
            if (_commandToMove.oldIndex > -1)
            {
                _commandsProperty.MoveArrayElement(_commandToMove.oldIndex, _commandToMove.newIndex);
            }

            EditorGUILayout.Space(10);
            
            if (GUILayout.Button("Add Command"))
            {
                ShowAddCommandMenu();
            }
            
            EditorGUILayout.Space(20);

            EditorGUILayout.LabelField("JSON Management", EditorStyles.boldLabel);
            if (GUILayout.Button("Export to JSON"))
            {
                ExportToJson();
            }
            if (GUILayout.Button("Import from JSON"))
            {
                ImportFromJson();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowAddCommandMenu()
        {
            GenericMenu menu = new GenericMenu();
            GameEventArchetype currentArchetype = (GameEventArchetype)_archetypeProperty.enumValueIndex;

            var commandTypes = TypeCache.GetTypesDerivedFrom<EventCommand>();

            foreach (var type in commandTypes)
            {
                if (type.IsAbstract) continue;

                var availableInAttr = type.GetCustomAttribute<AvailableIn>();
                bool isAvailable = false;

                if (availableInAttr == null)
                {
                    isAvailable = true;
                }
                else if (currentArchetype == GameEventArchetype.Generic)
                {
                    isAvailable = true;
                }
                else if (availableInAttr.SupportedTypes.Any(supportedType => supportedType.ToString() == currentArchetype.ToString()))
                {
                     isAvailable = true;
                }

                if (isAvailable)
                {
                    menu.AddItem(new GUIContent(type.Name), false, () => { AddCommand(type); });
                }
            }

            menu.ShowAsContext();
        }

        private void AddCommand(Type commandType)
        {
            var newCommand = Activator.CreateInstance(commandType) as EventCommand;
            if(newCommand == null) return;

            _commandsProperty.InsertArrayElementAtIndex(_commandsProperty.arraySize);
            var newElement = _commandsProperty.GetArrayElementAtIndex(_commandsProperty.arraySize-1);
            newElement.managedReferenceValue = newCommand;

            string[] guids = AssetDatabase.FindAssets("t:CharacterDatabase");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                CharacterDatabase database = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(path);
                if (database != null && database.Characters.Count > 0)
                {
                    string defaultId = database.Characters[0].CharacterID;

                    if (newCommand is CharacterCommand characterCommand)
                    {
                        characterCommand.EDITOR_SetDefaultCharacter(defaultId);
                    }
                    else if (newCommand is DialogueCommand dialogueCommand)
                    {
                        dialogueCommand.EDITOR_SetDefaultCharacter(defaultId);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private bool NeedsAssetWarning(SerializedProperty commandProperty)
        {
            if (commandProperty.managedReferenceValue is PlaySoundCommand playSoundCommand)
            {
                // For PlaySound, only warn if the action is NOT Stop and the clip is missing.
                return playSoundCommand.Action != SoundAction.Stop && playSoundCommand.AudioClip == null;
            }
            
            if (commandProperty.managedReferenceValue is BackgroundCommand bgCommand)
            {
                // For Background, only warn if the action is Show and the corresponding asset is missing.
                if (bgCommand.Action == BackgroundAction.Show)
                {
                    return (bgCommand.Type == BackgroundType.Image && bgCommand.Image == null) ||
                           (bgCommand.Type == BackgroundType.Video && bgCommand.Video == null);
                }
            }

            if (commandProperty.managedReferenceValue is UGECameraCommand cameraCommand)
            {
                // Warn if the camera name is not set.
                return string.IsNullOrEmpty(cameraCommand.TargetCameraName);
            }

            // Add other command checks here if needed in the future.

            return false;
        }

        private void DrawCommand(SerializedProperty commandProperty, int index)
        {
            bool hasWarning = NeedsAssetWarning(commandProperty);
            Color originalGuiColor = GUI.backgroundColor;

            if (hasWarning)
            {
                GUI.backgroundColor = new Color(1f, 0.8f, 0.8f); // Light red tint for warning
            }

            EditorGUILayout.BeginVertical("box");
            GUI.backgroundColor = originalGuiColor; // Reset color for the content inside the box

            EditorGUILayout.BeginHorizontal();

            var isExpandedProp = commandProperty.FindPropertyRelative("_isNodeExpanded");
            
            if (hasWarning)
            {
                isExpandedProp.boolValue = true; // Force expand if there is a warning
            }

            isExpandedProp.boolValue = EditorGUILayout.Foldout(isExpandedProp.boolValue, GetCommandSummary(commandProperty), true);

            if (GUILayout.Button("▲", GUILayout.Width(25))) _commandToMove = (index, index - 1);
            if (GUILayout.Button("▼", GUILayout.Width(25))) _commandToMove = (index, index + 1);
            if (GUILayout.Button("X", GUILayout.Width(25))) _commandToDelete = index;

            EditorGUILayout.EndHorizontal();

            if (isExpandedProp.boolValue)
            {
                // The existing HelpBox logic will now be visible because of the forced expansion.
                if (commandProperty.managedReferenceValue is PlaySoundCommand playSoundCommand)
                {
                    if (playSoundCommand.Action != SoundAction.Stop && playSoundCommand.AudioClip == null)
                    {
                        EditorGUILayout.HelpBox("AudioClip must be assigned for this action.", MessageType.Warning);
                    }
                }
                else if (commandProperty.managedReferenceValue is BackgroundCommand bgCommand)
                {
                    if (bgCommand.Action == BackgroundAction.Show)
                    {
                        if (bgCommand.Type == BackgroundType.Image && bgCommand.Image == null)
                        {
                            EditorGUILayout.HelpBox("Background Image must be assigned for 'Show' action.", MessageType.Warning);
                        }
                        else if (bgCommand.Type == BackgroundType.Video && bgCommand.Video == null)
                        {
                            EditorGUILayout.HelpBox("Background Video must be assigned for 'Show' action.", MessageType.Warning);
                        }
                    }
                }
                else if (commandProperty.managedReferenceValue is UGECameraCommand cameraCommand)
                {
                    if (string.IsNullOrEmpty(cameraCommand.TargetCameraName))
                    {
                        EditorGUILayout.HelpBox("Target Camera Name must be assigned.", MessageType.Warning);
                    }
                }

                EditorGUI.indentLevel++;

                var iterator = commandProperty.Copy();
                var endProperty = iterator.GetEndProperty();
                iterator.NextVisible(true); 

                if (iterator.name == "m_Script" && iterator.type == "PPtr<MonoScript>")
                {
                    iterator.NextVisible(false);
                }
                
                // Draw all properties except for the node expansion state
                while (iterator.NextVisible(false) && !SerializedProperty.EqualContents(iterator, endProperty))
                {
                    if (iterator.name != "_isNodeExpanded")
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }
        
        private string GetCharacterName(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return "None";
            if (_characterNameCache != null && _characterNameCache.TryGetValue(guid, out var name) && !string.IsNullOrEmpty(name))
            {
                return name;
            }

            // If the guid is not in the cache, it's an invalid/old ID.
            // Display it safely by checking its length before trying to create a substring.
            if (guid.Length > 8)
            {
                return $"(Invalid ID: {guid.Substring(0, 8)}...)";
            }
            return $"(Invalid ID: {guid})";
        }

        private string GetCommandSummary(SerializedProperty commandProperty)
        {
            if (commandProperty.managedReferenceValue is EventCommand command)
            {
                switch (command.CommandType)
                {
                    case CommandType.Dialogue:
                        var dialogue = command as DialogueCommand;
                        return $"Dialogue: {GetCharacterName(dialogue.CharacterName)} - \"{Truncate(dialogue.DialogueText, 20)}\"" ;
                    case CommandType.Character:
                        var character = command as CharacterCommand;
                        return $"Character: {character.Action} {GetCharacterName(character.CharacterId)}";
                    case CommandType.Choice:
                        return "Choice: " + (command as ChoiceCommand).Choices.Count + " options";
                    case CommandType.Background:
                        var bgCommand = command as BackgroundCommand;
                        if (bgCommand.Action == BackgroundAction.Show)
                        {
                            return $"Background: Show ({bgCommand.Type})";
                        }
                        else 
                        {
                            return "Background: Hide";
                        }
                    case CommandType.Label:
                        var labelCommand = command as LabelCommand;
                        return $"Label: {labelCommand.LabelName}";
                    case CommandType.Camera:
                        var camera = command as UGECameraCommand;
                        string cameraName = string.IsNullOrEmpty(camera.TargetCameraName) ? "None" : camera.TargetCameraName;
                        return $"Camera: {camera.ActionType} on '{cameraName}'";
                    case CommandType.End:
                        return $"---" + " END EVENT " + "---";
                    default:
                        return command.CommandType.ToString();
                }
            }
            return "Unknown Command";
        }

        private string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }

        private void ExportToJson()
        {
            string path = EditorUtility.SaveFilePanel("Export Game Event to JSON", "", _gameEvent.name + ".json", "json");
            if (string.IsNullOrEmpty(path)) return;

            string json = _gameEvent.ToJson();
            File.WriteAllText(path, json);
        }

        private void ImportFromJson()
        {
            string path = EditorUtility.OpenFilePanel("Import Game Event from JSON", "", "json");
            if (string.IsNullOrEmpty(path)) return;
            
            Undo.RecordObject(_gameEvent, "Import Game Event from JSON");
            
            string json = File.ReadAllText(path);
            _gameEvent.FromJson(json);

            serializedObject.Update();
        }
    }
}