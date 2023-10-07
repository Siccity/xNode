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
        public static RenameTextField Show(Object target)
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
            GUIStyle stylesNodeHeaderRename = NodeEditorResources.styles.nodeHeaderRename;
            stylesNodeHeaderRename.fontSize =
                NodeEditor.GetEditor((Node)target, NodeEditorWindow.current).GetHeaderStyle().fontSize;
            input = GUILayout.TextField(input, stylesNodeHeaderRename);
            EditorGUI.FocusTextInControl(inputControlName);

            if (firstFrame)
            {
                // Fixes textfield not being fully selected on multiple consecutive rename activates.
                TextEditor textEditor =
                    (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
                textEditor.SelectAll();

                NodeEditor.GetEditor((Node)target, NodeEditorWindow.current).OnRenameActive();

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
            // Enabled undoing of renaming.
            Undo.RecordObject(target, $"Renamed Node: [{target.name}] -> [{input}]");

            target.name = input;
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

            EditorGUIUtility.editingTextField = false;
            NodeEditor.GetEditor((Node)target, NodeEditorWindow.current).OnRenameDeactive();
            NodeEditorWindow.current.Repaint();

            // If another action has not taken precedence, then just return to an idle state.
            // E.g Would not run if another action was taken such as clicking another node or clicking the empty graph.
            if (NodeEditorWindow.currentActivity == NodeEditorWindow.NodeActivity.Renaming)
            {
                NodeEditorWindow.currentActivity = NodeEditorWindow.NodeActivity.Idle;
            }
        }
    }
}