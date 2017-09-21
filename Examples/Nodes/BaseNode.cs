using UnityEngine;

[System.Serializable]
public class BaseNode : Node {

    public bool concat;
    [TextArea]
    public string SomeString;
    [Header("New stuff")]
    public Color col;
    public AnimationCurve anim;
    public Vector3 vec;
    protected override void Init() {
        inputs = new NodePort[2];
        inputs[0] = CreateNodeInput("IntInput", typeof(int));
        inputs[1] = CreateNodeInput("StringInput", typeof(string));
        outputs = new NodePort[1];
        outputs[0] = CreateNodeOutput("StringOutput", typeof(string));
    }
}
