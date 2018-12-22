using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class Program
    {
        static public int MijnPoort;

        static public Dictionary<int, Connection> Buren = new Dictionary<int, Connection>();
        static public List<int> Connecties = new List<int>();
        static object o = new object();

        // startup: als programma vanuit cmd wordt geopend, worden er meteen poorten aangemaakt en connecties gelegd
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
            // TEST
            o = MijnPoort;

            new Server(MijnPoort);
            Console.Title = "NetChange " + MijnPoort.ToString();

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
                            Connection connection = new Connection(newPoort);
                            AddConnection(connection);
                            connectSucceed = true;
                            Console.WriteLine("Verbonden: " + newPoort);
                        }
                        catch { Exception e; }
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

                                int poort = Int32.Parse(input.Split()[0]);
                                if (Connecties.Contains(poort))
                                {
                                    if (Connecties.Contains(poort))
                                    {
                                        Buren[poort].SendMessage(input.Substring(input.IndexOf(" ") + 1));
                                    }
                                    else
                                    {
                                        Console.WriteLine("Poort " + poort + " is niet bekend");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Poort " + poort + " is niet bekend");
                                }
                                break;
                            }
                        // C: connect met poort
                        case "C":
                            {
                                int poort = int.Parse(input.Split()[1]);
                                Connection connection = new Connection(poort);
                                AddConnection(connection);
                                break;
                            }
                        // D: destroy verbinding met poort
                        case "D":
                            {
                                int poort = int.Parse(input.Split()[1]);
                                if (Connecties.Contains(poort))
                                {
                                    if (Connecties.Contains(poort))
                                    {
                                        Buren[poort].SendMessage("Remove Connection " + MijnPoort);
                                        RemoveConnection(poort);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Poort " + poort + " is niet bekend");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Poort " + poort + " is niet bekend");
                                }
                                break;
                            }
                    }
                }
                catch
                { }
            }

        }

        // Print dictionary
        static void Print()
        {
            Console.WriteLine(MijnPoort + " 0 Local");
            Connecties.Sort();
            foreach (int i in Connecties)
            {
                Console.WriteLine(i + " " + Buren[i].ping + " " + Buren[i].favopoort);
            }
        }

        // Voeg andere poort in connection toe aan buren en update dictionary naar neighbors
        public static void AddConnection(Connection connection)
        {
            lock (o)
            {
                int poort = connection.doeladres;
                if (Buren.ContainsKey(poort))
                {
                    if (Buren[poort].doeladres != Buren[poort].favopoort && connection.doeladres == connection.favopoort)
                    {   Buren[poort] = connection;
                        Console.WriteLine("Verbonden: " + poort);
                    }
                }
                else if (poort != MijnPoort)
                {
                    // Leg verbinding aan (als client)
                    Buren.Add(poort, connection);
                    Connecties.Add(poort);
                }
                connection.SendDictionary();
                connection.SendDictionary(GetNeigbours(), poort);
            }
        }
        public static void UpdateConnection(int poort, int favopoort, int ping)
        {
            Console.WriteLine("Afstand naar " + poort + " is nu " + ping + " via " + favopoort);
            Connection connection = Buren[poort];
            connection.favopoort = favopoort;
            connection.ping = ping;
            connection.SendDictionary();
            connection.SendDictionary(GetNeigbours(), poort);
        }

        // verwijdert een hele poort (?) uit het systeem en update aan alle betreffenden (buren eigen poort + buren verwijderde poort) nieuwe dictionary
        public static void RemoveConnection2(int poort)
        {
            lock (o)
            {
                List<int> lijst = GetVirtualPorts(poort);
                foreach (int port in lijst)
                {
                    Connection connection = Buren[port];
                    Connecties.Remove(port);
                    Buren.Remove(port);
                    if (connection.doeladres == connection.favopoort)
                    {
                        Console.WriteLine("Verbroken: " + port);
                    }
                    else
                    {
                        Console.WriteLine("Onbereikbaar: " + port);
                    }
                }

            }
        }

        public static void RemoveConnection(int poort)
        {
            lock (o)
            {
                List<int> lijst = GetVirtualPorts(poort);
                foreach (int port in lijst)
                {
                    Connection connection = Buren[port];
                    Connecties.Remove(port);
                    Buren.Remove(port);
                    try
                    {
                        List<Connection> neigbours = GetNeigbours();
                        foreach (Connection i in neigbours)
                        {
                            i.SendMessage("Removed Connection " + port + " " + MijnPoort);
                        }
                    }
                    catch { }
                    if (connection.doeladres == connection.favopoort)
                    {
                        Console.WriteLine("Verbroken: " + port);
                    }
                    else
                    {
                        Console.WriteLine("Onbereikbaar: " + port);
                    }

                }
            }
        }
        public static List<Connection> GetNeigbours()
        {
            List<Connection> neigbours = new List<Connection>();
            foreach (int i in Connecties)
            {
                Connection connectie = Program.Buren[i];
                //if (connectie.doeladres == connectie.favopoort)
                {
                    neigbours.Add(connectie);
                }
            }
            return neigbours;
        }
        public static List<int> GetVirtualPorts(int poort)
        {
            List<int> output = new List<int>();
            output.Add(poort);
            foreach (int i in Connecties)
            {
                if (Buren[i].favopoort == poort && !output.Contains(i))
                {
                    output.Add(i);
                }
            }

            return output;
        }
    }
}
