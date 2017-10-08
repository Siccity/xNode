using UnityEngine;

[System.Serializable]
public class MathNode : Node {
    [Input] public float a;
    [Input] public float b;
    [Output] public float result;
    public enum MathType { Add, Subtract, Multiply, Divide}
    public MathType mathType = MathType.Add;

    protected override void Init() {
        name = "Math";
    }

    public override void OnCreateConnection(NodePort from, NodePort to) {

    }
}
