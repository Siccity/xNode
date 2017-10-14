using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNode : Node {

	public int myInt;
	[Input(false)] public float myFloat;
	public double myDouble;
	public long myLong;
	public bool myBool;
	[Input(true)] public string myString;
	public Rect myRect;
	[Input(true)] public Vector2 myVec2;
	[Input(true)] public Vector3 myVec3;
	public Vector4 myVec4;
	public Color myColor;
	public AnimationCurve myCurve;
}
