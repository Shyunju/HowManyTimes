using UnityEditor;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace UGESystem
{
    /// <summary>
    /// Provides a Unity editor window that imports story data from a <c>.zip</c> file exported from the web,
    /// automatically creating and updating project assets.
    /// </summary>
    public class WebImportWizard : EditorWindow
    {
        private string _zipPath = "";
        private Vector2 _scrollPosition;
        private string _log = "Import log will be displayed here...\n";
        
        // --- Asset Path Constants ---
        private const string CHARACTER_DB_PATH = "Assets/Resources/UGESystem/CharacterData/CharacterDatabase.asset";
        private const string GAME_EVENT_FOLDER = "Assets/Resources/UGESystem/EventSO";
        private const string STORYBOARD_FOLDER = "Assets/Resources/UGESystem/Storyboards";
        
        // DTOs to read identifying info without full deserialization
        private class GameEventDtoTemp
        {
            public string Guid;
            public string Name;
        }

        private class StoryboardDtoTemp
        {
            public string Name;
        }

        /// <summary>
        /// Opens the Web Import Wizard window.
        /// </summary>
        [MenuItem("Tools/UGESystem/Web Import Wizard")]
        public static void Open()
        {
            WebImportWizard window = GetWindow<WebImportWizard>("Web Import Wizard");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        /// <summary>
        /// Draws the custom GUI for the Web Import Wizard,
        /// allowing users to select a <c>.zip</c> file, initiate the import process, and view the import log.
        /// </summary>
        public void OnGUI()
        {
            EditorGUILayout.LabelField("Web Project Import", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Select the 'UGESystem_Web.zip' file exported from the web tool to import your project.", MessageType.Info);
            
            EditorGUILayout.Space(10);

            // ZIP file selection
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Zip File Path", _zipPath, EditorStyles.textField);
            if (GUILayout.Button("Browse...", GUILayout.Width(80)))
            {
                string path = EditorUtility.OpenFilePanel("Select UGESystem_Web.zip", "", "zip");
                if (!string.IsNullOrEmpty(path))
                {
                    _zipPath = path;
                    _log = $"Selected file: {_zipPath}\n";
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Import button
            GUI.enabled = !string.IsNullOrEmpty(_zipPath) && File.Exists(_zipPath);
            if (GUILayout.Button("Start Import", GUILayout.Height(30)))
            {
                ImportProject();
            }
            GUI.enabled = true;
            
            EditorGUILayout.Space(10); 
            
            // Log area
            EditorGUILayout.LabelField("Import Log", EditorStyles.boldLabel);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, EditorStyles.helpBox, GUILayout.ExpandHeight(true));
            EditorGUILayout.SelectableLabel(_log, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void ImportProject()
        {
            _log = $"Import process started from: {_zipPath}\n\n";
            Repaint();

            var jsonData = new Dictionary<string, string>();
            var gameEventAssetMap = new Dictionary<string, GameEvent>(); // GUID -> Asset

            try
            {
                // 1. Unzip and read all JSON files
                using (ZipArchive archive = ZipFile.OpenRead(_zipPath))
                {
                    _log += $"Successfully opened zip archive. Found {archive.Entries.Count} entries.\n";
                    
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (!entry.FullName.EndsWith(".json")) continue;

                        using (StreamReader reader = new StreamReader(entry.Open(), Encoding.UTF8))
                        {
                            string content = reader.ReadToEnd();
                            jsonData.Add(entry.FullName, content);
                        }
                    }
                    _log += $"Read {jsonData.Count} JSON files.\n\n";
                }

                // Pre-scan all GameEvents in project to build a GUID map for faster lookups
                _log += "Scanning project for existing GameEvents...\n";
                string[] allGameEventArgs = AssetDatabase.FindAssets("t:GameEvent");
                foreach (var guid in allGameEventArgs)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var gameEvent = AssetDatabase.LoadAssetAtPath<GameEvent>(path);
                    if (gameEvent != null && !string.IsNullOrEmpty(gameEvent.Guid) && !gameEventAssetMap.ContainsKey(gameEvent.Guid))
                    {
                        gameEventAssetMap.Add(gameEvent.Guid, gameEvent);
                    }
                }
                _log += $"Found {gameEventAssetMap.Count} existing GameEvents with GUIDs.\n\n";

                // 2. Import Character Database
                ImportCharacterDatabase(jsonData);

                // 3. Import Game Events
                ImportGameEvents(jsonData, gameEventAssetMap);

                // 4. Import Storyboards
                ImportStoryboards(jsonData, gameEventAssetMap);

                _log += "\nImport process finished successfully!";
                EditorUtility.DisplayDialog("Import Complete", "Web project data has been successfully imported.", "OK");
            }
            catch (System.Exception e)
            {
                _log += $"\nERROR: An error occurred during import.\n{e.Message}\n\n{e.StackTrace}";
#if UNITY_EDITOR
                Debug.LogError($"[WebImportWizard] Error: {e.Message}");
#endif
                EditorUtility.DisplayDialog("Import Failed", "An error occurred during the import process. Please check the console and the wizard log for details.", "OK");
            }
            finally
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Repaint();
            }
        }

        private void ImportCharacterDatabase(Dictionary<string, string> jsonData)
        {
            // flexible key search to handle different path separators or folder structures
            string characterDbKey = jsonData.Keys.FirstOrDefault(k => k.EndsWith("character_db.json", System.StringComparison.OrdinalIgnoreCase));
            
            if (string.IsNullOrEmpty(characterDbKey))
            {
                _log += "[Warning] No 'character_db.json' found in zip. Skipping CharacterDatabase.\n";
                return;
            }

            string jsonContent = jsonData[characterDbKey];
            _log += "Found character database file. Processing...\n";
            
            // Try to find existing database anywhere in the project
            CharacterDatabase db = null;
            string[] guids = AssetDatabase.FindAssets("t:CharacterDatabase");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                db = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(path);
                _log += $"Found existing CharacterDatabase at: {path}\n";
                if (guids.Length > 1)
                {
                    _log += "[Warning] Multiple CharacterDatabase assets found. Using the first one found.\n";
                }
            }

            // Create new if not found
            if (db == null)
            {
                _log += $"No existing CharacterDatabase found. Creating new at '{CHARACTER_DB_PATH}'.\n";
                string folderPath = Path.GetDirectoryName(CHARACTER_DB_PATH);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                db = ScriptableObject.CreateInstance<CharacterDatabase>();
                AssetDatabase.CreateAsset(db, CHARACTER_DB_PATH);
            }
            
            db.FromJson(jsonContent);
            EditorUtility.SetDirty(db);
            _log += "Successfully imported and updated character data.\n\n";
        }

        private void ImportGameEvents(Dictionary<string, string> jsonData, Dictionary<string, GameEvent> gameEventAssetMap)
        {
            var gameEventJsonFiles = jsonData.Where(kvp => kvp.Key.StartsWith("GameEvents/")).ToList();
            if (gameEventJsonFiles.Count == 0)
            {
                _log += "[Info] No GameEvents found in zip file.\n";
                return;
            }

            _log += $"Found {gameEventJsonFiles.Count} GameEvent file(s) to process.\n";
            
            // Ensure the target directory exists for new assets
            if (!Directory.Exists(GAME_EVENT_FOLDER))
            {
                Directory.CreateDirectory(GAME_EVENT_FOLDER);
            }
            
            int updatedCount = 0;
            int createdCount = 0;

            foreach (var kvp in gameEventJsonFiles)
            {
                var tempDto = JsonConvert.DeserializeObject<GameEventDtoTemp>(kvp.Value);
                if (tempDto == null || string.IsNullOrEmpty(tempDto.Guid))
                {
                    _log += $"[Warning] Skipping file {kvp.Key} because it has no GUID.\n";
                    continue;
                }

                if (gameEventAssetMap.TryGetValue(tempDto.Guid, out GameEvent existingEvent))
                {
                    // Asset exists, update it
                    _log += $"Updating existing GameEvent: '{existingEvent.name}' (GUID: {tempDto.Guid})\n";
                    existingEvent.FromJson(kvp.Value);
                    EditorUtility.SetDirty(existingEvent);
                    updatedCount++;
                }
                else
                {
                    // Asset does not exist, create it
                    string assetName = string.IsNullOrEmpty(tempDto.Name) ? "NewGameEvent" : tempDto.Name;
                    string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(GAME_EVENT_FOLDER, $"{assetName}.asset"));
                    
                    _log += $"Creating new GameEvent at: {assetPath}\n";

                    GameEvent newEvent = ScriptableObject.CreateInstance<GameEvent>();
                    newEvent.FromJson(kvp.Value);
                    AssetDatabase.CreateAsset(newEvent, assetPath);
                    gameEventAssetMap.Add(newEvent.Guid, newEvent);
                    createdCount++;
                }
            }
            _log += $"Finished processing GameEvents. Updated: {updatedCount}, Created: {createdCount}.\n\n";
        }

        private void ImportStoryboards(Dictionary<string, string> jsonData, Dictionary<string, GameEvent> gameEventAssetMap)
        {
            var storyboardJsonFiles = jsonData.Where(kvp => kvp.Key.StartsWith("Storyboards/")).ToList();
            if (storyboardJsonFiles.Count == 0)
            {
                _log += "[Info] No Storyboards found in zip file.\n";
                return;
            }
            
            _log += $"Found {storyboardJsonFiles.Count} Storyboard file(s) to process.\n";

            // Ensure the target directory exists for new assets
            if (!Directory.Exists(STORYBOARD_FOLDER))
            {
                Directory.CreateDirectory(STORYBOARD_FOLDER);
            }
            
            int updatedCount = 0;
            int createdCount = 0;

            foreach (var kvp in storyboardJsonFiles)
            {
                var tempDto = JsonConvert.DeserializeObject<StoryboardDtoTemp>(kvp.Value);
                if (tempDto == null || string.IsNullOrEmpty(tempDto.Name))
                {
                    _log += $"[Warning] Skipping file {kvp.Key} because it has no Name for identification.\n";
                    continue;
                }
                
                // For storyboards, we'll try to find existing ones by name. This is less robust than GUID.
                string searchString = $"t:Storyboard {tempDto.Name}";
                string[] existingStoryboardGuids = AssetDatabase.FindAssets(searchString);
                Storyboard existingStoryboard = null;

                if (existingStoryboardGuids.Length > 0)
                {
                    foreach (var guid in existingStoryboardGuids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        // Check if the asset is in our target folder and its name matches exactly
                        if (Path.GetDirectoryName(path).Replace('\\', '/') == STORYBOARD_FOLDER &&
                            Path.GetFileNameWithoutExtension(path) == tempDto.Name)
                        {
                            existingStoryboard = AssetDatabase.LoadAssetAtPath<Storyboard>(path);
                            break;
                        }
                    }
                }

                if (existingStoryboard != null)
                {
                    _log += $"Updating existing Storyboard: '{existingStoryboard.name}'\n";
                    existingStoryboard.FromJson(kvp.Value, gameEventAssetMap);
                    EditorUtility.SetDirty(existingStoryboard);
                    updatedCount++;
                }
                else
                {
                    string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(STORYBOARD_FOLDER, $"{tempDto.Name}.asset"));
                    _log += $"Creating new Storyboard at: {assetPath}\n";

                    var newStoryboard = ScriptableObject.CreateInstance<Storyboard>();
                    newStoryboard.FromJson(kvp.Value, gameEventAssetMap);
                    AssetDatabase.CreateAsset(newStoryboard, assetPath);
                    createdCount++;
                }
            }
            _log += $"Finished processing Storyboards. Updated: {updatedCount}, Created: {createdCount}.\n\n";
        }
    }
}