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
                lock (locks[poort])
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
                                Console.WriteLine("Afstand naar " + node.ReturnPoort() + " is nu " + node.ReturnDistance() + " via " + node.ReturnNeighbor());
                            }
                        }
                    }
                }
            }
            // mocht er nog helemaal geen verbinding bestaan, dan wordt er een nieuwe lijst gemaakt en wordt deze toegevoegd
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
            // bij een accepted connection wordt gekeken of er al een verbinding is naar de andere poort
            if (connections.ContainsKey(poort))
            {
                // zo ja, dan wordt er gekeken of deze directe verbinding al bestaat
                if (!connections[poort].Contains(node))
                {
                    lock (locks[poort])
                    {
                        // als dat niet het geval is, wordt de verbinding aangelegd en worden dictionaries uitgewisseld.
                        node.AcceptConnection(connection);
                        connections[poort].Add(node);
                        UpdateNeighbors(poort, node);
                        node.WriteMessage(poort + " SendAll " + eigenpoort);
                        node.WriteMessage(eigenpoort + " SendAll " + poort);
                    }
                }
            }
            else
            {
                // mocht er nog helemaal geen verbinding zijn, dan wordt de poort toegevoegd aan de routing table en begint de accept-loop opnieuw
                object locked = new object();
                locks.Add(poort, locked);
                lock (locks[poort])
                {
                    connections.Add(poort, new List<Node>());
                    AcceptConnection(poort, node, connection);
                }
            }
        }
        public void RemoveConnection(int poort)
        {
            Node node = GetNode(poort);
            RemoveConnection(poort, node);
        }

        public void RemoveConnection(int poort, Node node)
        {
            if (connections[poort].Exists(x => x.ReturnNeighbor() == node.ReturnNeighbor()))
            {
                Dictionary<int, List<Node>>.KeyCollection keyColl = connections.Keys;
                Node temp = connections[poort].Find(x => x.ReturnNeighbor() == node.ReturnNeighbor());
                bool removedconnection = false;
                lock (locks[poort])
                {
                    if (temp.ReturnDistance() == 1)
                    {
                        temp.WriteMessage("RemoveNode " + eigenpoort);
                    }
                    connections[poort].Remove(temp);
                    if (connections[poort].Count == 0)
                    {
                        connections.Remove(poort);
                        removedconnection = true;
                    }
                }
                if (removedconnection)
                {
                    locks.Remove(poort);
                    foreach (int neighbor in keyColl)
                    {
                        if (neighbor != eigenpoort)
                            SendMessage(neighbor, "RemoveNode " + poort);
                    }
                }
                List<int> remove = new List<int>();
                foreach (int neighbor in keyColl)
                {
                    removedconnection = false;
                    for (int i = 0; i < connections[neighbor].Count; i++)
                    {  
                        lock (locks[neighbor])
                        {
                            Node ele = connections[neighbor][i];
                            //Console.WriteLine(ele.ReturnNeighbor() + " " + ele.ReturnPoort() + " " + node.ReturnPoort());
                            if (ele.ReturnNeighbor() == node.ReturnPoort() && ele.ReturnPoort() != eigenpoort)
                            {
                                connections[neighbor].Remove(ele);
                                if (connections[neighbor].Count == 0)
                                {
                                    remove.Add(neighbor);
                                }
                            }
                        }
                        //Console.WriteLine("done");
                    }
                }

                foreach(int key in remove)
                {
                    lock (locks[key])
                    {
                        connections.Remove(key);
                    }                    
                    foreach (int neighbor in keyColl)
                    {
                        if (neighbor != eigenpoort)
                            SendMessage(neighbor, "RemoveNode " + key);
                    }
                    locks.Remove(key);
                }
                foreach(int neighbor in keyColl)
                {
                    if (GetNode(neighbor).ReturnDistance() >= keyColl.Count)
                    {
                        RemoveConnection(neighbor);
                    }
                }
            }
            else
            {
                Console.WriteLine("Error: no connection to port {0}", poort);
            }
        }

        public Node GetNode(int poort)
        {
            if (connections.ContainsKey(poort))
            {
                List<Node> orderedNodes = connections[poort].OrderBy(x => x.ReturnDistance()).ToList();
                return orderedNodes[0];
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
            try
            {
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
            catch { }
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
                    bestConnection.WriteMessage(poort + " NewNode " + eigenpoort + " " + ele.ReturnPoort() + " " + ele.ReturnDistance() + " " + ele.ReturnNeighbor());
                }
            }
        }
    }
}

