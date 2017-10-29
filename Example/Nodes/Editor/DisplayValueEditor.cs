using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BasicNodes {
    [CustomNodeEditor(typeof(DisplayValue), "BasicNodes/DisplayValue")]
    public class DisplayValueEditor : NodeEditor {

        protected override void OnBodyGUI(out Dictionary<NodePort, Vector2> portPositions) {
            base.OnBodyGUI(out portPositions);
            object obj = target.GetValue(null);
            if (obj != null) EditorGUILayout.LabelField(target.GetValue(null).ToString());
        }
    }
}