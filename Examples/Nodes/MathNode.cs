using UnityEngine;

[System.Serializable]
public class MathNode : Node {

    public int someValue;
    public enum MathType { Add, Subtract, Multiply, Divide}
    public MathType mathType = MathType.Add;

    protected override void Init() {
        inputs = new NodePort[2];
        inputs[0] = CreateNodeInput("A", typeof(float));
        inputs[1] = CreateNodeInput("B", typeof(float));
        outputs = new NodePort[1];
        outputs[0] = CreateNodeOutput("Result", typeof(float));
    }
}
