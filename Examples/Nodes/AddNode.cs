using UnityEngine;

[System.Serializable]
public class AddNode : Node {

    public int someValue;

    protected override void Init() {
        inputs = new NodePort[2];
        inputs[0] = CreateNodeInput("A", typeof(float));
        inputs[1] = CreateNodeInput("B", typeof(float));
        outputs = new NodePort[1];
        outputs[0] = CreateNodeOutput("Result", typeof(float));
    }
}
