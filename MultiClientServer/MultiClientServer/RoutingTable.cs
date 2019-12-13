using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class RoutingTable
    {
        Dictionary<int, List<Node>> connections = new Dictionary<int, List<Node>>();

        //Dictionary<int, object> locks = new Dictionary<int, object>();
        object locks = new object();
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
                //lock (locks)
                //{
                    if (!connections[poort].Exists(x => x.ReturnNeighbor() == node.ReturnNeighbor()))
                    {
                        // als de verbinding via een nieuwe neighbor gaat en een distance van 1 heeft, wordt er een directe verbinding gelegd
                        if (node.ReturnDistance() == 1)
                        {
                            lock (locks)
                            {
                                Console.WriteLine("Creating connection within node");
                                node.CreateConnection(poort);
                            }
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
                            lock (locks)
                            {
                                ele.Update(node.ReturnDistance());
                            }
                            UpdateNeighbors(poort, node);
                            Console.WriteLine("Afstand naar " + node.ReturnPoort() + " is nu " + node.ReturnDistance() + " via " + node.ReturnNeighbor());
                            }
                        }
                    }
                //}
            }
            // mocht er nog helemaal geen verbinding bestaan, dan wordt er een nieuwe lijst gemaakt en wordt deze toegevoegd
            else
            {
                object locked = new object();
                //locks.Add(poort, locked);
                //lock (locks)
                //{
                    connections.Add(poort, new List<Node>());
                    AddConnection(poort, node);
                //}
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
                    lock (locks)
                    {
                        connections[poort].Add(node);
                        node.AcceptConnection(connection);
                    }
                    // als dat niet het geval is, wordt de verbinding aangelegd en worden dictionaries uitgewisseld.

                    UpdateNeighbors(poort, node);
                    node.WriteMessage(poort + " SendAll " + eigenpoort);
                    node.WriteMessage(eigenpoort + " SendAll " + poort);                    
                }
            }
            else
            {
                // mocht er nog helemaal geen verbinding zijn, dan wordt de poort toegevoegd aan de routing table en begint de accept-loop opnieuw
                object locked = new object();
                //locks.Add(poort, locked);
                //lock (locks)
                //{
                    connections.Add(poort, new List<Node>());
                //}
                AcceptConnection(poort, node, connection);
            }
        }

        public void RemoveConnection(int poort, Node node)
        {
            int[] keyColl = GetConnections();
            //Dictionary<int, List<Node>>.KeyCollection keyColl = connections.Keys;

            Node temp = new Node(0,0,0);
            foreach (int key in keyColl)
            {
                foreach(Node x in connections[key])
                {
                    if (x.ReturnNeighbor() == node.ReturnNeighbor() && x.ReturnPoort() == node.ReturnPoort())
                    {
                        temp = x;
                    }
                }
            }
            lock (locks)
            {
                if (temp.ReturnDistance() == 1)
                {
                    temp.WriteMessage("RemoveNode " + eigenpoort);
                }
                connections[poort].Remove(temp);
                if (connections[poort].Count == 0)
                {
                    connections.Remove(poort);
                    foreach (int key in keyColl)
                    {
                        temp = GetNode(key);
                        if (temp.ReturnDistance() == 1)
                        {
                            temp.WriteMessage("RemoveNode " + poort);
                        }
                    }
                }
            }
            List<Node> remove = new List<Node>();

            foreach (int neighbor in keyColl)
            {
                foreach (Node ele in connections[neighbor])
                {                   
                    if ((ele.ReturnDistance() >= keyColl.Length || ele.ReturnNeighbor() == poort) && ele.ReturnDistance() > 1 && GetNode(ele.ReturnNeighbor()).ReturnDistance() != 1)
                    {
                        remove.Add(ele);
                    }
                }
            }
            foreach(Node i in remove)
            {
                RemoveConnection(i.ReturnPoort(), i);
            }
        }

        public Node GetNode(int poort)
        {
            lock (locks)
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
            int[] connecties = GetConnections();
            foreach (int neighbor in connecties)
            {
                bool updateSucceed = false;
                while (!updateSucceed)
                {
                    try
                    {
                        if (neighbor != poort)
                        {
                            Node directNeighbor = GetNode(neighbor);
                            if (directNeighbor.ReturnDistance() == 1)
                            {
                                directNeighbor.WriteMessage(neighbor + " NewNode " + eigenpoort + " " + node.ReturnPoort() + " " + node.ReturnDistance() + " " + node.ReturnNeighbor());
                            }
                            updateSucceed = true;
                        }
                        else
                        {
                            updateSucceed = true;
                        }
                    }
                    catch
                    {
                        //System.Threading.Thread.Sleep(50);
                    }
                }
            }
        }

        // stuurt de hele routing table die op dat moment bestaat naar de andere poort
        public void SendAll(int poort)
        {
            Node bestConnection = GetNode(poort);
            int[] connecties = GetConnections();
            foreach (int neighbor in connecties)
            {
                foreach (var ele in connections[neighbor])
                {
                    bestConnection.WriteMessage(poort + " NewNode " + eigenpoort + " " + ele.ReturnPoort() + " " + ele.ReturnDistance() + " " + ele.ReturnNeighbor());
                }
            }
        }

        public void PrintAll()
        {
            int[] connecties = GetConnections();
            foreach (int i in connecties)
            {
                for (int j = 0; j < connections[i].Count(); j++)
                {
                    Node temp = connections[i][j];
                    Console.WriteLine(temp.ReturnPoort() + " " + temp.ReturnDistance() + " " + temp.ReturnNeighbor());
                }
            }
        }
    }
}

