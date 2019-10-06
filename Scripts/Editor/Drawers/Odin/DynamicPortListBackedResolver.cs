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
using static XNode.Node;

namespace XNodeEditor.Odin
{
	[DrawerPriority( 0.4, 0, 0 )]
	public class DynamicPortCollectionOverrideDrawer<T> : CollectionDrawer<T>
	{
		protected override bool CanDrawValueProperty( InspectorProperty property )
		{
			var input = property.GetAttribute<InputAttribute>();
			if ( input != null )
				return input.dynamicPortList;

			var output = property.GetAttribute<OutputAttribute>();
			if ( output != null )
				return output.dynamicPortList;

			return false;
		}
	}

	[ResolverPriority( 15 )]
	public class DynamicPortListBackedResolver<TList, TElement> : StrongListPropertyResolver<TList, TElement>
		where TList : IList<TElement>
	{
		public override bool CanResolveForPropertyFilter( InspectorProperty property )
		{
			var input = property.GetAttribute<InputAttribute>();
			if ( input != null )
				return input.dynamicPortList;

			var output = property.GetAttribute<OutputAttribute>();
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
				.Append( GetPortAttribute( Property.Name, childIndex ) )
				.AppendIf( labelTextAttribute == null && hideLabelAttribute == null, new LabelTextAttribute( $"{Property.Name} {childIndex}" ) );

				result = InspectorPropertyInfo.CreateValue(
						name: CollectionResolverUtilities.DefaultIndexToChildName( childIndex ),
						order: childIndex,
						serializationBackend: this.Property.BaseValueEntry.SerializationBackend,
						new GetterSetter<TList, TElement>(
							getter: ( ref TList list ) => list[childIndex],
							setter: ( ref TList list, TElement element ) => list[childIndex] = element ),
						attributes: attributes.ToArray() );

				this.childInfos[childIndex] = result;
			}

			return result;
		}

		internal AsDynamicPortWithDataAtribute GetPortAttribute( string fieldName, int index )
		{
			return new AsDynamicPortWithDataAtribute()
			{
				fieldName = fieldName,
				index = index,
				Node = node,

				connectionType = connectionType,
				backingValue = backingValue
			};
		}

		protected override void Add( TList collection, object value )
		{
			int nextId = this.ChildCount;

			if ( IsInput )
				this.node.AddDynamicInput( typeof( TElement ), connectionType, typeConstraint, $"{Property.Name} {nextId}" );
			else
				this.node.AddDynamicOutput( typeof( TElement ), connectionType, typeConstraint, $"{Property.Name} {nextId}" );

			lastRemovedConnections.Clear();

			base.Add( collection, value );
		}

		protected override void InsertAt( TList collection, int index, object value )
		{
			int nextId = this.ChildCount;

			// Remove happens before insert and we lose all the connections
			// Add a new port at the end
			if ( IsInput )
				this.node.AddDynamicInput( typeof( TElement ), connectionType, typeConstraint, $"{Property.Name} {nextId}" );
			else
				this.node.AddDynamicOutput( typeof( TElement ), connectionType, typeConstraint, $"{Property.Name} {nextId}" );

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

			base.InsertAt( collection, index, value );
		}

		protected override void Remove( TList collection, object value )
		{
			int index = collection.IndexOf( (TElement)value );
			RemoveAt( collection, index );
		}

		protected List<NodePort> lastRemovedConnections = new List<NodePort>();

		protected override void RemoveAt( TList collection, int index )
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

			base.RemoveAt( collection, index );
		}

		protected override void Clear( TList collection )
		{
			foreach ( var port in ports )
				node.RemoveDynamicPort( port );

			lastRemovedConnections.Clear();

			base.Clear( collection );
		}

		protected Node node => ( Property.Tree.UnitySerializedObject.targetObject as Node );
		protected List<NodePort> ports
		{
			get
			{
				// This created a lot of garbage
				List<NodePort> dynamicPorts = new List<NodePort>();
				for ( int i = 0; i < int.MaxValue; ++i )
				{
					var nodePort = node.GetPort( $"{Property.Name} {i}" );
					if ( nodePort == null )
						break;

					dynamicPorts.Add( nodePort );
				}
				return dynamicPorts;
			}
		}

		protected bool IsInput => Property.GetAttribute<InputAttribute>() != null;

		public ConnectionType connectionType => IsInput ? Property.GetAttribute<InputAttribute>().connectionType : Property.GetAttribute<OutputAttribute>().connectionType;
		public TypeConstraint typeConstraint => IsInput ? Property.GetAttribute<InputAttribute>().typeConstraint : Property.GetAttribute<OutputAttribute>().typeConstraint;
		public ShowBackingValue backingValue => IsInput ? Property.GetAttribute<InputAttribute>().backingValue : Property.GetAttribute<OutputAttribute>().backingValue;
	}

	public class DynamicPortListAttributeProcessor<T> : OdinAttributeProcessor<T>
	{
		public override bool CanProcessSelfAttributes( InspectorProperty property )
		{
			// We can guess that it's going to fall in here
			var input = property.GetAttribute<InputAttribute>();
			if ( input != null )
				return input.dynamicPortList;

			var output = property.GetAttribute<OutputAttribute>();
			if ( output != null )
				return output.dynamicPortList;

			return false;
		}

		public override void ProcessSelfAttributes( InspectorProperty property, List<Attribute> attributes )
		{
			var listDrawerSettingsAttribute = attributes.GetOrAddAttribute<ListDrawerSettingsAttribute>();
			listDrawerSettingsAttribute.Expanded = true;
			listDrawerSettingsAttribute.ShowPaging = false;
			listDrawerSettingsAttribute.AlwaysAddDefaultValue = true;
		}
	}
}
#endif