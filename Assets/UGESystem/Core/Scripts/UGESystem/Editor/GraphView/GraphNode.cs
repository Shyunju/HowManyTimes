using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UGESystem
{
    /// <summary>
    /// Defines the visual representation and behavior of a single node within the storyboard graph editor,
    /// including properties, ports, and state visualization.
    /// </summary>
    public class GraphNode : Node
    {
        /// <summary>
        /// The unique ID of this graph node.
        /// </summary>
        public string NodeID { get; private set; }
        /// <summary>
        /// The underlying <see cref="EventNodeData"/> associated with this graph node.
        /// </summary>
        public EventNodeData EventNodeData { get; private set; }
        /// <summary>
        /// The <see cref="SerializedProperty"/> representing the <see cref="EventNodeData"/> for this graph node.
        /// </summary>
        public SerializedProperty NodeProperty { get; private set; }

        /// <summary>
        /// The input port for this graph node, allowing incoming connections.
        /// </summary>
        public Port InputPort { get; private set; }
        /// <summary>
        /// The output port for this graph node, allowing outgoing connections.
        /// </summary>
        public Port OutputPort { get; private set; }

        private HelpBox _warningBox;
        private EventStatus _currentStatus = EventStatus.NotStarted;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphNode"/> class with the specified serialized property.
        /// </summary>
        /// <param name="nodeProperty">The <see cref="SerializedProperty"/> representing the <see cref="EventNodeData"/> for this node.</param>
        public GraphNode(SerializedProperty nodeProperty)
        {
            NodeProperty = nodeProperty;

            var eventNodeData = nodeProperty.managedReferenceValue as EventNodeData;
            if (eventNodeData == null) return;

            var positionProperty = nodeProperty.FindPropertyRelative("_position");
            Vector2 position = positionProperty != null ? positionProperty.vector2Value : Vector2.zero;

            title = eventNodeData.Name;
            SetPosition(new Rect(position, Vector2.zero));
            NodeID = eventNodeData.NodeID;
            EventNodeData = eventNodeData;

            style.width = 400;

            InputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            InputPort.portName = "In";
            inputContainer.Add(InputPort);

            OutputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            OutputPort.portName = "Out";
            outputContainer.Add(OutputPort);
            
            _warningBox = new HelpBox("", HelpBoxMessageType.Warning);
            _warningBox.style.display = DisplayStyle.None;
            mainContainer.Add(_warningBox);

            // Create a single IMGUI container to draw all properties reliably.
            var imguicontainer = new IMGUIContainer(() =>
            {
                NodeProperty.serializedObject.Update();

                var nameProperty = NodeProperty.FindPropertyRelative("_name");
                var nodeIdProperty = NodeProperty.FindPropertyRelative("_nodeID");
                var typeProperty = NodeProperty.FindPropertyRelative("_type");
                var isRepeatableProperty = NodeProperty.FindPropertyRelative("_isRepeatable");
                var gameEventAssetProperty = NodeProperty.FindPropertyRelative("GameEventAsset");
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(nodeIdProperty);
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(nameProperty);
                if (EditorGUI.EndChangeCheck())
                {
                    UpdateNodeStyle();
                }

                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(typeProperty);
                EditorGUILayout.PropertyField(isRepeatableProperty);
                EditorGUILayout.PropertyField(gameEventAssetProperty);
                if (EditorGUI.EndChangeCheck())
                {
                    ValidateAssetCompatibility();
                }
                
                NodeProperty.serializedObject.ApplyModifiedProperties();
            });
            mainContainer.Add(imguicontainer);
            
            mainContainer.Add(BuildPolymorphicListView("Start Conditions", nodeProperty.FindPropertyRelative("_startConditions"), typeof(AbstractEventCondition)));
            
            UpdateNodeStyle();
            ValidateAssetCompatibility();
        }

        private void ValidateAssetCompatibility()
        {
            if (NodeProperty == null || NodeProperty.serializedObject.targetObject == null) return;

            var gameEventAsset = NodeProperty.FindPropertyRelative("GameEventAsset").objectReferenceValue as GameEvent;
            var nodeType = (GameEventType)NodeProperty.FindPropertyRelative("_type").enumValueIndex;

            if (gameEventAsset == null)
            {
                _warningBox.style.display = DisplayStyle.None;
                return;
            }

            var assetArchetype = gameEventAsset.Archetype;
            bool isCompatible = false;

            if (assetArchetype == GameEventArchetype.Generic)
            {
                isCompatible = true;
            }
            else
            {
                isCompatible = assetArchetype.ToString() == nodeType.ToString();
            }

            if (!isCompatible)
            {
                _warningBox.text = $"Warning: GameEvent with archetype '{assetArchetype}' is assigned to a node of type '{nodeType}'.";
                _warningBox.style.display = DisplayStyle.Flex;
            }
            else
            {
                _warningBox.style.display = DisplayStyle.None;
            }
        }
        
        private VisualElement BuildPolymorphicListView(string label, SerializedProperty listProperty, Type baseType)
        {
            var container = new Foldout { text = label };
            
            var addButton = new Button(() =>
            {
                var menu = new GenericMenu();
                var types = TypeCache.GetTypesDerivedFrom(baseType);
                foreach (var type in types)
                {
                    if (type.IsAbstract) continue;
                    menu.AddItem(new GUIContent(type.Name), false, () =>
                    {
                        listProperty.InsertArrayElementAtIndex(listProperty.arraySize);
                        var newElement = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
                        newElement.managedReferenceValue = Activator.CreateInstance(type);
                        listProperty.serializedObject.ApplyModifiedProperties();
                    });
                }
                menu.ShowAsContext();
            }) { text = "Add" };
            container.Add(addButton);

            container.Add(new IMGUIContainer(() =>
            {
                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    var elementProperty = listProperty.GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(elementProperty, true);
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        listProperty.DeleteArrayElementAtIndex(i);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                listProperty.serializedObject.ApplyModifiedProperties();
            }));

            return container;
        }

        /// <summary>
        /// Sets the visual status of the graph node (e.g., NotStarted, InProgress, Completed) and updates its style accordingly.
        /// </summary>
        /// <param name="newStatus">The new <see cref="EventStatus"/> for the node.</param>
        public void SetStatus(EventStatus newStatus)
        {
            _currentStatus = newStatus;
            UpdateNodeStyle();
        }

        /// <summary>
        /// Updates the title and visual style of the node based on its current state and properties.
        /// </summary>
        public void UpdateTitle() // Kept for backward compatibility from IMGUIContainer
        {
            UpdateNodeStyle();
        }

        private void UpdateNodeStyle()
        {
            var nameProperty = NodeProperty.FindPropertyRelative("_name");
            if (nameProperty == null || NodeProperty.serializedObject.targetObject == null) return;

            // Set title
            title = EventNodeData.IsStartNode ? $"{nameProperty.stringValue} (Start)" : nameProperty.stringValue;

            // Set Color based on priority
            if (_currentStatus == EventStatus.InProgress)
            {
                // InProgress color overrides all other states
                titleContainer.style.backgroundColor = new StyleColor(new Color(0.2f, 0.6f, 0.2f)); // Bright Green
            }
            else if (EventNodeData.IsStartNode)
            {
                // If not InProgress, a Start Node is always blue
                titleContainer.style.backgroundColor = new StyleColor(new Color(0.15f, 0.2f, 0.4f)); // Blue
            }
            else if (_currentStatus == EventStatus.Completed)
            {
                // If not a Start Node and not InProgress, a completed node is red
                titleContainer.style.backgroundColor = new StyleColor(new Color(0.7f, 0.2f, 0.2f)); // Red
            }
            else // NotStarted
            {
                // Default color for non-start, non-completed nodes
                titleContainer.style.backgroundColor = new StyleColor(StyleKeyword.Null);
            }
        }

        /// <summary>
        /// Populates the contextual menu for the graph node, adding actions such as 'Set as Start Node'.
        /// </summary>
        /// <param name="evt">The event data for populating the contextual menu.</param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            evt.menu.AppendAction("Set as Start Node", (action) =>
            {
                var storyboard = NodeProperty.serializedObject.targetObject as Storyboard;
                if (storyboard != null)
                {
                    var graphView = GetFirstAncestorOfType<StoryboardGraphView>();
                    
                    foreach (var nodeData in storyboard.EventNodes)
                    {
                        nodeData.IsStartNode = false;
                    }
                    
                    EventNodeData.IsStartNode = true;
                    EditorUtility.SetDirty(storyboard);

                    if (graphView != null)
                    {
                        graphView.nodes.ForEach(node => (node as GraphNode)?.UpdateNodeStyle());
                    }
                }
            });
        }
    }
}
