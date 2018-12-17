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
        static void Main(string[] args)
        {
            Console.Write("Op welke poort ben ik server? ");
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
            Console.WriteLine("Typ [verbind poortnummer] om verbinding te maken, bijvoorbeeld: verbind 1100");
            Console.WriteLine("Typ [poortnummer bericht] om een bericht te sturen, bijvoorbeeld: 1100 hoi hoi");

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
                //try
                {
                    if (input.StartsWith("verbind"))
                    {
                        int poort = int.Parse(input.Split()[1]);
                        if (Buren.ContainsKey(poort))
                            Console.WriteLine("Hier is al verbinding naar! (manualInput)");
                        else
                        {
                            // Leg verbinding aan (als client)
                            Connection connection = new Connection(poort);
                            Buren.Add(poort, connection);
                            Connecties.Add(poort);
                            connection.ping = connection.Ping(poort);
                            
                        }
                    }
                    else if (input.StartsWith("ping"))
                    {
                        int poort = int.Parse(input.Split()[1]);
                        if (Buren.ContainsKey(poort))
                        {
                            Console.WriteLine((Buren[poort].Ping(poort)));
                        }
                        else
                        {
                            Console.WriteLine("Hier is al verbinding naar! (pingpong)");
                        }
                    }
                    else if (input.StartsWith("R"))
                    {
                        print();
                    }
                    else
                    {
                        // Stuur berichtje
                        string[] delen = input.Split(new char[] { ' ' }, 2);
                        int poort = int.Parse(delen[0]);
                        if (!Buren.ContainsKey(poort))
                        {
                            Console.WriteLine("Hier is al verbinding naar! (Message)");

                        }
                        else
                        {
                            Buren[poort].Write.WriteLine(MijnPoort + ": " + delen[1]);
                        }
                    }
                }
                //catch
                //{
                //    Console.WriteLine("foute input probeer het nog eens");
                //}
            }
            
        }
        static void print()
        {
            for (int i = 0; i < Connecties.Count; i++)
            {
               Console.WriteLine(Connecties[i] + " " + Buren[Connecties[i]].ping);
            }
        }
    }
}
