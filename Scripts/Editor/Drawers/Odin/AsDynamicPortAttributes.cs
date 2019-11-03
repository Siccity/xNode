#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using System;
using UnityEditor;
using UnityEngine;
using XNode;

namespace XNodeEditor.Odin
{
	internal abstract class AsDynamicPortAtribute : System.Attribute
	{
		public string FieldName { get; set; }
		public Node Node { get; set; }

		public bool InList { get; set; }
		public Node.ShowBackingValue BackingValue { get; set; }

		public NodePort Port
		{
			get
			{
				return Node.GetPort( FieldName );
			}
		}
	}

	internal class AsDynamicPortNoDataAtribute : AsDynamicPortAtribute { }
	internal class AsDynamicPortWithDataAtribute : AsDynamicPortAtribute { }

	internal struct AsDynamicPortScope : IDisposable
	{
		public AsDynamicPortScope( NodePort port, bool inList )
		{
			EditorGUILayout.BeginVertical();
			var rect = GUILayoutUtility.GetRect( 0f, float.MaxValue, 0f, 0f, GUI.skin.label, GUILayout.ExpandWidth( true ) );
			if ( port != null && NodeEditor.isNodeEditor )
			{
				if ( port.IsInput )
				{
					Vector2 offset;
					if ( inList )
						offset = new Vector2( -42, 0 );
					else
						offset = new Vector2( -18, 0 );

					NodeEditorGUILayout.PortField( new Vector2( rect.xMin, rect.center.y ) + offset, port );
				}
				else
				{
					Vector2 offset;
					if ( inList )
						offset = new Vector2( 21, 0 );
					else
						offset = new Vector2( 0, 0 );

					NodeEditorGUILayout.PortField( new Vector2( rect.xMax, rect.center.y ) + offset, port );
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

			using ( new AsDynamicPortScope( Attribute.Port, Attribute.InList ) )
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
				drawData = Attribute.BackingValue == Node.ShowBackingValue.Always || Attribute.BackingValue == Node.ShowBackingValue.Unconnected && !Attribute.Port.IsConnected;

			using ( new AsDynamicPortScope( Attribute.Port, Attribute.InList ) )
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