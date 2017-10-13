using UnityEngine;

[System.Serializable]
public class MathNode : ExampleNodeBase {
    [Input] public float c;
    [Input] public float b;
    [Output] public float result;
    public enum MathType { Add, Subtract, Multiply, Divide}
    public MathType mathType = MathType.Add;

    protected override void Init() {
        name = "Math";
    }

    public override object GetValue(NodePort port) {
        float a = GetInputFloat("c");
        float b = GetInputFloat("b");
        
        switch(port.fieldName) {
            case "result":
                switch(mathType) {
                    case MathType.Add: return a + b;
                    case MathType.Subtract: return a - b;
                    case MathType.Multiply: return a * b;
                    case MathType.Divide: return a / b;
                }
                break;
        }
        return 0f;
    }
}
