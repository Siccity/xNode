[System.Serializable]
public class ConstantValue : ExampleNodeBase {
    public float a;
    [Output] public float value;

    protected override void Init() {
        name = "Constant Value";
    }

    public override object GetValue(NodePort port) {
        return a;
    }
}
