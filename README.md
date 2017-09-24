
### UnityNodeEditorCore
A simple, userfriendly node editor framework for unity. Ideal as a base for custom state machines, dialogue systems, decision makers etc.

---
Node example:
```csharp
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
}
```
![editor](https://user-images.githubusercontent.com/6402525/30787371-1c3ae552-a187-11e7-853a-214914c2ba69.PNG)
