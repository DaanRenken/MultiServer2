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
            Write.WriteLine("Poort: " + Program.MijnPoort);
            
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
            //senddictionary();
            try
            {
                while (true)
                {
                    string message = Read.ReadLine();
                    Console.WriteLine(message);
                    if (message.StartsWith(eigenadres.ToString()))
                    {

                        message = message.Substring(message.IndexOf(" ")+1);
                        if (message.StartsWith("ping ping"))
                        {
                            Program.Buren[Int32.Parse(message.Split()[2])].Write.WriteLine(doeladres + " ping pong");
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
                        //Program.Buren[Int32.Parse(message.Split()[0])].Write.WriteLine(message);
                    }
                }
            }
            catch { } // Verbinding is kennelijk verbroken
        }
        void UpdateDictionary(string input)
        {
            if (!Program.Buren.ContainsKey(Int32.Parse(input.Split()[0])))
            {
                new Connection(Int32.Parse(input.Split()[0]));
                Program.Connecties.Add(Int32.Parse(input.Split()[0]));
            }
            Program.Buren.TryGetValue(Int32.Parse(input.Split()[0]), out Connection connection);
            if (connection.ping > this.ping)
            {
                connection.Read = this.Read;
                connection.Write = this.Write;
                connection.favopoort = this.favopoort;
            }
            

        }
        public int Ping(int poort)
        {
            stop = true;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Program.Buren[poort].Write.WriteLine(doeladres+ " ping ping " + Program.MijnPoort);
            while (Stop())
            {
            }
            stopwatch.Stop();
            return (int)stopwatch.ElapsedMilliseconds;
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
                string output = doeladres + " Dictionary " + Program.Connecties[i] + " " + Program.Buren[Program.Connecties[i]].ping;
                Console.WriteLine(output);
                Program.Buren[doeladres].Write.WriteLine(output);
            }
        }
    }
}
