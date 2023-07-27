using System;
/// <summary> Overrides the ValueType of the Port, to have a ValueType different from the type of its serializable field </summary>
/// <remarks> Especially useful in Dynamic Port Lists to create Value-Port Pairs with different type. </remarks>
[AttributeUsage(AttributeTargets.Field)]
public class PortTypeOverrideAttribute : Attribute {
    public Type type;
    /// <summary> Overrides the ValueType of the Port </summary>
    /// <param name="type">ValueType of the Port</param>
    public PortTypeOverrideAttribute(Type type) {
        this.type = type;
    }
}
