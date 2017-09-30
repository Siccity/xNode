using System.Collections.Generic;
using UnityEngine;

[CustomNodeEditor(typeof(MathNode), "Math")]
public class AddNodeEditor : NodeEditor {

    public override void OnNodeGUI(out Dictionary<NodePort, Vector2> portPositions) {
        base.OnNodeGUI(out portPositions);
    }
}
