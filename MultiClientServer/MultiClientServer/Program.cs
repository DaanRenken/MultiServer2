﻿using System;
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
                            //Connection connection = new Connection(newPoort);
                            //AddConnection(newPoort, connection);
                            routingtable.AddConnection(newPoort, new Node(newPoort, 1, newPoort));
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
                                if (routingtable.containskey(poort))
                                {
                                    routingtable.SendMessage(poort, input);
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
                                routingtable.AddConnection(poort, new Node(poort, 1, poort));
                                break;
                            }
                        // D: destroy verbinding met poort
                        case "D":
                            {
                                int poort = int.Parse(input.Split()[1]);
                                if (routingtable.containskey(poort))
                                {
                                    if (routingtable.containskey(poort))
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
            int[] connecties = routingtable.GetConnections();
            foreach (int i in connecties)
            {
                Node tempnode = routingtable.GetNode(i);
                Console.WriteLine(tempnode.ReturnPoort() + " " + tempnode.ReturnDistance() + " " + tempnode.ReturnNeighbor());
            }
        }

        public static void AddConnection(int newPoort) {
            routingtable.AddConnection(newPoort, new Node(newPoort, 0, newPoort));
        }
    }
}

