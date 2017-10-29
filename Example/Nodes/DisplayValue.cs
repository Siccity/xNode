namespace BasicNodes {
    public class DisplayValue : Node {
        [Input(ShowBackingValue.Never)] public object value;

        public override object GetValue(NodePort port) {
            return GetInputByFieldName<object>("value", value);
        }
    }
}
