using System;
using UnityEngine;

namespace XNode.Examples {
    /// <summary> Defines an example nodegraph that can be created as an asset in the Project window. </summary>
    [Serializable, CreateAssetMenu(fileName = "New Math Graph", menuName = "xNode Examples/Math Graph")]
    public class MathGraph : XNode.NodeGraph { }
}