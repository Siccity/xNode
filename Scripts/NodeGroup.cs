using System.Collections.Generic;

namespace XNode
{
    [NodeColorHeader(0.1f, 0.1f, 0.1f, 0.35f)]
    [NodeColorBody(0.1f, 0.1f, 0.1f, 0.35f)]
    [CreateNodeMenu("Group")]
    public class NodeGroup : Node
    {
        public int width = 400;
        public int height = 400;

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

                // Number at the end must match the fixedHeight for the group's header style.
                if (node.position.y > position.y + height + 46)
                {
                    continue;
                }

                result.Add(node);
            }

            return result;
        }
    }
}