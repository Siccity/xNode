using System.Collections.Generic;
using UnityEngine;

namespace XNode
{
    [CreateNodeMenu("Group")]
    public class NodeGroup : Node
    {
        public int width = 400;
        public int height = 400;
        public Color color = new Color(1f, 1f, 1f, 0.1f);

        public override object GetValue(NodePort port)
        {
            return null;
        }

        /// <summary> Gets nodes in this group </summary>
        public List<Node> GetNodes()
        {
            var result = new List<Node>();
            foreach (Node node in graph.nodes)
            {
                if (node == this)
                {
                    continue;
                }

                if (node == null)
                {
                    continue;
                }

                if (node.position.x < position.x)
                {
                    continue;
                }

                if (node.position.y < position.y)
                {
                    continue;
                }

                if (node.position.x > position.x + width)
                {
                    continue;
                }

                if (node.position.y > position.y + height + 30)
                {
                    continue;
                }

                result.Add(node);
            }

            return result;
        }
    }
}