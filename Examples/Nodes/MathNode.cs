using UnityEngine;

[System.Serializable]
public class MathNode : Node {
    [Input] public float a;
    [Input] public float b;
    [Output] public float result;
    public enum ValueType { Float, Int }
    public enum MathType { Add, Subtract, Multiply, Divide}
    public ValueType valueType = ValueType.Float;
    public MathType mathType = MathType.Add;
}
