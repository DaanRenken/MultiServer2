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

        // Derde constructor, gebruikt voor ???
        // Bedoeld om streamreader/streamwriter eruit te slopen
        public Connection(int doeladres, int favopoort, StreamReader read)
        {
            this.Read = read;
            this.eigenadres = Program.MijnPoort;
            this.doeladres = doeladres;
            this.favopoort = favopoort;

            Thread thread = new Thread(ReaderThread);
            threads.Add(thread);
            thread.Start();
        }

        // LET OP: Nadat er verbinding is gelegd, kun je vergeten wie er client/server is (en dat kun je aan het Connection-object dus ook niet zien!)

        //private void Timer(object o, EventArgs e)
        //{
        //    Ping(doeladres);
        //}


        // Deze loop leest wat er binnenkomt en print dit
        public void ReaderThread()
        {
            //timer = new System.Timers.Timer();
            //timer.Elapsed += new System.Timers.ElapsedEventHandler(Timer);
            //timer.Interval = 60000;
            //timer.Enabled = true;

            try
            {
                while (true)
                {
                    string message = Read.ReadLine();
                    //Console.WriteLine(message);
                    // als een bericht binnenkomt wat voor de eigen poort bedoeld is, wordt er gekeken wat de opdracht is
                    if (message.StartsWith(eigenadres.ToString()))
                    {
                        message = message.Substring(message.IndexOf(" ") + 1);
                        // bij "ping ping" wordt "ping pong" teruggestuurd (basically een testbericht)
                        if (message.StartsWith("ping ping"))
                        {
                            SendMessage(Int32.Parse(message.Split()[2]), eigenadres, "ping pong");
                        }
                        // bij "ping pong" is er eerst "ping ping" gestuurd en krijg je die nu terug
                        else if (message.StartsWith("ping pong"))
                        {
                            lock (o)
                            {
                                Program.Buren[Int32.Parse(message.Split()[2])].stop = false;
                            }
                        }
                        // bij "GetDictionary" wordt de dictionary gestuurd naar iedere neighbor
                        else if (message.StartsWith("GetDictionary"))
                        {
                            SendDictionary();
                        }
                        // bij "Dictionary" wordt een nieuwe connection aan de dictonary toegevoegd
                        else if (message.StartsWith("Dictionary"))
                        {
                            UpdateDictionary(message.Substring(message.IndexOf(" ") + 1));
                        }
                        // bij "Remove Connection" wordt de connectie tussen deze en de betreffende poort verwijderd
                        // vanuit RemoveConnection wordt vervolgens "Removed Connection" naar alle buren gestuurd
                        else if (message.StartsWith("Remove Connection"))
                        {
                            Console.WriteLine(message);
                            int poort = Int32.Parse(message.Split()[2]);
                            Program.RemoveConnection(poort);
                        }
                        // bij "Removed Connection" is er een connectie verdwenen en updaten alle buren hun dictionary en sturen die naar hun buren
                        else if (message.StartsWith("Removed Connection"))
                        {
                            Console.WriteLine(message);
                            int poort = Int32.Parse(message.Split()[2]);
                            //if (this.doeladres == this.favopoort)
                            {
                                // !!
                                // wat gebeurt er hier precies?
                                // !!
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
                        // als het geen van die dingen is, wordt de message gewoon geprint
                        else
                        {
                            Console.WriteLine(message);
                        }
                    }
                    // als het bericht niet voor deze poort is, wordt het doorgestuurd naar degene voor wie het wel bestemd is
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

        // UpdateDictionary voegt een nieuwe connection aan de dictionary toe, mocht deze nog niet bestaan
        void UpdateDictionary(string input)
        {
            int poort = Int32.Parse(input.Split()[0]);
            if (!Program.Buren.ContainsKey(poort) && poort != eigenadres)
            {
                Connection connection = new Connection(this.Read, this.Write, poort);
                connection.favopoort = this.favopoort;
                Program.AddConnection(connection);
                //connection.ping = this.ping + Int32.Parse(input.Split()[1]);             
            }

        }

        // Alle SendMessage functies sturen messages, sturen ze door of ontvangen ze en writen ze
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

        // Ping is een testfunctie die de tijd tussen twee poorten meet
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
            }
            else
            {
                //Console.WriteLine(poort + " ping timed out");
            }

        }

        // lock boolean
        bool Stop()
        {
            lock (o)
            {
                return stop;
            }
        }

        // Alle SendDictionary functies sturen de dictionary van de poort door naar hun neighbors, of printen een ontvangen dictionary
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
