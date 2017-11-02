namespace BasicNodes {
    public class DisplayValue : Node {
        [Input(ShowBackingValue.Never)] public object value;

        public override object GetValue(NodePort port) {
            return GetInputValue<object>("value", value);
        }
    }
}
