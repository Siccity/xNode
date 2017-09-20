using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary> Base class to derive custom Node editors from. Use this to create your own custom inspectors and editors for your nodes. </summary>
public class NodeEditor : Editor {
    
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
    }
}
