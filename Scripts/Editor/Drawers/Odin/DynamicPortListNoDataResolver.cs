#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XNode;
using XNodeEditor.Odin;

namespace XNodeEditor
{
	[ResolverPriority( 10 )]
	public class DynamicPortListNoDataResolver<TNotAList> : BaseOrderedCollectionResolver<TNotAList>
	{
		public override bool CanResolveForPropertyFilter( InspectorProperty property )
		{
			var input = property.GetAttribute<Node.InputAttribute>();
			if ( input != null )
				return input.dynamicPortList;

			var output = property.GetAttribute<Node.OutputAttribute>();
			if ( output != null )
				return output.dynamicPortList;

			return false;
		}

		private Dictionary<int, InspectorPropertyInfo> childInfos = new Dictionary<int, InspectorPropertyInfo>();

		public override InspectorPropertyInfo GetChildInfo( int childIndex )
		{
			if ( childIndex < 0 || childIndex >= this.ChildCount )
			{
				throw new IndexOutOfRangeException();
			}

			InspectorPropertyInfo result;

			if ( !this.childInfos.TryGetValue( childIndex, out result ) )
			{
				var attributes = this.Property.Attributes.Where( attr => !attr.GetType().IsDefined( typeof( DontApplyToListElementsAttribute ), true ) );
				var labelTextAttribute = attributes.OfType<LabelTextAttribute>().SingleOrDefault();
				var hideLabelAttribute = attributes.OfType<HideLabelAttribute>().SingleOrDefault();

				attributes = attributes
				.AppendIf( true, GetPortAttribute( Property.Name, childIndex ) )
				.AppendIf( labelTextAttribute == null && hideLabelAttribute == null, new LabelTextAttribute( string.Format( "{0} {1}", Property.Name, childIndex ) ) );

				result = InspectorPropertyInfo.CreateValue(
						name: CollectionResolverUtilities.DefaultIndexToChildName( childIndex ),
						order: childIndex,
						serializationBackend: this.Property.BaseValueEntry.SerializationBackend,
						getterSetter: new GetterSetter<TNotAList, NodePort>(
							getter: ( ref TNotAList list ) => ports[childIndex], // Return absolutely nothing? Return a port?
							setter: ( ref TNotAList list, NodePort element ) => ports[childIndex] = element ),
						attributes: attributes.ToArray() );

				this.childInfos[childIndex] = result;
			}

			return result;
		}

		internal AsDynamicPortNoDataAtribute GetPortAttribute( string fieldName, int index )
		{
			return new AsDynamicPortNoDataAtribute()
			{
				FieldName = string.Format( "{0} {1}", fieldName, index ),
				InList = true,
				Node = node,

				BackingValue = backingValue
			};
		}

		protected override void Add( TNotAList collection, object value )
		{
			int nextId = this.ChildCount;

			if ( IsInput )
				this.node.AddDynamicInput( typeof( TNotAList ), connectionType, typeConstraint, string.Format( "{0} {1}", Property.Name, nextId ) );
			else
				this.node.AddDynamicOutput( typeof( TNotAList ), connectionType, typeConstraint, string.Format( "{0} {1}", Property.Name, nextId ) );

			lastRemovedConnections.Clear();

			//base.Add( collection, value );
		}

		protected override void InsertAt( TNotAList collection, int index, object value )
		{
			int nextId = this.ChildCount;

			// Remove happens before insert and we lose all the connections
			// Add a new port at the end
			if ( IsInput )
				this.node.AddDynamicInput( typeof( TNotAList ), connectionType, typeConstraint, string.Format( "{0} {1}", Property.Name, nextId ) );
			else
				this.node.AddDynamicOutput( typeof( TNotAList ), connectionType, typeConstraint, string.Format( "{0} {1}", Property.Name, nextId ) );

			var dynamicPorts = this.ports;

			// Move everything down to make space
			for ( int k = dynamicPorts.Count - 1; k > index; --k )
			{
				for ( int j = 0; j < dynamicPorts[k - 1].ConnectionCount; j++ )
				{
					XNode.NodePort other = dynamicPorts[k - 1].GetConnection( j );
					dynamicPorts[k - 1].Disconnect( other );
					dynamicPorts[k].Connect( other );
				}
			}

			// Let's just re-add connections to this node that were probably his
			foreach ( var c in lastRemovedConnections )
				dynamicPorts[index].Connect( c );

			lastRemovedConnections.Clear();

			//base.InsertAt( collection, index, value );
		}

