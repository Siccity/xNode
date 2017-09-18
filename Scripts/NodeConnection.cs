using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UNEC {
    /// <summary> Data travels from Input Node's Output port to Output Node's Input port </summary>
    public struct NodeConnection {
        public int inputNodeId { get { return _inputNodeId; } }
        public int inputPortId { get { return _inputPortId; } }
        public int outputNodeId { get { return _outputNodeId; } }
        public int outputPortId { get { return _outputPortId; } }
        [SerializeField] private int _inputNodeId, _inputPortId, _outputNodeId, _outputPortId;

        /// <summary> Data travels from Input Node's Output port to Output Node's Input port </summary>
        public NodeConnection(int inputNodeId, int outputPortId, int outputNodeId, int inputPortId) {
            _inputNodeId = inputNodeId;
            _outputPortId = outputPortId;
            _outputNodeId = outputNodeId;
            _inputPortId = inputPortId;
        }
    }
}