#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using XNode;

namespace XNodeEditor.Odin
{
	public class SimpleNodePortDrawer<T> : OdinValueDrawer<T>
		where T : NodePort
	{
		protected override void Initialize()
		{
			base.Initialize();

			this.SkipWhenDrawing = !NodeEditor.isNodeEditor;
		}

		protected override void DrawPropertyLayout( GUIContent label )
		{
			if ( label != null )
				EditorGUILayout.LabelField( label );
		}
	}
}
#endif