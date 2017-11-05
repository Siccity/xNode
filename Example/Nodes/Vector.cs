using UnityEngine;
using XNode;

namespace BasicNodes {
    public class Vector : Node {
        [Input] public float x, y, z;
        [Output] public Vector3 vector;

        public override object GetValue(NodePort port) {
            vector.x = GetInputValue<float>("x", this.x);
            vector.y = GetInputValue<float>("y", this.y);
            vector.z = GetInputValue<float>("z", this.z);
            return vector;
        }
    }
}