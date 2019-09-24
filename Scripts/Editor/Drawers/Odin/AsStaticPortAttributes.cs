#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using System;
using UnityEditor;
using UnityEngine;
using XNode;
using static XNode.Node;

namespace XNodeEditor.Odin
{
	internal struct AsStaticPortScope : IDisposable
	{
		public AsStaticPortScope( NodePort port )
		{
			EditorGUILayout.BeginVertical();
			var rect = GUILayoutUtility.GetRect( 0f, float.MaxValue, 0f, 0f, GUI.skin.label, GUILayout.ExpandWidth( true ) );
			if ( NodeEditor.isNodeEditor )
			{
				if ( port.IsInput )
				{
					NodeEditorGUILayout.PortField( new Vector2( rect.xMin - 18, rect.center.y + 2 ), port );
				}
				else
				{
					NodeEditorGUILayout.PortField( new Vector2( rect.xMax + 2, rect.center.y + 2 ), port );
				}
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.BeginVertical();
		}

		public void Dispose()
		{
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
	}

	[DrawerPriority( 0.4, 0, 0 )]
	internal class InputAttributeDrawer<T> : OdinAttributeDrawer<InputAttribute, T>
	{
		protected bool drawData = false;

		protected override void DrawPropertyLayout( GUIContent label )
		{
			NodePort port = ( Property.Tree.UnitySerializedObject.targetObject as Node ).GetInputPort( Property.Name );
			if ( Event.current.type == EventType.Layout )
				drawData = Attribute.backingValue == ShowBackingValue.Always || Attribute.backingValue == ShowBackingValue.Unconnected && !port.IsConnected;

			using ( new AsStaticPortScope( port ) )
			{
				if ( drawData )
					CallNextDrawer( label );
				else
					EditorGUILayout.LabelField( label );
			}
		}
	}

	[DrawerPriority( 0.4, 0, 0 )]
	internal class OutputAttributeDrawer<T> : OdinAttributeDrawer<OutputAttribute, T>
	{
		protected bool drawData = false;

		protected override void DrawPropertyLayout( GUIContent label )
		{
			NodePort port = ( Property.Tree.UnitySerializedObject.targetObject as Node ).GetOutputPort( Property.Name );
			if ( Event.current.type == EventType.Layout )
				drawData = Attribute.backingValue == ShowBackingValue.Always || Attribute.backingValue == ShowBackingValue.Unconnected && !port.IsConnected;

			using ( new AsStaticPortScope( port ) )
			{
				if ( drawData )
					CallNextDrawer( label );
				else
					EditorGUILayout.LabelField( label );
			}
		}
	}
}
#endif