using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class RoutingTable
    {
        Dictionary<int, List<Node>> connections = new Dictionary<int, List<Node>>();

        public RoutingTable(int poort, Node node)
        {
            AddConnection(poort, node);
        }

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
                connections[poort].OrderBy(x => x.ReturnDistance()).ToList<Node>();
                return connections[poort][0];
            }
            else
            {
                return null;
            }
        }

        public int[] GetConnections()
        {
           return connections.Keys.ToArray();
        }

        public bool containskey(int poort)
        {
            return connections.ContainsKey(poort);
        }

        public void SendMessage(int poort, String input)
        {
            Node bestConnection = GetNode(poort);
            if (bestConnection == null)
            {
                Console.WriteLine("Error: no connection to port {0}", poort);
            }
            else
            {
                if (bestConnection.ReturnDistance() == 0)
                {
                    Console.WriteLine(input);
                }
                else
                {
                    Console.WriteLine("Hier moet nu een message verstuurd worden.");
                }
            }
        }
    }
}
