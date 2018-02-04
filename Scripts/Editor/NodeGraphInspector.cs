using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using XNode;
using XNodeEditor;

[CustomEditor(typeof(XNode.NodeGraph), true)]
public class NodeGraphInspector : Editor
{
	SerializedProperty nodesProp;
	SerializedProperty variablesProp;

	void OnEnable()
	{
		nodesProp = serializedObject.FindProperty("nodes");
		variablesProp = serializedObject.FindProperty("variables");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		DrawVariables();

		serializedObject.ApplyModifiedProperties();
	}

	void DrawVariables()
	{
		EditorGUILayout.LabelField("Variables");
		EditorGUILayout.Space();
		for (int i = 0; i < variablesProp.arraySize; i++)
			DrawVariable(i);
		DrawVariablesActions();
	}

	void DrawVariable(int index)
	{
		var variableProp = variablesProp.GetArrayElementAtIndex(index);
		
		var idProp = variableProp.FindPropertyRelative("id");
		var typeProp = variableProp.FindPropertyRelative("typeString");


		DrawVariableId(idProp);
		DrawVariableType(typeProp);
		DrawVariableValue(variableProp, typeProp.stringValue);
		DrawVariableActions(index);

		EditorGUILayout.Space();		
	}

	void DrawVariableId(SerializedProperty idProp)
	{
		EditorGUILayout.PropertyField(idProp);
	}

	void DrawVariableType(SerializedProperty typeProp)
	{
		List<string> options = new List<string>();
		options.Add(typeProp.stringValue);
		int idx = 0;

		List<string> additionalTypes = new List<string>();
		additionalTypes.Add(typeof(float).AssemblyQualifiedName);
		additionalTypes.Add(typeof(int).AssemblyQualifiedName);
		additionalTypes.Add(typeof(bool).AssemblyQualifiedName);
		additionalTypes.Add(typeof(string).AssemblyQualifiedName);
		additionalTypes.Add(typeof(Vector3).AssemblyQualifiedName);
		
		/*
		Assembly asm = typeof(Vector3).Assembly;
		foreach (var colorPair in NodeEditorPreferences.typeColors)
		{
			var type = asm.GetType(colorPair.Key, false);
			Debug.Log(colorPair.Key + "   " + type);

			if (type == null)
				continue;
			if (additionalTypes.Contains(type.AssemblyQualifiedName))
				continue;
			additionalTypes.Add(colorPair.Key);
		}
		*/

		foreach (var addType in additionalTypes)
		{
			if (!options.Contains(addType))
				options.Add(addType);
		}

		List<string> prettyOptions = new List<string>();

		foreach (var option in options)
		{
			prettyOptions.Add(System.Type.GetType(option, false).PrettyName());
		}


		idx = EditorGUILayout.Popup(idx, prettyOptions.ToArray());

		typeProp.stringValue = options[idx];
	}

	void DrawVariableValue(SerializedProperty variableProp, string type)
	{
		if (type != "")
		{
			type = System.Type.GetType(type, false).PrettyName();
			type = NodeGraph.GetSafeType(type);
			
			var valprop = variableProp.FindPropertyRelative(type + "Value");
			
			if (valprop == null && type != "object")
			{
				type = "object";
				valprop = variableProp.FindPropertyRelative(type + "Value");
			}

			if (valprop != null)
				EditorGUILayout.PropertyField(valprop);
			else
				EditorGUILayout.LabelField("Value");
		}
		else
			EditorGUILayout.LabelField("Value");
	}

	void DrawVariableActions(int index)
	{
		if (GUILayout.Button("Remove variable", GUILayout.Width(120)))
		{
			variablesProp.DeleteArrayElementAtIndex(index);
		}
	}

	void DrawVariablesActions()
	{
		GUILayout.BeginHorizontal();

		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Add new variable", GUILayout.Width(120)))
		{
			variablesProp.InsertArrayElementAtIndex(variablesProp.arraySize);
			var newVarProp = variablesProp.GetArrayElementAtIndex(variablesProp.arraySize -1);
			newVarProp.FindPropertyRelative("id").stringValue = (target as NodeGraph).GetSafeId("new_variable");
			newVarProp.FindPropertyRelative("typeString").stringValue = typeof(float).AssemblyQualifiedName;
		}

		GUILayout.EndHorizontal();
	}
}
