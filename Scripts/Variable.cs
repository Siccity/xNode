using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XNode
{
	[System.Serializable]
	public class Variable
	{
		public string id;
		public string typeString;
		public System.Type type
		{
			get
			{
				if (typeString == typeof(float).AssemblyQualifiedName) return typeof(float);
				if (typeString == typeof(int).AssemblyQualifiedName) return typeof(int);
				if (typeString == typeof(string).AssemblyQualifiedName) return typeof(string);
				if (typeString == typeof(bool).AssemblyQualifiedName) return typeof(bool);
				if (typeString == typeof(Vector3).AssemblyQualifiedName) return typeof(Vector3);
				return typeof(Object);
			}
		}

		public object Value
		{
			get
			{
				if (typeString == typeof(float).AssemblyQualifiedName) return floatValue;
				if (typeString == typeof(int).AssemblyQualifiedName) return intValue;
				if (typeString == typeof(string).AssemblyQualifiedName) return stringValue;
				if (typeString == typeof(bool).AssemblyQualifiedName) return boolValue;
				if (typeString == typeof(Vector3).AssemblyQualifiedName) return vector3Value;
				return objectValue;
			}
		}

		public string stringValue;
		public float floatValue;
		public int intValue;
		public bool boolValue;
		public Vector3 vector3Value;
		
		// more here

		public Object objectValue;


		public Variable()
		{
			id = "new_variable";
			typeString = typeof(float).AssemblyQualifiedName;
		}
	}
}
