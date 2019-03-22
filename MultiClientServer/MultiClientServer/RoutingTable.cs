using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class RoutingTable
    {
        Dictionary<int, List<Node>> connections = new Dictionary<int, List<Node>>();

        public void AddConnection(int poort, Node node)
        {
            if (connections.ContainsKey(poort) && !connections[poort].Contains(node))
            {
                connections[poort].Add(node);
            }
            else
            {
                connections.Add(poort, new List<Node>());
                AddConnection(poort, node);
            }
        }

        public void RemoveConnection(int poort, Node node)
        {
            if (connections.ContainsKey(poort) && connections[poort].Contains(node))
            {
                connections[poort].Remove(node);
                if (connections[poort].Count == 0)
                {
                    connections.Remove(poort);
                }
            }
            else
            {

            }
        }

        public Node GetNode(int poort)
        {
            if (connections.ContainsKey(poort))
            {
                connections[poort].Sort();
                return connections[poort][0];
            }
            else
            {
                return null;
            }
        }

    }
}
