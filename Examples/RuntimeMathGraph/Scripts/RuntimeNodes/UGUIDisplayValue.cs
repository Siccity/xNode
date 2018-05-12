using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

namespace XNode.Examples.RuntimeMathNodes {
	public class UGUIDisplayValue : UGUIMathBaseNode {
		public Text label;

		void Update() {
			DisplayValue displayValue = node as DisplayValue;
			object obj = displayValue.GetInputValue<object>("input");
			if (obj != null) label.text = obj.ToString();
			else label.text = "n/a";
		}
	}
}