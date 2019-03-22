using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MultiClientServer
{
    class Program
    {
        static public int MijnPoort;

        static public List<int> Connecties = new List<int>();
        static RoutingTable routingtable = new RoutingTable();

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
                            //AddConnection(newPoort, connection);

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

                                int poort = Int32.Parse(input.Split()[0]);
                                if (Connecties.Contains(poort))
                                {
                                    if (Connecties.Contains(poort))
                                    {
                                        //Buren[poort].SendMessage(input.Substring(input.IndexOf(" ") + 1));
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
                                //AddConnection(poort, connection);
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
                                        //Buren[poort].SendMessage("Remove Connection " + MijnPoort);
                                        //RemoveConnection(poort);
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

        static void Print()
        {
            Console.WriteLine(MijnPoort + " 0 Local");
            Connecties.Sort();
            foreach (int i in Connecties)
            {
                Console.WriteLine(i);// + " " + Buren[i].ping + " " + Buren[i].favopoort);
            }
        }
    }
}

