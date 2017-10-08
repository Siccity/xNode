using UnityEngine;

[System.Serializable]
public class BaseNode : Node {

    [Input] public string input;
    [Output] public string output;
    public bool concat;
    [TextArea]
    public string SomeString;
    [Header("New stuff")]
    public Color col;
    public AnimationCurve anim;
    public Vector3 vec;
}
