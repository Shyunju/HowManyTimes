using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO; 
using Newtonsoft.Json; 
using System.Collections.Generic; 

namespace UGESystem
{
    /// <summary>
    /// Provides a basic custom inspector for <see cref="Storyboard"/> assets,
    /// supporting direct data manipulation and JSON import/export functionality as an alternative to the main graph editor.
    /// </summary>
    [CustomEditor(typeof(Storyboard))]
    public class StoryboardEditor : UnityEditor.Editor
    {
        private SerializedProperty _eventNodesProperty;

        /// <summary>
        /// Caches the SerializedProperty for the '_eventNodes' field when the inspector is enabled.
        /// /// (Korean) 인스펙터가 활성화될 때 '_eventNodes' 필드의 SerializedProperty를 캐시합니다.
        /// </summary>
        private void OnEnable()
        {
            _eventNodesProperty = serializedObject.FindProperty("_eventNodes");
        }

        /// <summary>
        /// Draws the custom inspector GUI for the <see cref="Storyboard"/> asset,
        /// displaying a list of <see cref="EventNodeData"/> elements,
        /// providing controls to add new nodes, and offering JSON import/export capabilities.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Storyboard Nodes", EditorStyles.boldLabel);

            // Draw the EventNodeData list.
            for (int i = _eventNodesProperty.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty nodeProperty = _eventNodesProperty.GetArrayElementAtIndex(i);
                DrawEventNode(nodeProperty, i);
            }

