using System;
using System.Linq;
using System.Reflection;

public static class TypeExtensions
{
    /// <summary> Determines whether an instance of a specified type can be assigned to a variable of the current type. </summary>
    public static bool IsCastableFrom(this Type to, Type from)
    {
        if ( to.IsAssignableFrom ( from ) )
            return true;
        return from.GetMethods ( BindingFlags.Public | BindingFlags.Static ).Any ( m =>
        {
            return m.ReturnType == to && ( m.Name == "op_Implicit" || m.Name == "op_Explicit" );
        } );
    }
}
