using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace MultiClientServer
{
    class Connection
    {
        public StreamReader Read;
        public StreamWriter Write;
        public List<Thread> threads = new List<Thread>();
        public int ping, eigenadres, doeladres, favopoort;
        private bool stop;
        object o = new object();
        // Connection heeft 2 constructoren: deze constructor wordt gebruikt als wij CLIENT worden bij een andere SERVER
        public Connection(int port)
        {
            TcpClient client = new TcpClient("localhost", port);
            Read = new StreamReader(client.GetStream());
            Write = new StreamWriter(client.GetStream());
            Write.AutoFlush = true;

            // De server kan niet zien van welke poort wij client zijn, dit moeten we apart laten weten
            this.eigenadres = Program.MijnPoort;
            this.doeladres = port;
            this.favopoort = port;

            Write.WriteLine("Poort: " + eigenadres);
            // Start het reader-loopje
            Thread thread = new Thread(ReaderThread);
            threads.Add(thread);
            thread.Start();

        }

        // Deze constructor wordt gebruikt als wij SERVER zijn en een CLIENT maakt met ons verbinding
        public Connection(StreamReader read, StreamWriter write)
        {
            Read = read; Write = write;
            // Start het reader-loopje
            Thread thread = new Thread(ReaderThread);
            threads.Add(thread);
            thread.Start();         
        }

        // LET OP: Nadat er verbinding is gelegd, kun je vergeten wie er client/server is (en dat kun je aan het Connection-object dus ook niet zien!)

        // Deze loop leest wat er binnenkomt en print dit
        public void ReaderThread()
        {
            try
            {
                while (true)
                {
                    string message = Read.ReadLine();
                    Console.WriteLine(message);
                    if (message.StartsWith(eigenadres.ToString()))
                    {

                        message = message.Substring(message.IndexOf(" ") + 1);
                        if (message.StartsWith("ping ping"))
                        {
                            Program.Buren[Int32.Parse(message.Split()[2])].Write.WriteLine(message.Split()[2] + " ping pong");
                        }
                        else if (message.StartsWith("ping pong"))
                        {
                            lock (o)
                            {
                                stop = false;
                            }
                        }
                        else if (message.StartsWith("GetDictionary"))
                        {
                            senddictionary();
                        }
                        else if (message.StartsWith("Dictionary"))
                        {
                            UpdateDictionary(message.Substring(message.IndexOf(" ") + 1));
                        }
                    }
                    else
                    {
                        lock (o)
                        {
                            Program.Buren[Int32.Parse(message.Split()[0])].Write.WriteLine(message);
                        }
                    }
                }
            }
            catch { } // Verbinding is kennelijk verbroken
        }
        void UpdateDictionary(string input)
        {
            lock (o)
            {
                if (!Program.Buren.ContainsKey(Int32.Parse(input.Split()[0])) && Int32.Parse(input.Split()[0]) != eigenadres) //
                {
                    Connection connection = new Connection(this.Read, this.Write);
                    Program.Buren.Add(Int32.Parse(input.Split()[0]), connection);
                    Program.Connecties.Add(Int32.Parse(input.Split()[0]));
                    connection.Read = this.Read;
                    connection.Write = this.Write;
                    connection.favopoort = this.favopoort;
                    connection.ping = this.ping;
                }
            }

        }
        public void Ping(int poort)
        {
            stop = true;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Program.Buren[poort].Write.WriteLine(poort + " ping ping " + Program.MijnPoort);
            while (Stop())
            {
            }
            stopwatch.Stop();            
            ping = (int) stopwatch.ElapsedMilliseconds;
        }
        bool Stop()
        {
            lock (o)
            {
                return stop;
            }
        }
        public void print()
        {
            Console.WriteLine(Program.Connecties.ToString());
        }
        public void senddictionary()
        {
            for (int i = 0; i < Program.Connecties.Count; i++)
            {
                //if (Program.Connecties[i] != eigenadres)
                {
                    string output = doeladres + " Dictionary " + Program.Connecties[i] + " " + Program.Buren[Program.Connecties[i]].ping;
                    Console.WriteLine(output);
                    Program.Buren[doeladres].Write.WriteLine(output);
                }
            }
        }
    }
}
