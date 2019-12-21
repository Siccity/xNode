using System;

namespace XNodeEditor {
    
    /// <summary>
    /// <para>When applied to the definition of a custom class, allows for the tooltip shown beside the nodes to be overriden.</para>
    /// Optionally, whether an output node's value is shown in the tooltip can also be turned off.
    /// Leaving the tooltip override blank or null will leave the normal tooltip (type name) in place.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class OverrideTooltipAttribute : Attribute {
        public readonly bool overrideTooltip;
        public readonly string tooltip;
        public readonly bool hideValue;
            
        public OverrideTooltipAttribute(string tooltip = "", bool hideValue = false) {
            overrideTooltip = !string.IsNullOrEmpty(tooltip);
            if (overrideTooltip) this.tooltip = tooltip;
            this.hideValue = hideValue;
        }
    }
    
}