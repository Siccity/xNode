namespace ExampleNodes {
    public class DisplayValue : Node {
        [Input] public float value;

        public float GetValue() {
            return GetInputByFieldName<float>("value", value);
        }
    }
}