            if (GUILayout.Button("Add New Event Node"))
            {
                var menu = new GenericMenu();
                
                menu.AddItem(new GUIContent(nameof(EventNodeData)), false, () =>
                {
                    AddNewNode(typeof(EventNodeData));
                });

                var types = TypeCache.GetTypesDerivedFrom(typeof(EventNodeData));
                foreach (var type in types)
                {
                    if (type.IsAbstract) continue;
                    menu.AddItem(new GUIContent(type.Name), false, () =>
                    {
                        AddNewNode(type);
                    });
                }
                menu.ShowAsContext();
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

        /// <summary>
        /// Adds a new node of a specified type to the storyboard's event node list.
        /// /// (Korean) 지정된 타입의 새 노드를 스토리보드의 이벤트 노드 목록에 추가합니다.
        /// </summary>
        /// <param name="type">The type of EventNodeData to create. /// (Korean) 생성할 EventNodeData의 타입입니다.</param>
        private void AddNewNode(Type type)
        {
            int newIndex = _eventNodesProperty.arraySize;
            _eventNodesProperty.InsertArrayElementAtIndex(newIndex);
            var newElement = _eventNodesProperty.GetArrayElementAtIndex(newIndex);
            newElement.managedReferenceValue = Activator.CreateInstance(type);
            serializedObject.ApplyModifiedProperties();
            
            RefreshGraphView();
        }

        /// <summary>
        /// Draws the inspector GUI for a single event node property.
        /// /// (Korean) 단일 이벤트 노드 프로퍼티에 대한 인스펙터 GUI를 그립니다.
        /// </summary>
        /// <param name="nodeProperty">The SerializedProperty representing the EventNodeData. /// (Korean) EventNodeData를 나타내는 SerializedProperty입니다.</param>
        /// <param name="index">The index of the node in the list. /// (Korean) 리스트에 있는 노드의 인덱스입니다.</param>
        private void DrawEventNode(SerializedProperty nodeProperty, int index)
        {
            if (nodeProperty == null || nodeProperty.serializedObject.targetObject == null) return;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            SerializedProperty nameProperty = nodeProperty.FindPropertyRelative("_name");

            string displayName = (nameProperty != null && !string.IsNullOrEmpty(nameProperty.stringValue))
                ? nameProperty.stringValue
                : "New Node";
            
            nodeProperty.isExpanded = EditorGUILayout.Foldout(nodeProperty.isExpanded, $"Node: {displayName}", true);

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                var nodeIdProperty = nodeProperty.FindPropertyRelative("_nodeID");
                if (nodeIdProperty != null)
                {
                    DeleteNodeFromGraphView(nodeIdProperty.stringValue);
                    return;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (nodeProperty.isExpanded)
            {
                if (nodeProperty == null || nodeProperty.serializedObject.targetObject == null)
                {
                    EditorGUILayout.EndVertical();
                    return;
                }

                EditorGUI.indentLevel++;

                var nodeIdProperty = nodeProperty.FindPropertyRelative("_nodeID");
                if (nodeIdProperty != null)
                {
                    GUI.enabled = false;
                    EditorGUILayout.PropertyField(nodeIdProperty, new GUIContent("Node ID (GUID)"));
                    GUI.enabled = true;
                }

                var iterator = nodeProperty.Copy();
                var endProperty = iterator.GetEndProperty();
                iterator.NextVisible(true); 

                if (iterator.name == "m_Script" && iterator.type == "PPtr<MonoScript>")
                {
                    iterator.NextVisible(false);
                }
                
                while (iterator.NextVisible(false) && !SerializedProperty.EqualContents(iterator, endProperty))
                {
                    if (iterator.name == "_nodeID") continue;

                    if (iterator.name == "_startConditions")
                    {
                        EditorHelper.BuildPolymorphicListView(serializedObject, iterator, typeof(AbstractEventCondition));
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(iterator, true);
                    }
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Deletes a node from the graph view and the underlying storyboard data.
        /// It prioritizes finding an open StoryboardEditorWindow to perform the deletion visually.
        /// /// (Korean) 그래프 뷰와 기본 스토리보드 데이터에서 노드를 삭제합니다.
        /// /// 시각적 삭제를 수행하기 위해 열려있는 StoryboardEditorWindow를 우선적으로 찾습니다.
        /// </summary>
        /// <param name="nodeId">The unique ID of the node to delete. /// (Korean) 삭제할 노드의 고유 ID입니다.</param>
        private void DeleteNodeFromGraphView(string nodeId)
        {
            var windows = Resources.FindObjectsOfTypeAll<StoryboardEditorWindow>();
            foreach (var window in windows)
            {
                var storyboard = (Storyboard)target;
                if (window.CurrentStoryboard == storyboard && window.rootVisualElement.Contains(window.GetGraphView()))
                {
                    var graphView = window.GetGraphView();
                    if (graphView.CreatedNodes.TryGetValue(nodeId, out var graphNode))
                    {
                        graphView.DeleteElements(new[] { graphNode });
                        return;
                    }
                }
            }

#if UNITY_EDITOR
            Debug.LogWarning("Storyboard Editor Window not found or not focused on this storyboard. Deleting data directly. This may cause UI exceptions if the window is open but unfocused.");
#endif
            for (int i = 0; i < _eventNodesProperty.arraySize; i++)
            {
                var prop = _eventNodesProperty.GetArrayElementAtIndex(i);
                if (prop.FindPropertyRelative("_nodeID").stringValue == nodeId)
                {
                    _eventNodesProperty.DeleteArrayElementAtIndex(i);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Refreshes any open storyboard editor windows to reflect changes in the underlying data.
        /// /// (Korean) 열려있는 모든 스토리보드 에디터 창을 새로고침하여 기본 데이터의 변경사항을 반영합니다.
        /// </summary>
        private void RefreshGraphView()
        {
            var windows = Resources.FindObjectsOfTypeAll<StoryboardEditorWindow>();
            foreach (var window in windows)
            {
                var storyboard = (Storyboard)target;
                if (window.CurrentStoryboard == storyboard)
                {
                    window.SetStoryboard(storyboard); 
                }
            }
        }

        /// <summary>
        /// Exports the current storyboard data to a JSON file.
        /// /// (Korean) 현재 스토리보드 데이터를 JSON 파일로 내보냅니다.
        /// </summary>
        private void ExportToJson()
        {
            Storyboard storyboard = (Storyboard)target;
            string path = EditorUtility.SaveFilePanel("Export Storyboard to JSON", "", storyboard.name + ".json", "json");
            if (string.IsNullOrEmpty(path)) return;

            string json = storyboard.ToJson();
            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Imports storyboard data from a JSON file, overwriting the current data.
        /// /// (Korean) JSON 파일에서 스토리보드 데이터를 가져와 현재 데이터를 덮어씁니다.
        /// </summary>
        private void ImportFromJson()
        {
            Storyboard storyboard = (Storyboard)target;
            string path = EditorUtility.OpenFilePanel("Import Storyboard from JSON", "", "json");
            if (string.IsNullOrEmpty(path)) return;
            
            Undo.RecordObject(storyboard, "Import Storyboard from JSON");
            
            string json = File.ReadAllText(path);

            var dto = JsonConvert.DeserializeObject<Storyboard.StoryboardDto>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = new List<JsonConverter>
                {
                    new Vector2Converter(),
                    new Vector3Converter(),
                    new ColorConverter()
                }
            });

            if (dto != null)
            {
                storyboard.EventNodes.Clear();
                var nodeLookup = new Dictionary<string, EventNodeData>();

                if (dto.Nodes != null)
                {
                    foreach (var nodeDto in dto.Nodes)
                    {
                        var newEventNodeData = new EventNodeData(nodeDto);
                        if (!string.IsNullOrEmpty(nodeDto.GameEventGuid))
                        {
                            newEventNodeData.GameEventAsset = GameEventGuidManager.GetGameEvent(nodeDto.GameEventGuid);
                        }
                        storyboard.EventNodes.Add(newEventNodeData);
                        nodeLookup.Add(newEventNodeData.NodeID, newEventNodeData);
                    }
                }

                if (dto.Connections != null)
                {
                    foreach (var connectionDto in dto.Connections)
                    {
                        if (nodeLookup.TryGetValue(connectionDto.OutputNodeGuid, out var outputNode) &&
                            nodeLookup.TryGetValue(connectionDto.InputNodeGuid, out var inputNode))
                        {
                            outputNode.NextNodeIDs.Add(inputNode.NodeID);
                        }
                    }
                }
            }

            EditorUtility.SetDirty(storyboard);
            serializedObject.Update();

            RefreshGraphView();
        }
    }
}
