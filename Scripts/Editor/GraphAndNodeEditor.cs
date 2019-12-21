using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor {

    [CustomEditor(typeof(XNode.NodeGraph), true)]
    public class GlobalGraphEditor : Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();

            var graphObject = serializedObject.targetObject as XNode.NodeGraph;
            
            var button = GUILayout.Button("Edit graph", GUILayout.Height(40));
            if (button && graphObject != null) {
                NodeEditorWindow.Open(graphObject);
            }
            
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Raw data", "BoldLabel");

            DrawDefaultInspector();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
    

    [CustomEditor(typeof(XNode.Node), true)]
    public class GlobalNodeEditor : Editor {
        private SerializedProperty graph;
        
        private void OnEnable() {
            graph = serializedObject.FindProperty("graph");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            var graphObject = GetTargetObjectOfProperty(graph) as XNode.NodeGraph;
            
            var button = GUILayout.Button("Edit graph", GUILayout.Height(40));
            if (button && graphObject != null) {
                NodeEditorWindow.Open(graphObject);
            }

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Raw data", "BoldLabel");

            // Now draw the node itself.
            DrawDefaultInspector();
            
            serializedObject.ApplyModifiedProperties();
        }
        
        
        
        // HELPER METHODS BELOW RETRIEVED FROM https://github.com/lordofduct/spacepuppy-unity-framework/blob/master/SpacepuppyBaseEditor/EditorHelper.cs
        // BASED ON POST https://forum.unity.com/threads/get-a-general-object-value-from-serializedproperty.327098/#post-2309545
        // AND ADJUSTED SLIGHTLY AFTERWARDS
        
        /// <summary>
        /// Gets the object the property represents.
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        private static object GetTargetObjectOfProperty(SerializedProperty prop) {
            if (prop == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements) {
                if (element.Contains("[")) {
                    var elementName = element.Substring(0, element.IndexOf("[", StringComparison.Ordinal));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[", StringComparison.Ordinal))
                        .Replace("[", "")
                        .Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else {
                    obj = GetValue_Imp(obj, element);
                }
            }

            return obj;
        }
        
        private static object GetValue_Imp(object source, string name) {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null) {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }

            return null;
        }
        
        private static object GetValue_Imp(object source, string name, int index) {
            if (!(GetValue_Imp(source, name) is IEnumerable enumerable)) return null;
            var enm = enumerable.GetEnumerator();
            //while (index-- >= 0)
            //    enm.MoveNext();
            //return enm.Current;

            for (var i = 0; i <= index; i++) {
                if (!enm.MoveNext()) return null;
            }

            return enm.Current;
        }
    }

}
