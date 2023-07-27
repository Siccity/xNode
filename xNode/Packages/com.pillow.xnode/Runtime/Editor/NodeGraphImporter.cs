using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using XNode;

namespace XNodeEditor {
    /// <summary> Deals with modified assets </summary>
    internal class NodeGraphImporter : AssetPostprocessor {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            foreach (var path in importedAssets) {
                // Skip processing anything without the .asset extension
                if (Path.GetExtension(path) != ".asset")
                {
                    continue;
                }

                // Get the object that is requested for deletion
                var graph = AssetDatabase.LoadAssetAtPath<NodeGraph>(path);
                if (graph == null)
                {
                    continue;
                }

                // Get attributes
                var graphType = graph.GetType();
                var attribs = Array.ConvertAll(
                    graphType.GetCustomAttributes(typeof(NodeGraph.RequireNodeAttribute), true), x => x as NodeGraph.RequireNodeAttribute);

                var position = Vector2.zero;
                foreach (var attrib in attribs) {
                    if (attrib.type0 != null)
                    {
                        AddRequired(graph, attrib.type0, ref position);
                    }

                    if (attrib.type1 != null)
                    {
                        AddRequired(graph, attrib.type1, ref position);
                    }

                    if (attrib.type2 != null)
                    {
                        AddRequired(graph, attrib.type2, ref position);
                    }
                }
            }
        }

        private static void AddRequired(NodeGraph graph, Type type, ref Vector2 position) {
            if (!graph.nodes.Any(x => x.GetType() == type)) {
                var node = graph.AddNode(type);
                node.position = position;
                position.x += 200;
                if (node.name == null || node.name.Trim() == "")
                {
                    node.name = NodeEditorUtilities.NodeDefaultName(type);
                }

                if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(graph)))
                {
                    AssetDatabase.AddObjectToAsset(node, graph);
                }
            }
        }
    }
}
