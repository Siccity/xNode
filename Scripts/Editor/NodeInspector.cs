using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using XNode;

[CustomEditor(typeof(Node), true)]
public class NodeInspector : Editor
{
	public override void OnInspectorGUI() { /*hides unneeded info*/ }
}
