using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace UGESystem
{
    /// <summary>
    /// The core class for the storyboard editor window, using the <see cref="GraphView"/> API
    /// to display, connect, and manage a visual graph of <see cref="EventNodeData"/>.
    /// </summary>
    public class StoryboardGraphView : GraphView
    {
        private Storyboard _currentStoryboard;
        private SerializedObject _storyboardObject;

        /// <summary>
        /// Gets the currently loaded <see cref="Storyboard"/> asset in the graph view.
        /// </summary>
        public Storyboard CurrentStoryboard => _currentStoryboard;
        /// <summary>
        /// A dictionary mapping node IDs to their corresponding <see cref="GraphNode"/> instances within this graph view.
        /// </summary>
        public Dictionary<string, GraphNode> CreatedNodes { get; private set; } = new Dictionary<string, GraphNode>();

        /// <summary>
        /// Initializes a new instance of the <see cref="StoryboardGraphView"/> class,
        /// setting up the basic graph view functionalities like zoom, panning, selection, and grid background.
        /// </summary>
        public StoryboardGraphView()
        {
            style.flexGrow = 1;
            
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            this.AddManipulator(CreateContextMenu());

            graphViewChanged += OnGraphViewChanged;
        }

        /// <summary>
        /// Determines which ports are compatible for connecting an edge from a given start port.
        /// </summary>
        /// <param name="startPort">The port from which a connection is initiated.</param>
        /// <param name="nodeAdapter">An adapter for nodes.</param>
        /// <returns>A list of compatible ports.</returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });
            return compatiblePorts;
        }

        private void MarkDirty()
        {
            if (_currentStoryboard != null)
            {
                EditorUtility.SetDirty(_currentStoryboard);
            }
            _storyboardObject.ApplyModifiedProperties();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.movedElements != null)
            {
                foreach (var element in graphViewChange.movedElements)
                {
                    if (element is GraphNode node)
                    {
                        var positionProperty = node.NodeProperty.FindPropertyRelative("_position");
                        positionProperty.vector2Value = node.GetPosition().position;
                    }
                }
                MarkDirty();
            }
            
            if (graphViewChange.elementsToRemove != null)
            {
                foreach (GraphElement element in graphViewChange.elementsToRemove)
                {
                    if (element is GraphNode node)
                    {
                        _currentStoryboard.EventNodes.Remove(node.EventNodeData);
                        MarkDirty();
                    }
                    else if (element is Edge edge)
                    {
                        var sourceNode = edge.output.node as GraphNode;
                        var targetNode = edge.input.node as GraphNode;
                        if (sourceNode != null && targetNode != null)
                        {
                            sourceNode.EventNodeData.NextNodeIDs.Remove(targetNode.NodeID);

                            var conditionToRemove = targetNode.EventNodeData.StartConditions
                                .OfType<PreviousEventCompletedCondition>()
                                .FirstOrDefault(c => c.TargetNodeID == sourceNode.NodeID);
                            if (conditionToRemove != null)
                            {
                                targetNode.EventNodeData.StartConditions.Remove(conditionToRemove);
                            }
                            
                            MarkDirty();
                        }
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null)
            {
                foreach (Edge edge in graphViewChange.edgesToCreate)
                {
                    var sourceNode = edge.output.node as GraphNode;
                    var targetNode = edge.input.node as GraphNode;
                    if (sourceNode != null && targetNode != null)
                    {
                        sourceNode.EventNodeData.NextNodeIDs.Add(targetNode.NodeID);

                        var newCondition = new PreviousEventCompletedCondition(
                            $"Wait for '{sourceNode.EventNodeData.Name}' to complete.",
                            sourceNode.NodeID
                        );
                        targetNode.EventNodeData.StartConditions.Add(newCondition);
                        
                        MarkDirty();
                    }
                }
            }

            return graphViewChange;
        }

        /// <summary>
        /// Clears all nodes and edges from the graph view.
        /// </summary>
        public void ClearGraph()
        {
            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;
        }

        /// <summary>
        /// Populates the graph view with nodes and edges based on the provided <see cref="Storyboard"/> data.
        /// </summary>
        /// <param name="storyboard">The <see cref="Storyboard"/> asset to display in the graph view.</param>
        public void PopulateGraph(Storyboard storyboard)
        {
            _currentStoryboard = storyboard;
            if (_currentStoryboard == null) 
            {
                ClearGraph();
                return;
            }

            _storyboardObject = new SerializedObject(_currentStoryboard);
            
            ClearGraph();

            var eventNodesProperty = _storyboardObject.FindProperty("_eventNodes");

            if (eventNodesProperty == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"Could not find '_eventNodes' property in {storyboard.name}. Check the field name in Storyboard.cs.");
#endif
                return;
            }

            CreatedNodes.Clear(); // Clear the public property
            
            _storyboardObject.Update();

            for (int i = 0; i < eventNodesProperty.arraySize; i++){
                var nodeProperty = eventNodesProperty.GetArrayElementAtIndex(i);
                var eventNodeData = nodeProperty.managedReferenceValue as EventNodeData;

                if (eventNodeData == null) continue;
                if (string.IsNullOrEmpty(eventNodeData.NodeID)) continue;
                
                var graphNode = CreateGraphNode(nodeProperty);
                CreatedNodes.Add(eventNodeData.NodeID, graphNode); // Add to public property
            }

            foreach (var nodeData in _currentStoryboard.EventNodes)
            {
                if (nodeData == null || !CreatedNodes.ContainsKey(nodeData.NodeID)) continue;

                GraphNode sourceNode = CreatedNodes[nodeData.NodeID];

                // Draw normal connection lines
                foreach (string nextNodeID in nodeData.NextNodeIDs)
                {
                    if (string.IsNullOrEmpty(nextNodeID) || !CreatedNodes.ContainsKey(nextNodeID)) continue;
                    
                    GraphNode targetNode = CreatedNodes[nextNodeID];
                    
                    Edge edge = sourceNode.OutputPort.ConnectTo(targetNode.InputPort);
                    AddElement(edge);
                }
                
                // Draw branch connection lines (read-only)
                if (nodeData.GameEventAsset != null)
                {
                    foreach (var command in nodeData.GameEventAsset.Commands)
                    {
                        if (command is EndCommand endCommand && endCommand.IsBranching && !string.IsNullOrEmpty(endCommand.TargetNodeID))
                        {
                            var targetEventNode = _currentStoryboard.EventNodes.FirstOrDefault(n => n.NodeID == endCommand.TargetNodeID);
                            if (targetEventNode != null && CreatedNodes.ContainsKey(targetEventNode.NodeID))
                            {
                                GraphNode targetNode = CreatedNodes[targetEventNode.NodeID];
                                
                                if (sourceNode.OutputPort == null)
                                {
#if UNITY_EDITOR
                                    Debug.LogError($"Source node '{sourceNode.NodeID}' has null OutputPort.");
#endif
                                    continue;
                                }
                                if (targetNode.InputPort == null)
                                {
#if UNITY_EDITOR
                                    Debug.LogError($"Target node '{targetNode.NodeID}' has null InputPort.");
#endif
                                    continue;
                                }

                                Edge branchEdge = sourceNode.OutputPort.ConnectTo(targetNode.InputPort);
                                
                                // Since the edge color change function is no longer used, remove the userData assignment
                                branchEdge.capabilities &= ~(Capabilities.Selectable | Capabilities.Deletable);
                                
                                AddElement(branchEdge);
                            }
                        }
                    }
                }
            }
            
            // After drawing all nodes and edges, forcefully overwrite the style at the end.
            ApplyStyles();
            
            FrameAll();
        }

        private void ApplyStyles()
        {
            nodes.ForEach(node =>
            {
                (node as GraphNode)?.UpdateTitle();
            });
        }

        private GraphNode CreateGraphNode(SerializedProperty nodeProperty)
        {
            var node = new GraphNode(nodeProperty);
            AddElement(node);
            return node;
        }

        private IManipulator CreateContextMenu()
        {
            var contextMenu = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Create Node", actionEvent => 
                    CreateNewNode("New Event", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
            );
            return contextMenu;
        }

        private void CreateNewNode(string nodeName, Vector2 position)
        {
            if (_currentStoryboard == null)
            {
                EditorUtility.DisplayDialog("No Storyboard Selected", "Please select a Storyboard asset in the Project window first.", "OK");
                return;
            }

            string nodeId = System.Guid.NewGuid().ToString();
            var newNodeData = new EventNodeData(nodeId, nodeName, GameEventType.Dialogue, position);
            _currentStoryboard.EventNodes.Add(newNodeData);

            EditorUtility.SetDirty(_currentStoryboard);
            AssetDatabase.SaveAssets();

            _storyboardObject.Update();

            var eventNodesProperty = _storyboardObject.FindProperty("_eventNodes");
            var newNodeProperty = eventNodesProperty.GetArrayElementAtIndex(eventNodesProperty.arraySize - 1);

            CreateGraphNode(newNodeProperty);
        }

        private Vector2 GetLocalMousePosition(Vector2 mousePosition)
        {
            return contentViewContainer.WorldToLocal(mousePosition);
        }
    }
}
