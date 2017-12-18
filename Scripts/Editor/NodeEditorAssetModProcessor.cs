using UnityEditor;
using UnityEngine;

namespace XNodeEditor {
    public class NodeEditorAssetModProcessor : UnityEditor.AssetModificationProcessor {
        public static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options) {
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

            if (!(obj is UnityEditor.MonoScript)) return AssetDeleteResult.DidNotDelete;

            UnityEditor.MonoScript script = obj as UnityEditor.MonoScript;
            System.Type scriptType = script.GetClass();

            if (scriptType != typeof(XNode.Node) && !scriptType.IsSubclassOf(typeof(XNode.Node))) return AssetDeleteResult.DidNotDelete;

            //Find ScriptableObjects using this script
            string[] guids = AssetDatabase.FindAssets("t:" + scriptType);
            for (int i = 0; i < guids.Length; i++) {
                string assetpath = AssetDatabase.GUIDToAssetPath(guids[i]);
                Object[] objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetpath);
                for (int k = 0; k < objs.Length; k++) {
                    XNode.Node node = objs[k] as XNode.Node;
                    if (node.GetType() == scriptType) {
                        if (node != null && node.graph != null) {
                            Debug.LogWarning(node.name + " of " + node.graph + " depended on deleted script and has been removed automatically.", node.graph);
                            node.graph.RemoveNode(node);
                        }
                    }
                }

            }
            return AssetDeleteResult.DidNotDelete;
        }
    }
}