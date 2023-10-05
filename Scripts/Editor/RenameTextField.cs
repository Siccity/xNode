using UnityEditor;
using UnityEngine;
using XNode;

namespace XNodeEditor
{
    /// <summary> Utility for renaming assets </summary>
    public class RenameTextField
    {
        private const string inputControlName = "nameInput";

        public static RenameTextField current { get; private set; }
        public static bool IsActive => current != null;
        public Object target;
        public string input;

        private bool firstFrame = true;

        /// <summary> Show a rename text field for an asset in the node header. Will trigger reimport of the asset on apply.
        public static RenameTextField Show(Object target, float width = 200)
        {
            RenameTextField textField = new RenameTextField();
            if (current != null)
            {
                current = null;
            }

            current = textField;
            textField.target = target;
            textField.input = target.name;

            return textField;
        }

        public void DrawRenameTextField()
        {
            GUI.SetNextControlName(inputControlName);
            input = GUILayout.TextField(input, NodeEditorResources.styles.nodeHeaderRename);
            EditorGUI.FocusTextInControl(inputControlName);

            if (firstFrame)
            {
                TextEditor textEditor =
                    (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                textEditor.SelectAll();
                firstFrame = false;
            }

            Event e = Event.current;
            // If input is empty, revert name to default instead
            if (input == null || input.Trim() == "")
            {
                if (e.isKey && e.keyCode == KeyCode.Return)
                {
                    SaveAndClose();
                }
            }
            // Rename asset to input text
            else
            {
                if (e.isKey && e.keyCode == KeyCode.Return)
                {
                    SaveAndClose();
                }
            }

            if (e.isKey && e.keyCode == KeyCode.Escape)
            {
                Close();
            }
        }

        public void SaveAndClose()
        {
            target.name = input;
            NodeEditor.GetEditor((Node)target, NodeEditorWindow.current).OnRename();
            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(target)))
            {
                AssetDatabase.SetMainObject((target as Node).graph, AssetDatabase.GetAssetPath(target));
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
            }

            Close();
            target.TriggerOnValidate();
        }

        public void Close()
        {
            firstFrame = true;
            current = null;
            NodeEditorWindow.current.Repaint();
            if (NodeEditorWindow.currentActivity == NodeEditorWindow.NodeActivity.Renaming)
            {
                NodeEditorWindow.currentActivity = NodeEditorWindow.NodeActivity.Idle;
            }
        }
    }
}