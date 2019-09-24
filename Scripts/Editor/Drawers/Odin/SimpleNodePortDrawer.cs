#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using XNode;

namespace XNodeEditor.Odin
{
	[DrawerPriority( 0.3, 0, 0 )]
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

	public class NodePortAttributeProcessor : OdinAttributeProcessor<NodePort>
	{
		public override bool CanProcessSelfAttributes( InspectorProperty property )
		{
			return true;
		}

		public override bool CanProcessChildMemberAttributes( InspectorProperty parentProperty, MemberInfo member )
		{
			return false;
		}

		public override void ProcessSelfAttributes( InspectorProperty property, List<Attribute> attributes )
		{
			attributes.Add( new HideReferenceObjectPickerAttribute() );
		}
	}
}
#endif