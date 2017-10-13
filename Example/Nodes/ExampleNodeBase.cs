using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleNodeBase : Node {

	public float GetInputFloat(string fieldName) {
		float result = 0f;
        NodePort port = GetInputByFieldName(fieldName);
        if (port == null) return result;
        int connectionCount = port.ConnectionCount;
        for (int i = 0; i < connectionCount; i++) {
            NodePort connection = port.GetConnection(i);
            if (connection == null) continue;
            object obj = connection.GetValue();
            if (obj == null) continue;
            if (connection.type == typeof(int)) result += (int)obj;
            else if (connection.type == typeof(float)) result += (float)obj;
        }
        return result;
	}
}
