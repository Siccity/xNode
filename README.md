[![Discord](https://img.shields.io/discord/361769369404964864.svg)](https://discord.gg/qgPrHv4)
[![GitHub issues](https://img.shields.io/github/issues/Siccity/UnityNodeEditorCore.svg)](https://github.com/Siccity/UnityNodeEditorCore/issues)
[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg)](https://raw.githubusercontent.com/Siccity/UnityNodeEditorCore/master/LICENSE.md)
[![GitHub Wiki](https://img.shields.io/badge/wiki-available-brightgreen.svg)](https://github.com/Siccity/UnityNodeEditorCore/wiki)

### UnityNodeEditorCore
Thinking of developing a node-based plugin? Then this is for you. You can download it as an archive and unpack to a new unity project, or connect it as git submodule.

UNEC is ideal as a base for custom state machines, dialogue systems, decision makers etc.

![editor](https://user-images.githubusercontent.com/6402525/31379481-a9c15950-adae-11e7-91c4-387dd020261e.png)

### Key features of UnityNodeEditorCore:
* Lightweight in runtime
* Very little boilerplate code
* Strong separation of editor and runtime code
* No runtime reflection (unless you need to edit/build node graphs at runtime. In this case, all reflection is cached.)
* Does not rely on any 3rd party plugins
* Custom node inspector code is very similar to regular custom inspector code

### Node example:
```csharp
[System.Serializable]
public class MathNode : Node {
    [Input] public float a;
    [Input] public float b;
    [Output] public float result;
    public enum MathType { Add, Subtract, Multiply, Divide}
    public MathType mathType = MathType.Add;
    
    public override object GetValue(NodePort port) {
        if (port.fieldName == "result") {
            float a = GetInputFloat("a");
            float b = GetInputFloat("b");
            switch(mathType) {
                case MathType.Add: return a + b;
                case MathType.Subtract: return a - b;
                case MathType.Multiply: return a * b;
                case MathType.Divide: return a / b;
            }
        }
        return 0f;
    }
}
```

Join the [Discord](https://discord.gg/qgPrHv4 "Join Discord server") server to leave feedback or get support.
Feel free to also leave suggestions/requests in the [issues](https://github.com/Siccity/UnityNodeEditorCore/issues "Go to Issues") page.

Projects using UnityNodeEditorCore:
* [Graphmesh](https://github.com/Siccity/Graphmesh "Go to github page")
