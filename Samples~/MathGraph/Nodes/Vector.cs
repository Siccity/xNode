using UnityEngine;

namespace XNode.Examples.MathNodes {
    public class Vector : XNode.Node {
        [Input] public float x, y, z;
        [Output] public Vector3 vector;

        public override object GetValue(XNode.NodePort port) {
            vector.x = GetInputValue<float>("x", this.x);
            vector.y = GetInputValue<float>("y", this.y);
            vector.z = GetInputValue<float>("z", this.z);
            return vector;
        }
    }
}