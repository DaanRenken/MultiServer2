using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class RoutingTable
    {
        Dictionary<int, List<Node>> connections = new Dictionary<int, List<Node>>();
        int eigenpoort;

        public RoutingTable(int poort, Node node)
        {
            AddConnection(poort, node);
            eigenpoort = poort;
        }

        public void AddConnection(int poort, Node node)
        // Hier moet nog een case toegevoegd worden als verbinding wordt gezocht met een poort die (nog) niet bestaat
        {
            // als er al een (indirecte) verbinding is naar de poort, wordt er gekeken om wat voor soort node de toevoeging gaat
            if (connections.ContainsKey(poort))
            {
                if (!connections[poort].Exists(x => x.ReturnNeighbor() == node.ReturnNeighbor()))
                {
                    // als de verbinding via een nieuwe neighbor gaat en een distance van 1 heeft, wordt er een directe verbinding gelegd
                    if (node.ReturnDistance() == 1)
                    {
                        Console.WriteLine("Creating connection within node");
                        node.CreateConnection(poort);
                    }
                    // bij een langere distance wordt er een normale, indirecte verbinding gelegd
                    connections[poort].Add(node);
                    UpdateNeighbors(poort, node);
                }
                // als er al een verbinding bestaat met dezelfde preferred neighbor, wordt er gekeken of dit een verbetering is
                else
                {
                    foreach (var ele in connections[poort])
                    {
                        if (node.ReturnNeighbor() == ele.ReturnNeighbor() && node.ReturnDistance() < ele.ReturnDistance())
                        {
                            // zo ja, dan wordt de oude verbinding geupdated
                            ele.Update(node.ReturnDistance());
                            UpdateNeighbors(poort, node);
                        }
                    }
                }
            }
            // mocht er nog helemaal geen verbinding bestaan, dan wordt er een nieuwe lijst gemaakt en wordt deze toegevoegd
            else
            {
                connections.Add(poort, new List<Node>());
                AddConnection(poort, node);
            }
        }

        public void AcceptConnection(int poort, Node node, Connection connection)
        {
            if (connections.ContainsKey(poort) && !connections[poort].Contains(node))
            {
                if (node.ReturnDistance() == 1)
                {
                    node.AcceptConnection(connection);
                }
                connections[poort].Add(node);
                node.WriteMessage(poort + " SendAll " + eigenpoort);
            }
            else
            {
                connections.Add(poort, new List<Node>());
                //AddConnection(poort, node);
                AcceptConnection(poort, node, connection);
                UpdateNeighbors(poort, node);
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
                // als de distance 0 is (en dus voor jezelf bedoeld is), dan wordt het bericht gewoon geprint.
                // deze is eigenlijk overbodig, aangezien connection.cs hier al voor zorgt
                if (bestConnection.ReturnDistance() == 0)
                {
                    Console.WriteLine(input);
                }
                // als de distance 1 is, dan wordt het bericht direct doorgestuurd aan de bestemde neighbor
                else if (bestConnection.ReturnDistance() == 1)
                {
                    bestConnection.WriteMessage(input);
                }
                // als de distance groter dan 1 is, wordt het bericht doorgestuurd aan de preferred neighbor
                else
                {
                    input = bestConnection.ReturnNeighbor().ToString() + " " + input;
                    SendMessage(bestConnection.ReturnNeighbor(), input);
                }
            }
        }

        // stuurt de node door naar alle directe neighbors, behalve degene die net is toegevoegd
        public void UpdateNeighbors(int poort, Node node)
        {
            Dictionary<int, List<Node>>.KeyCollection keyColl = connections.Keys;
            foreach (int neighbor in keyColl)
            {
                if (neighbor != poort)
                {
                    Node directNeighbor = GetNode(neighbor);
                    if (directNeighbor.ReturnDistance() == 1)
                    {
                        directNeighbor.WriteMessage(neighbor + " NewNode " + eigenpoort + " " + node.ReturnPoort() + " " + node.ReturnDistance() + " " + node.ReturnNeighbor());
                    }
                }
            }
        }

        // stuurt de hele routing table die op dat moment bestaat naar de andere poort
        public void SendAll(int poort)
        {
            Node bestConnection = GetNode(poort);
            Dictionary<int, List<Node>>.KeyCollection keyColl = connections.Keys;
            foreach (int neighbor in keyColl)
            {
                foreach (var ele in connections[neighbor])
                {
                    bestConnection.WriteMessage(poort + " NewNode " + eigenpoort + " " + bestConnection.ReturnPoort() + " " + bestConnection.ReturnDistance() + " " + bestConnection.ReturnNeighbor());
                }
            }
        }
    }
}
