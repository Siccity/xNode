
### UnityNodeEditorCore
A simple, userfriendly node editor framework for unity. Ideal as a base for custom state machines, dialogue systems, decision makers etc.

![editor](https://user-images.githubusercontent.com/6402525/31379481-a9c15950-adae-11e7-91c4-387dd020261e.png)

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

Join the [Discord](https://discord.gg/qgPrHv4 "Join Discord server") server to leave feedback or get support.
Feel free to also leave suggestions/requests in the [issues](https://github.com/Siccity/UnityNodeEditorCore/issues "Go to Issues") page.

Projects using UnityNodeEditorCore:
* [Graphmesh](https://github.com/Siccity/Graphmesh "Go to github page")
