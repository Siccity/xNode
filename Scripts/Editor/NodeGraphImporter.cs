using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using XNode;

namespace XNodeEditor {
    /// <summary> Deals with modified assets </summary>
    class NodeGraphImporter : AssetPostprocessor {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
            foreach (string path in importedAssets) {
                // Skip processing anything without the .asset extension
                if (Path.GetExtension(path) != ".asset") continue;

                // Get the object that is requested for deletion
                NodeGraph graph = AssetDatabase.LoadAssetAtPath<NodeGraph>(path);
                if (graph == null) continue;

                // Get attributes
                Type graphType = graph.GetType();
                NodeGraph.RequireNodeAttribute[] attribs = Array.ConvertAll(
                    graphType.GetCustomAttributes(typeof(NodeGraph.RequireNodeAttribute), true), x => x as NodeGraph.RequireNodeAttribute);


                Vector2 position = Vector2.zero;
                foreach (NodeGraph.RequireNodeAttribute attrib in attribs) {
                    if (attrib.type0 != null) {
                        if (!graph.nodes.Any(x => x.GetType() == attrib.type0)) {
                            XNode.Node node = graph.AddNode(attrib.type0);
                            node.position = position;
                            position.x += 200;
                            if (node.name == null || node.name.Trim() == "") node.name = NodeEditorUtilities.NodeDefaultName(attrib.type0);
                            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(graph))) AssetDatabase.AddObjectToAsset(node, graph);
                        }
                    }
                }
            }
        }
    }
}