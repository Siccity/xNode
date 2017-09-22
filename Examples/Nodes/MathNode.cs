using UnityEngine;

[System.Serializable]
public class MathNode : Node {

    public enum ValueType { Float, Int }
    public enum MathType { Add, Subtract, Multiply, Divide}
    public ValueType valueType = ValueType.Float;
    public MathType mathType = MathType.Add;

    protected override void Init() {
        inputs = new NodePort[2];
        inputs[0] = CreateNodeInput("A", typeof(float));
        inputs[1] = CreateNodeInput("B", typeof(float));
        outputs = new NodePort[1];
        outputs[0] = CreateNodeOutput("Result", typeof(float));
    }

    public void OnValidate() {
        System.Type type = typeof(int);
        if (valueType == ValueType.Float) type = typeof(float);
        inputs[0].type = type;
        inputs[1].type = type;
        outputs[0].type = type;
    }
}