		protected override void Remove( TNotAList collection, object value )
		{
			//int index = collection.IndexOf( (TElement)value );
			//RemoveAt( collection, index );
			throw new NotImplementedException();
		}

		protected List<NodePort> lastRemovedConnections = new List<NodePort>();

		protected override void RemoveAt( TNotAList collection, int index )
		{
			var dynamicPorts = this.ports;

			if ( dynamicPorts[index] == null )
			{
				Debug.LogWarning( "No port found at index " + index + " - Skipped" );
			}
			else if ( dynamicPorts.Count <= index )
			{
				Debug.LogWarning( "DynamicPorts[" + index + "] out of range. Length was " + dynamicPorts.Count + " - Skipped" );
			}
			else
			{
				lastRemovedConnections.Clear();
				lastRemovedConnections.AddRange( dynamicPorts[index].GetConnections() );

				// Clear the removed ports connections
				dynamicPorts[index].ClearConnections();
				// Move following connections one step up to replace the missing connection
				for ( int k = index + 1; k < dynamicPorts.Count; k++ )
				{
					for ( int j = 0; j < dynamicPorts[k].ConnectionCount; j++ )
					{
						XNode.NodePort other = dynamicPorts[k].GetConnection( j );
						dynamicPorts[k].Disconnect( other );
						dynamicPorts[k - 1].Connect( other );
					}
				}

				// Remove the last dynamic port, to avoid messing up the indexing
				node.RemoveDynamicPort( dynamicPorts[dynamicPorts.Count() - 1].fieldName );
			}

			//base.RemoveAt( collection, index );
		}

		protected override void Clear( TNotAList collection )
		{
			foreach ( var port in ports )
				node.RemoveDynamicPort( port );

			lastRemovedConnections.Clear();

			//base.Clear( collection );
		}

		public override bool ChildPropertyRequiresRefresh( int index, InspectorPropertyInfo info )
		{
			return false;
		}

		protected override bool CollectionIsReadOnly( TNotAList collection )
		{
			return false;
		}

		protected override int GetChildCount( TNotAList value )
		{
			return ports.Count;
		}

		public override int ChildNameToIndex( string name )
		{
			return CollectionResolverUtilities.DefaultChildNameToIndex( name );
		}

		public override Type ElementType { get { return typeof( NodePort ); } }

		protected Node node { get { return ( Property.Tree.UnitySerializedObject.targetObject as Node ); } }

		protected List<NodePort> ports
		{
			get
			{
				// This created a lot of garbage
				List<NodePort> dynamicPorts = new List<NodePort>();
				for ( int i = 0; i < int.MaxValue; ++i )
				{
					var nodePort = node.GetPort( string.Format( "{0} {1}", Property.Name, i ) );
					if ( nodePort == null )
						break;

					dynamicPorts.Add( nodePort );
				}
				return dynamicPorts;
			}
		}

		protected bool IsInput { get { return Property.GetAttribute<Node.InputAttribute>() != null; } }

		public Node.ConnectionType connectionType { get { return IsInput ? Property.GetAttribute<Node.InputAttribute>().connectionType : Property.GetAttribute<Node.OutputAttribute>().connectionType; } }
		public Node.TypeConstraint typeConstraint { get { return IsInput ? Property.GetAttribute<Node.InputAttribute>().typeConstraint : Property.GetAttribute<Node.OutputAttribute>().typeConstraint; } }
		public Node.ShowBackingValue backingValue { get { return IsInput ? Property.GetAttribute<Node.InputAttribute>().backingValue : Property.GetAttribute<Node.OutputAttribute>().backingValue; } }
	}
}
#endif