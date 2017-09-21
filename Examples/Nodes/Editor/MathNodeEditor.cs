using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CustomNodeEditor(typeof(MathNode))]
public class AddNodeEditor : NodeEditor {

    public override void OnNodeGUI() {
        GUILayout.Label("YEAH CUSTOM");
    }
}
