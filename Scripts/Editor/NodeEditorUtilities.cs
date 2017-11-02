using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary> A set of editor-only utilities and extensions for UnityNodeEditorBase </summary>
public static class NodeEditorUtilities {

    public static bool GetAttrib<T>(Type classType, out T attribOut) where T : Attribute {
        object[] attribs = classType.GetCustomAttributes(typeof(T), false);
        return GetAttrib(attribs, out attribOut);
    }

    public static bool GetAttrib<T>(object[] attribs, out T attribOut) where T : Attribute {
        for (int i = 0; i < attribs.Length; i++) {
            if (attribs[i].GetType() == typeof(T)) {
                attribOut = attribs[i] as T;
                return true;
            }
        }
        attribOut = null;
        return false;
    }

    public static bool HasAttrib<T>(object[] attribs) where T : Attribute {
        for (int i = 0; i < attribs.Length; i++) {
            if (attribs[i].GetType() == typeof(T)) {
                return true;
            }
        }
        return false;
    }

    /// <summary> Turns camelCaseString into Camel Case String </summary>
    public static string PrettifyCamelCase(this string camelCase) {
        if (string.IsNullOrEmpty(camelCase)) return ""; 
        string s = System.Text.RegularExpressions.Regex.Replace(camelCase, "([A-Z])", " $1", System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        return char.ToUpper(s[0]) + s.Substring(1);
    }

    /// <summary> Returns true if this can be casted to <see cref="Type"/></summary>
    public static bool IsCastableTo(this Type from, Type to) {
        if (to.IsAssignableFrom(from)) return true;
        var methods = from.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(
                m => m.ReturnType == to &&
                (m.Name == "op_Implicit" ||
                    m.Name == "op_Explicit")
            );
        return methods.Count() > 0;
    }
}