using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomNodeEditor(typeof(DisplayValue), "Display Value")]
public class DisplayValueEditor : NodeEditor {

    public override void OnNodeGUI(out Dictionary<NodePort, Vector2> portPositions) {
        base.OnNodeGUI(out portPositions);
        EditorGUILayout.LabelField("Value: " + GetResult());
    }

    public float GetResult() {
        ExampleNodeBase t = target as ExampleNodeBase;
        return t.GetInputFloat("value");
    }
}
