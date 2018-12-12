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

            // Start het reader-loopje

            Thread thread = new Thread(ReaderThread);
            threads.Add(thread);
            thread.Start();     
        }

        // LET OP: Nadat er verbinding is gelegd, kun je vergeten wie er client/server is (en dat kun je aan het Connection-object dus ook niet zien!)
        private void Timer(object o, EventArgs e)
        {
            Ping(doeladres);
        }


        // Deze loop leest wat er binnenkomt en print dit
        public void ReaderThread()
        {
            timer = new System.Timers.Timer();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer);
            timer.Interval = 60000;
            //timer.Enabled = true;
            try
            {
                while (true)
                {
                    string message = Read.ReadLine();
                    //Console.WriteLine(message);
                    if (message.StartsWith(eigenadres.ToString()))
                    {

                        message = message.Substring(message.IndexOf(" ") + 1);
                        if (message.StartsWith("ping ping"))
                        {
                            SendMessage(Int32.Parse(message.Split()[2]), eigenadres, "ping pong");
                        }
                        else if (message.StartsWith("ping pong"))
                        {
                            lock (o)
                            {
                                Program.Buren[Int32.Parse(message.Split()[2])].stop = false;
                            }
                        }
                        else if (message.StartsWith("GetDictionary"))
                        {
                            SendDictionary();
                        }
                        else if (message.StartsWith("Dictionary"))
                        {
                            UpdateDictionary(message.Substring(message.IndexOf(" ") + 1));
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
                            Console.WriteLine("Bericht voor " + voor + " doorgestuurd naar " + naar);
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
            if (!Program.Buren.ContainsKey(poort) && poort != eigenadres)
            {
                Connection connection = new Connection(this.Read, this.Write, poort);
                Program.AddConnection(connection);
                connection.favopoort = this.favopoort;
                connection.ping = this.ping + Int32.Parse(input.Split()[1]);
                //connection.Ping(connection.doeladres);               
            }

        }
        public void SendMessage(string message)
        {
            message = doeladres + " " + message;
            Write.WriteLine(message);
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
        public void Ping(int poort)
        {
            stop = true;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            SendMessage(poort, eigenadres, "ping ping");
            int i = 0;
            while (Stop() && stopwatch.ElapsedMilliseconds < 5000)
            {
                i++;
            }
            stopwatch.Stop(); 
            if (!Stop())
            {
                ping = (int)stopwatch.ElapsedMilliseconds;
                //Console.WriteLine(doeladres + " ping is " + ping);
            }
            else
            {
                Console.WriteLine(poort + " ping timed out");
            }

        }
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
                //if (Program.Connecties[i] != eigenadres)
                {
                    string output = "Dictionary " + Program.Connecties[i] + " " + Program.Buren[Program.Connecties[i]].ping;
                    Console.WriteLine(output);
                    SendMessage(output);
                }
            }
        }
        public void SendDictionary(List<Connection> Neigbours, int connectie)
        {
            string output = "Dictionary " + connectie + " " + Program.Buren[connectie].ping;
            for (int i = 0; i < Neigbours.Count; i++)
            {
                Neigbours[i].SendMessage(output);
            }
        }

        public List<Connection> GetNeigbours()
        {
            List<Connection> neigbours = new List<Connection>();
            for (int i = 0; i < Program.Connecties.Count; i++)
            {
                Connection connectie = Program.Buren[Program.Connecties[i]];
                if (connectie.doeladres == connectie.favopoort)
                {
                    neigbours.Add(connectie);
                }
            }
            return neigbours;
        }
    }
}
