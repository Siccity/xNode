using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace XNodeEditor {
    public static class EditorStylesHacks {
        private static FieldInfo EditorLabelField;
        private static FieldInfo EditorFoldoutField;
        private static FieldInfo EditorStylesInstanceField;
        private static EditorStyles EditorStylesInstance;

        private static readonly BindingFlags EditorStylesBindingFlags =
            BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance;

        public static GUIStyle Label {
            get => EditorStyles.label;
            set {
                CacheEditorStylesInstance();
                if (EditorLabelField == null) {
                    EditorLabelField = typeof(EditorStyles).GetField("m_Label", EditorStylesBindingFlags);
                }

                EditorLabelField.SetValue(EditorStylesInstance, value);
            }
        }

        public static GUIStyle Foldout {
            get => EditorStyles.foldout;
            set {
                CacheEditorStylesInstance();
                if (EditorFoldoutField == null) {
                    EditorFoldoutField = typeof(EditorStyles).GetField("m_Foldout", EditorStylesBindingFlags);
                }

                EditorFoldoutField.SetValue(EditorStylesInstance, value);
            }
        }

        private static void CacheEditorStylesInstance() {
            if (EditorStylesInstanceField == null) {
                EditorStylesInstanceField = typeof(EditorStyles).GetField("s_Current", EditorStylesBindingFlags);
            }

            if (EditorStylesInstance == null) {
                EditorStylesInstance = EditorStylesInstanceField.GetValue(null) as EditorStyles;
            }
        }
    }
}