using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BasicNodes {
    [CustomNodeEditor(typeof(DisplayValue))]
    public class DisplayValueEditor : NodeEditor {

        public override void OnBodyGUI() {
            base.OnBodyGUI();
            object obj = target.GetValue(null);
            if (obj != null) EditorGUILayout.LabelField(target.GetValue(null).ToString());
        }
    }
}