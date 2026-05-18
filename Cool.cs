using Il2Cpp;
using System.Collections.Generic;

namespace MSZDialougeMap
{
    public static class Cool
    {
        public static List<DialogueNode> GetAllNodes(this DialogueTree tree)
        {
            List<DialogueNode> visited = new List<DialogueNode>();
            foreach (DialogueNode firstNode in tree.startNodes)
                TraverseNode(firstNode, visited);
            return visited;
        }

        public static List<DialogueNode> TraverseNode(DialogueNode node, List<DialogueNode> visited)
        {
            if (node == null || visited.Contains(node)) return visited;
            visited.Add(node);
            if (node.nextNodes != null)
                foreach (DialogueNode next in node.nextNodes)
                    TraverseNode(next, visited);
            return visited;
        }
    }
}
