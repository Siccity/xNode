#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using System;
using UnityEditor;
using UnityEngine;
using XNode;
using static XNode.Node;

namespace XNodeEditor.Odin
{
	internal abstract class AsDynamicPortAtribute : System.Attribute
	{
		internal string fieldName { get; set; }
		internal int index { get; set; }
		internal Node Node { get; set; }

		internal ConnectionType connectionType { get; set; }
		internal ShowBackingValue backingValue { get; set; }

		internal NodePort Port
		{
			get
			{
				return Node.GetPort( $"{fieldName} {index}" );
			}
		}
	}

	internal class AsDynamicPortNoDataAtribute : AsDynamicPortAtribute { }
	internal class AsDynamicPortWithDataAtribute : AsDynamicPortAtribute { }

	internal struct AsDynamicPortScope : IDisposable
	{
		public AsDynamicPortScope( NodePort port )
		{
			EditorGUILayout.BeginVertical();
			var rect = GUILayoutUtility.GetRect( 0f, float.MaxValue, 0f, 0f, GUI.skin.label, GUILayout.ExpandWidth( true ) );
			if ( NodeEditor.isNodeEditor )
			{
				if ( port.IsInput )
				{
					NodeEditorGUILayout.PortField( new Vector2( rect.xMin - 42, rect.center.y ), port );
				}
				else
				{
					NodeEditorGUILayout.PortField( new Vector2( rect.xMax + 21, rect.center.y ), port );
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
	internal class AsDynamicPortNoDataAttributeDrawer<T> : OdinAttributeDrawer<AsDynamicPortNoDataAtribute, T>
	{
		protected override void DrawPropertyLayout( GUIContent label )
		{
			if ( Attribute.Port == null )
				return;

			using ( new AsDynamicPortScope( Attribute.Port ) )
				CallNextDrawer( label );
		}
	}

	[DrawerPriority( 0.4, 0, 0 )]
	internal class AsDynamicPortWithDataAtributeDrawer<T> : OdinAttributeDrawer<AsDynamicPortWithDataAtribute, T>
	{
		protected bool drawData = false;

		protected override void DrawPropertyLayout( GUIContent label )
		{
			if ( Attribute.Port == null )
				return;

			if ( Event.current.type == EventType.Layout )
				drawData = Attribute.backingValue == ShowBackingValue.Always || Attribute.backingValue == ShowBackingValue.Unconnected && !Attribute.Port.IsConnected;

			using ( new AsDynamicPortScope( Attribute.Port ) )
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