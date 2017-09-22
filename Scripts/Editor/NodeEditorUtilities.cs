using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
}
