using XNode;

namespace BasicNodes {
    [System.Serializable]
    public class MathNode : Node {
        // Adding [Input] or [Output] is all you need to do to register a field as a valid port on your node 
        [Input] public float a;
        [Input] public float b;
        // The value of an output node field is not used for anything, but could be used for caching output results
        [Output] public float result;

        // UNEC will display this as an editable field - just like the normal inspector would
        public MathType mathType = MathType.Add;
        public enum MathType { Add, Subtract, Multiply, Divide }

        // GetValue should be overridden to return a value for any specified output port
        public override object GetValue(NodePort port) {

            // Get new a and b values from input connections. Fallback to field values if input is not connected
            float a = GetInputValue<float>("a", this.a);
            float b = GetInputValue<float>("b", this.b);

            // After you've gotten your input values, you can perform your calculations and return a value
            if (port.fieldName == "result")
                switch (mathType) {
                    case MathType.Add: default: return a + b;
                    case MathType.Subtract: return a - b;
                    case MathType.Multiply: return a * b;
                    case MathType.Divide: return a / b;
                }
            else return 0f;
        }
    }
}