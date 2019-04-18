using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class Program
    {
        static public int MijnPoort;

        static RoutingTable routingtable;

        static void Main(string[] args)
        {
            bool cmdInput = (args.Length != 0);
            if (cmdInput)
            {
                MijnPoort = int.Parse(args[0]);
            }
            else
            {
                MijnPoort = int.Parse(Console.ReadLine());
            }

            new Server(MijnPoort);
            Console.Title = "NetChange " + MijnPoort.ToString();
            routingtable = new RoutingTable(MijnPoort, new Node(MijnPoort, 0, MijnPoort));
            if (cmdInput)
            {
                for (int i = 1; i < args.Length; i++)
                {
                    int newPoort = int.Parse(args[i]);
                    bool connectSucceed = false;
                    while (!connectSucceed)
                    {
                        System.Threading.Thread.Sleep(50);
                        try
                        {
                            Node newNode = new Node(newPoort, 1, newPoort);
                            AddConnection(newPoort, newNode);
                            connectSucceed = true;
                            Console.WriteLine("Verbonden: " + newPoort);
                        }
                        catch { }
                    }
                }
            }

            while (true)
            {
                string input = Console.ReadLine();
                try
                {
                    switch (input.Split()[0])
                    {
                        // R: reveal dictionary
                        case "R":
                            {
                                Print();
                                break;
                            }
                        // B: bericht naar poort
                        case "B":
                            {
                                input = input.Substring(input.IndexOf(" ") + 1);
                                SendMessage(input);
                                break;
                            }
                        // C: connect met poort
                        case "C":
                            {
                                int poort = int.Parse(input.Split()[1]);
                                Console.WriteLine("Adding connection to {0} in routing table", poort);
                                Node newNode = new Node(poort, 1, poort);
                                AddConnection(poort, newNode);
                                break;
                            }
                        // D: destroy verbinding met poort
                        case "D":
                            {
                                int poort = int.Parse(input.Split()[1]);
                                if (routingtable.containskey(poort))
                                {
                                    Node destroyNode = new Node(poort, 1, poort);
                                    routingtable.RemoveConnection(poort, destroyNode);
                                }
                                else
                                {
                                    Console.WriteLine("Poort " + poort + " is niet bekend");
                                }
                                break;
                            }
                    }
                }
                catch(Exception e)
                { Console.WriteLine(e); }
            }
        }

        static void Print()
        {
            int[] connecties = routingtable.GetConnections();
            foreach (int i in connecties)
            {
                Node tempnode = routingtable.GetNode(i);
                Console.WriteLine(tempnode.ReturnPoort() + " " + tempnode.ReturnDistance() + " " + tempnode.ReturnNeighbor());
            }
        }

        public static void SendMessage(String message)
        {
            int poort = Int32.Parse(message.Split()[0]);
            if (routingtable.containskey(poort))
            {
                routingtable.SendMessage(poort, message);
            }
            else
            {
                Console.WriteLine("Poort " + poort + " is niet bekend");
            }
        }

        public static void SendMessage(String message, bool doorgestuurd)
        {
            int poort = Int32.Parse(message.Split()[0]);
            Console.WriteLine("Bericht voor " + poort + " doorgestuurd naar " + routingtable.GetNode(poort).ReturnNeighbor());
            SendMessage(message);
        }

        public static void AddConnection(int newPoort, Node node)
        {
            routingtable.AddConnection(newPoort, node);
        }

        public static void AcceptConnection(int newPoort, Connection connection)
        {
            Node newNode = new Node(newPoort, 1, newPoort);
            routingtable.AcceptConnection(newPoort, newNode, connection);
        }

        public static void RemoveConnection(int poort)
        {
            routingtable.RemoveConnection(poort);
        }

        public static void SendAll(int poort)
        {
            routingtable.SendAll(poort);
        }
    }
}

