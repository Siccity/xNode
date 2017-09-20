using System.Reflection;
using System.Linq;
using System;

/// <summary> Contains reflection-related info </summary>
public partial class NodeEditorWindow {
    
    public static Type[] nodeTypes { get { return _nodeTypes != null ? _nodeTypes : _nodeTypes = GetNodeTypes(); } }
    private static Type[] _nodeTypes;

    public static Type[] GetNodeTypes() {
        //Get all classes deriving from Node via reflection
        Type derivedType = typeof(Node);
        Assembly assembly = Assembly.GetAssembly(derivedType);
        return assembly.GetTypes().Where(t => 
            t != derivedType &&
            derivedType.IsAssignableFrom(t)
            ).ToArray();
    }
}
