using UnityEditor;
using XNode.Examples.MathNodes;

namespace XNodeEditor.Examples {

    /// <summary> 
    /// NodeEditor functions similarly to the Editor class, only it is xNode specific.
    /// Custom node editors should have the CustomNodeEditor attribute that defines which node type it is an editor for.
    /// </summary>
    [CustomNodeEditor(typeof(DisplayValue))]
    public class DisplayValueEditor : NodeEditor {

        /// <summary> Called whenever the xNode editor window is updated </summary>
        public override void OnBodyGUI() {
            
            // Draw the default GUI first, so we don't have to do all of that manually.
            base.OnBodyGUI();

            // `target` points to the node, but it is of type `Node`, so cast it.
            DisplayValue displayValueNode = target as DisplayValue;

            // Get the value from the node, and display it
            object obj = displayValueNode.GetValue();
            if (obj != null) EditorGUILayout.LabelField(obj.ToString());
        }
    }
}