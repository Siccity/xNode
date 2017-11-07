using UnityEditor;
using XNodeEditor;

namespace BasicNodes {
    [CustomNodeEditor(typeof(DisplayValue))]
    public class DisplayValueEditor : NodeEditor {

        public override void OnBodyGUI() {
            base.OnBodyGUI();
            NodeEditorGUILayout.PortField(target.GetInputPort("input"));
            object obj = target.GetValue(null);
            if (obj != null) EditorGUILayout.LabelField(obj.ToString());
        }
    }
}