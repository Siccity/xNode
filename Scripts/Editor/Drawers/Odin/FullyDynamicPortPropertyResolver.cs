#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XNode;
using XNodeEditor.Odin;
using static XNode.Node;

namespace XNodeEditor
{
	[ResolverPriority( -4 )]
	public class FullyDynamicPortPropertyResolver<T> : BaseMemberPropertyResolver<T>
		where T : Node
	{
		private List<OdinPropertyProcessor> processors;

		protected override InspectorPropertyInfo[] GetPropertyInfos()
		{
			if ( this.processors == null )
			{
				this.processors = OdinPropertyProcessorLocator.GetMemberProcessors( this.Property );
			}

			var includeSpeciallySerializedMembers = this.Property.ValueEntry.SerializationBackend != SerializationBackend.Unity;
			var infos = InspectorPropertyInfoUtility.CreateMemberProperties( this.Property, typeof( T ), includeSpeciallySerializedMembers );

			for ( int i = 0; i < this.processors.Count; i++ )
			{
				ProcessedMemberPropertyResolverExtensions.ProcessingOwnerType = typeof( T );
				this.processors[i].ProcessMemberProperties( infos );
			}

			// Find ports that aren't managed by an attribute
			if ( this.Property.Tree.UnitySerializedObject.targetObjects.Length == 1 )
			{
				var node = Property.Tree.UnitySerializedObject.targetObject as Node;
				var nodeType = typeof( T );

				string error;
				MemberInfo[] fieldsAndProperties;
				nodeType.FindMember()
					.IsFieldOrProperty()
					.TryGetMembers( out fieldsAndProperties, out error );
				var attributedMembers = fieldsAndProperties?.Where( x => x.GetAttribute<InputAttribute>() != null || x.GetAttribute<OutputAttribute>() != null );

				foreach ( var port in node.DynamicPorts )
				{
					if ( attributedMembers.Any( x => port.fieldName.StartsWith( string.Format( "{0} ", x.Name ) ) ) )
						continue;

					// value can't possibly matter here so many this "lie" is ok
					infos.AddValue( port.fieldName, () => 0, value => { },
						new AsDynamicPortNoDataAtribute()
						{
							BackingValue = ShowBackingValue.Never,
							FieldName = port.fieldName,
							InList = false,
							Node = node
						}
					);
				}

			}

			return InspectorPropertyInfoUtility.BuildPropertyGroupsAndFinalize( this.Property, typeof( T ), infos, includeSpeciallySerializedMembers );
		}
	}
}
#endif