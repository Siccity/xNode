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
        float result = 0f;
        NodePort port = target.GetInputByFieldName("value");
        if (port == null) return result;
        int connectionCount = port.ConnectionCount;
        for (int i = 0; i < connectionCount; i++) {

            NodePort connection = port.GetConnection(i);
            if (connection == null) continue;

            object obj = connection.GetValue();
            if (obj == null) continue;

            if (connection.type == typeof(int)) result += (int)obj;
            else if (connection.type == typeof(float)) result += (float)obj;
        }
        return result;
    }
}
