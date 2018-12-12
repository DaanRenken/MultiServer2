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
                                Console.WriteLine("Poort onbekent!");
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
            for (int i = 0; i < Connecties.Count; i++)
            {
                Console.WriteLine(Connecties[i] + " " + Buren[Connecties[i]].ping + " " +Buren[Connecties[i]].favopoort);
            }
        }
        public  static void AddConnection(Connection connection)
        {
            int poort = connection.doeladres;
            if (Buren.ContainsKey(poort))
                Console.WriteLine("Hier is al verbinding naar!");
            else if (poort != MijnPoort)
            {
                // Leg verbinding aan (als client)

                Buren.Add(poort, connection);
                Connecties.Add(poort);
                //connection.eigenadres = MijnPoort;
                //connection.doeladres = poort;
                //connection.favopoort = poort;
                connection.Ping(poort);
                connection.SendDictionary();
                connection.SendDictionary(connection.GetNeigbours(), poort);
            }
        }
    }
}
