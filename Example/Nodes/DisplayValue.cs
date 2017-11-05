using XNode;

namespace BasicNodes {
    public class DisplayValue : Node {
        protected override void Init() {
            if (!HasPort("input")) AddInstanceInput(typeof(object), "input");
        }

        public override object GetValue(NodePort port) {
            return GetInputValue<object>("input");
        }
    }
}
