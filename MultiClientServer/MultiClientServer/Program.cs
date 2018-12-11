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
            //MijnPoort = int.Parse(args[0]);
            MijnPoort = int.Parse(Console.ReadLine());
            new Server(MijnPoort);
            Console.Title = MijnPoort.ToString();

            /*
            for (int i = 1; i < args.Length; i++)
            {
                int newPoort = int.Parse(args[i]);
                bool connectSucceed = false;
                while (!connectSucceed)
                {
                    System.Threading.Thread.Sleep(1000);
                    try
                    {
                        if (Buren.ContainsKey(newPoort))
                            Console.WriteLine("Hier is al verbinding naar!");
                        else
                        {
                            // Leg verbinding aan (als client)
                            Buren.Add(newPoort, new Connection(newPoort));
                        }
                        connectSucceed = true;
                    }
                    catch { Exception e; }
                }
            }
            */
            
            while (true)
            {
                string input = Console.ReadLine();
                //try
                {
                    if (input.StartsWith("verbind"))
                    {
                        int poort = int.Parse(input.Split()[1]);
                        if (Buren.ContainsKey(poort))
                            Console.WriteLine("Hier is al verbinding naar!");
                        else
                        {
                            // Leg verbinding aan (als client)
                            Connection connection = new Connection(poort);
                            Buren.Add(poort, connection);
                            Connecties.Add(poort);
                            connection.eigenadres = MijnPoort;
                            connection.doeladres = poort;
                            connection.Ping(poort);
                            connection.favopoort = poort;
                        }
                    }
                    else if (input.StartsWith("ping"))
                    {
                        int poort = int.Parse(input.Split()[1]);
                        if (Buren.ContainsKey(poort))
                        {
                            Buren[poort].Ping(poort);
                        }
                        else
                        {
                            Console.WriteLine("Poort onbekent!");
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
                            Console.WriteLine("Poort onbekent!");

                        }
                        else
                        {
                            Buren[poort].Write.WriteLine(input);
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
                Console.WriteLine(Connecties[i] + " " + Buren[Connecties[i]].ping + " print");
            }
        }
    }
}
