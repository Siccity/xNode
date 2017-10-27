using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ExampleNodes {
    [CustomNodeEditor(typeof(ExampleNodes.DisplayValue), "ExampleNodes/Display Value")]
    public class DisplayValueEditor : NodeEditor {

        protected override void OnBodyGUI(out Dictionary<NodePort, Vector2> portPositions) {
            base.OnBodyGUI(out portPositions);
            EditorGUILayout.LabelField(GetResult().ToString());
        }

        public float GetResult() {
            DisplayValue t = target as DisplayValue;
            return t.GetValue();
        }
    }
}
