using UnityEditor;
using XNodeEditor;

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