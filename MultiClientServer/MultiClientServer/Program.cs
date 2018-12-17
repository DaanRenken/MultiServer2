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
            Console.Title = MijnPoort.ToString();

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
                            if (Buren.ContainsKey(newPoort))
                                Console.WriteLine("Hier is al verbinding naar! (cmdInput)");
                            else
                            {
                                // Leg verbinding aan (als client)
                                Connection connection = new Connection(newPoort);
                                Buren.Add(newPoort, connection);
                                Connecties.Add(newPoort);
                            }
                            connectSucceed = true;
                            Console.WriteLine("Verbinding succesvol afgehandeld.");
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
                        case "R":
                            {
                                Print();
                                break;
                            }
                        case "B":
                            {
                                input = input.Substring(input.IndexOf(" ") + 1);
                                Buren[Int32.Parse(input.Split()[0])].SendMessage(input.Substring(input.IndexOf(" ") + 1));
                                break;
                            }
                        case "C":
                            {
                                int poort = int.Parse(input.Split()[1]);
                                Connection connection = new Connection(poort);
                                AddConnection(connection);
                                break;
                            }
                        case "D":
                            {
                                int poort = int.Parse(input.Split()[1]);
                                Buren[poort].SendMessage("Remove Connection " + MijnPoort);
                                RemoveConnection(poort);
                                break;
                            }
                    }
                    {
                        if (input.StartsWith("ping"))
                        {
                            int poort = int.Parse(input.Split()[1]);
                            if (Buren.ContainsKey(poort))
                            {
                                Buren[poort].Ping(poort);
                            }
                            else
                            {
                                //Console.WriteLine("Poort onbekent!");
                            }
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("foute input probeer het nog eens");
                }
            }

        }
        static void Print()
        {
            Console.WriteLine(MijnPoort + " 0 Local");
            Connecties.Sort();
            foreach (int i in Connecties)
            {
                Console.WriteLine(i + " " + Buren[i].ping + " " + Buren[i].favopoort);
            }
        }
        public static void AddConnection(Connection connection)
        {
            lock (o)
            {
                int poort = connection.doeladres;
                if (Buren.ContainsKey(poort))
                {
                    if (Buren[poort].doeladres != Buren[poort].favopoort && connection.doeladres == connection.favopoort)
                    {
                        Buren[poort] = connection;
                        Console.WriteLine("Verbonden: " + poort);
                    }
                    //Console.WriteLine("Hier is al verbinding naar!");
                }
                else if (poort != MijnPoort)
                {
                    // Leg verbinding aan (als client)
                    Buren.Add(poort, connection);
                    Connecties.Add(poort);
                    //connection.Ping(poort);
                    connection.SendDictionary();
                    connection.SendDictionary(GetNeigbours(), poort);
                    if (connection.doeladres == connection.favopoort)
                    {
                        Console.WriteLine("Verbonden: " + poort);
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
                    try
                    {
                        connection.SendMessage("Removed Connection " + MijnPoort);
                    }
                    catch { }
                    //Console.WriteLine("Remove Connection " + MijnPoort + "DEBUG");
                    Connecties.Remove(connection.doeladres);
                    Buren.Remove(connection.doeladres);
                    try
                    {
                        List<Connection> neigbours = GetNeigbours();
                        foreach (Connection i in neigbours)
                        {
                            i.SendMessage("Removed Connection " + port + " " + MijnPoort);
                            //Console.WriteLine("Removed Connection " + port + " " + MijnPoort + "DEBUG");
                        }
                        if (connection.doeladres == connection.favopoort)
                        {
                            Console.WriteLine("Verbroken: " + port);
                        }
                        else
                        {
                            Console.WriteLine("Onbereikbaar: " + port);
                        }
                    }
                    catch { }
                }
            }
        }
        public static List<Connection> GetNeigbours()
        {
            List<Connection> neigbours = new List<Connection>();
            foreach (int i in Connecties)
            {
                Connection connectie = Program.Buren[i];
                if (connectie.doeladres == connectie.favopoort)
                {
                    neigbours.Add(connectie);
                }
            }
            return neigbours;
        }
        public static List<int> GetVirtualPorts(int poort)
        {
            List<int> output = new List<int>();
            foreach (int i in Connecties)
            {
                if (Buren[i].favopoort == poort)
                {
                    output.Add(i);
                }
            }
            if (!output.Contains(poort))
            {
                output.Add(poort);
            }
            return output;
        }
    }
}
