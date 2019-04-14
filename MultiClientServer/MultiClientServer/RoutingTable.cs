using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class RoutingTable
    {
        Dictionary<int, List<Node>> connections = new Dictionary<int, List<Node>>();
        Dictionary<int, object> locks = new Dictionary<int, object>();

        public RoutingTable(int poort, Node node)
        {
            AddConnection(poort, node);
        }

        public void AddConnection(int poort, Node node)
        // Hier moet nog een case toegevoegd worden als verbinding wordt gezocht met een poort die (nog) niet bestaat
        {
            if (connections.ContainsKey(poort) && !connections[poort].Contains(node))
            {
                lock (locks[poort])
                {
                    if (node.ReturnDistance() == 1)
                    {
                        Console.WriteLine("Creating connection within node");
                        node.CreateConnection(poort);
                    }
                    connections[poort].Add(node);
                }
            }
            else
            {
                object locked = new object();
                locks.Add(poort, locked);
                lock (locks[poort])
                {
                    connections.Add(poort, new List<Node>());
                    AddConnection(poort, node);
                }
            }
        }

        public void AcceptConnection(int poort, Node node, Connection connection)
        {
            if (connections.ContainsKey(poort) && !connections[poort].Contains(node))
            {
                lock (locks[poort])
                {
                    if (node.ReturnDistance() == 1)
                    {
                        node.AcceptConnection(connection);
                    }
                    connections[poort].Add(node);
                }
            }
            else
            {
                object locked = new object();
                locks.Add(poort, locked);
                lock (locks[poort])
                {
                    connections.Add(poort, new List<Node>());
                    AddConnection(poort, node);
                }
            }
        }

        public void RemoveConnection(int poort, Node node)
        {
            if (connections.ContainsKey(poort) && connections[poort].Contains(node))
            {
                bool removedconnection = false;
                lock (locks[poort])
                {
                    connections[poort].Remove(node);
                    if (connections[poort].Count == 0)
                    {
                        connections.Remove(poort);
                        removedconnection = true;
                    }
                }
                if (removedconnection)
                {
                    locks.Remove(poort);
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
                    bestConnection.WriteMessage(input);
                }
            }
        }
    }
}
