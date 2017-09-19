using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNEC {
    /// <summary> Data travels from Input Node's Output port to Output Node's Input port </summary>
    public struct NodeConnection {
        public bool enabled;
        /// <summary> Data travels from Input Node's Output port to Output Node's Input port </summary>
        public int inputNodeId, inputPortId, outputNodeId, outputPortId;

        /// <summary> Data travels from Input Node's Output port to Output Node's Input port </summary>
        public NodeConnection(int inputNodeId, int outputPortId, int outputNodeId, int inputPortId) {
            this.inputNodeId = inputNodeId;
            this.outputPortId = outputPortId;
            this.outputNodeId = outputNodeId;
            this.inputPortId = inputPortId;
            enabled = true;
        }
    }
}