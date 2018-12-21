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
        private System.Timers.Timer timer;
        // Connection heeft 2 constructoren: deze constructor wordt gebruikt als wij CLIENT worden bij een andere SERVER
        public Connection(int poort)
        {
            TcpClient client = new TcpClient("localhost", poort);
            Read = new StreamReader(client.GetStream());
            Write = new StreamWriter(client.GetStream());
            Write.AutoFlush = true;

            // De server kan niet zien van welke poort wij client zijn, dit moeten we apart laten weten
            this.eigenadres = Program.MijnPoort;
            this.doeladres = poort;
            this.favopoort = poort;
            this.ping = 1;
            Write.WriteLine("Poort: " + eigenadres);

            // Start het reader-loopje
            Thread thread = new Thread(ReaderThread);
            threads.Add(thread);
            thread.Start();
        }

        // Deze constructor wordt gebruikt als wij SERVER zijn en een CLIENT maakt met ons verbinding
        public Connection(StreamReader read, StreamWriter write, int poort)
        {
            Read = read; Write = write;

            this.eigenadres = Program.MijnPoort;
            this.doeladres = poort;
            this.favopoort = poort;
            this.ping = 1;
            // Start het reader-loopje

            Thread thread = new Thread(ReaderThread);
            threads.Add(thread);
            thread.Start();
        }
        public Connection(int doeladres, int favopoort, int ping)
        {
            this.eigenadres = Program.MijnPoort;
            this.doeladres = doeladres;
            this.favopoort = favopoort;
            this.ping = ping;
        }

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
                        //if (message.StartsWith("ping ping"))
                        //{
                        //    SendMessage(Int32.Parse(message.Split()[2]), eigenadres, "ping pong");
                        //}
                        //else if (message.StartsWith("ping pong"))
                        //{
                        //    lock (o)
                        //    {
                        //        Program.Buren[Int32.Parse(message.Split()[2])].stop = false;
                        //    }
                        //}
                        if (message.StartsWith("GetDictionary"))
                        {
                            SendDictionary();
                        }
                        else if (message.StartsWith("Dictionary"))
                        {
                            UpdateDictionary(message.Substring(message.IndexOf(" ") + 1));
                        }
                        else if (message.StartsWith("Remove Connection"))
                        {
                            Console.WriteLine(message);
                            int poort = Int32.Parse(message.Split()[2]);
                            Program.RemoveConnection(poort);
                        }
                        else if (message.StartsWith("Removed Connection"))
                        {
                            Console.WriteLine(message);
                            int poort = Int32.Parse(message.Split()[2]);
                            //if (this.doeladres == this.favopoort)
                            {
                                if (Program.Buren[poort].favopoort != Int32.Parse(message.Split()[3]))
                                {
                                    SendDictionary(this, poort);
                                }
                                else if (Program.Buren[poort].favopoort == this.favopoort)
                                {
                                    Program.RemoveConnection(poort);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine(message);
                        }
                    }
                    else
                    {
                        lock (o)
                        {
                            int voor = Int32.Parse(message.Split()[0]);
                            int naar = Program.Buren[voor].favopoort;
                            SendMessage(Int32.Parse(message.Split()[0]), message);
                        }
                    }
                }
            }
            catch { } // Verbinding is kennelijk verbroken
        }
        void UpdateDictionary(string input)
        {
            int poort = Int32.Parse(input.Split()[0]);
            int ping2 = Int32.Parse(input.Split()[1]);
            if (!Program.Buren.ContainsKey(poort) && poort != eigenadres)
            {
                Console.WriteLine("start new connection" + this.doeladres + " " + this.favopoort + " " + this.ping + " " + poort);
                Connection connection = new Connection(poort, this.favopoort, ping2 + 1);
                Program.AddConnection(connection);
            }

        }
        public void SendMessage(string message)
        {
            message = doeladres + " " + message;
            if (this.doeladres == favopoort)
            {
                Write.WriteLine(message);
            }
            else
            {
                SendMessage(favopoort, message);
            }
        }
        public void SendMessage(int naarpoort, string message)
        {
            Program.Buren[naarpoort].Write.WriteLine(message);
        }
        public void SendMessage(int naarpoort, int vanpoort, string message)
        {
            message = naarpoort + " " + message + " " + vanpoort;
            SendMessage(naarpoort, message);
        }
        //public void Ping(int poort)
        //{
        //    stop = true;
        //    Stopwatch stopwatch = new Stopwatch();
        //    stopwatch.Start();
        //    SendMessage(poort, eigenadres, "ping ping");
        //    int i = 0;
        //    while (Stop() && stopwatch.ElapsedMilliseconds < 5000)
        //    {
        //        i++;
        //    }
        //    stopwatch.Stop();
        //    if (!Stop())
        //    {
        //        ping = (int)stopwatch.ElapsedMilliseconds;
        //    }
        //    else
        //    {
        //        //Console.WriteLine(poort + " ping timed out");
        //    }

        //}
        bool Stop()
        {
            lock (o)
            {
                return stop;
            }
        }

        public void SendDictionary()
        {
            for (int i = 0; i < Program.Connecties.Count; i++)
            {
                {
                    SendDictionary(this, Program.Connecties[i]);
                }
            }
        }
        public void SendDictionary(Connection Neigbour, int connectie)
        {
            string output = "Dictionary " + connectie + " " + Program.Buren[connectie].ping;
            Neigbour.SendMessage(output);
        }
        public void SendDictionary(List<Connection> Neigbours, int connectie)
        {

            for (int i = 0; i < Neigbours.Count; i++)
            {
                SendDictionary(Neigbours[i], connectie);
            }
        }


    }
}
